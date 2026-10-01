using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using EInvoice.Application.Common;
using EInvoice.Application.Roles;
using EInvoice.Domain.Authorization;
using EInvoice.Infrastructure.Authorization;
using EInvoice.Infrastructure.Identity;

namespace EInvoice.Infrastructure.Services;

public class RoleManagementService : IRoleManagementService
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public RoleManagementService(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<RoleListItemDto>> GetRolesAsync()
    {
        var roles = _roleManager.Roles.OrderBy(r => r.Name).ToList();
        var result = new List<RoleListItemDto>(roles.Count);

        foreach (var role in roles)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
            result.Add(new RoleListItemDto
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty,
                Description = role.Description,
                UserCount = usersInRole.Count,
                IsSystemRole = Roles.All.Contains(role.Name ?? string.Empty),
            });
        }

        return result;
    }

    public async Task<RoleDetailDto?> GetRoleByIdAsync(Guid id)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role is null)
        {
            return null;
        }

        var claims = await _roleManager.GetClaimsAsync(role);
        return new RoleDetailDto
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty,
            Description = role.Description,
            Permissions = claims.Where(c => c.Type == PermissionClaimTypes.Permission).Select(c => c.Value).ToList(),
            IsSystemRole = Roles.All.Contains(role.Name ?? string.Empty),
        };
    }

    public async Task<ServiceResult<Guid>> CreateRoleAsync(SaveRoleDto dto)
    {
        if (await _roleManager.RoleExistsAsync(dto.Name))
        {
            return ServiceResult<Guid>.Failure("A role with this name already exists.");
        }

        var role = new ApplicationRole(dto.Name, dto.Description);
        var createResult = await _roleManager.CreateAsync(role);
        if (!createResult.Succeeded)
        {
            return ServiceResult<Guid>.Failure(createResult.Errors.Select(e => e.Description));
        }

        var claimResult = await SetPermissionsAsync(role, dto.Permissions);
        if (!claimResult.Succeeded)
        {
            return ServiceResult<Guid>.Failure(claimResult.Errors);
        }

        return ServiceResult<Guid>.Success(role.Id);
    }

    public async Task<ServiceResult> UpdateRoleAsync(SaveRoleDto dto)
    {
        if (dto.Id is null)
        {
            return ServiceResult.Failure("Role id is required.");
        }

        var role = await _roleManager.FindByIdAsync(dto.Id.Value.ToString());
        if (role is null)
        {
            return ServiceResult.Failure("Role not found.");
        }

        var isSystemRole = Roles.All.Contains(role.Name ?? string.Empty);
        if (!isSystemRole)
        {
            role.Name = dto.Name;
            role.NormalizedName = dto.Name.ToUpperInvariant();
        }

        role.Description = dto.Description;

        var updateResult = await _roleManager.UpdateAsync(role);
        if (!updateResult.Succeeded)
        {
            return ServiceResult.Failure(updateResult.Errors.Select(e => e.Description));
        }

        return await SetPermissionsAsync(role, dto.Permissions);
    }

    public async Task<ServiceResult> DeleteRoleAsync(Guid id)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role is null)
        {
            return ServiceResult.Failure("Role not found.");
        }

        if (Roles.All.Contains(role.Name ?? string.Empty))
        {
            return ServiceResult.Failure("Built-in roles cannot be deleted.");
        }

        var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name!);
        foreach (var user in usersInRole)
        {
            var removeResult = await _userManager.RemoveFromRoleAsync(user, role.Name!);
            if (!removeResult.Succeeded)
            {
                return ServiceResult.Failure(removeResult.Errors.Select(e => e.Description));
            }
        }

        var result = await _roleManager.DeleteAsync(role);
        return result.Succeeded
            ? ServiceResult.Success()
            : ServiceResult.Failure(result.Errors.Select(e => e.Description));
    }

    private async Task<ServiceResult> SetPermissionsAsync(ApplicationRole role, IReadOnlyCollection<string> permissions)
    {
        var validPermissions = new HashSet<string>(Permissions.GetAllValues(), StringComparer.Ordinal);
        var existingClaims = (await _roleManager.GetClaimsAsync(role))
            .Where(c => c.Type == PermissionClaimTypes.Permission)
            .ToList();

        foreach (var claim in existingClaims)
        {
            var removeResult = await _roleManager.RemoveClaimAsync(role, claim);
            if (!removeResult.Succeeded)
            {
                return ServiceResult.Failure(removeResult.Errors.Select(e => e.Description));
            }
        }

        foreach (var permission in permissions.Where(validPermissions.Contains).Distinct())
        {
            var addResult = await _roleManager.AddClaimAsync(role, new Claim(PermissionClaimTypes.Permission, permission));
            if (!addResult.Succeeded)
            {
                return ServiceResult.Failure(addResult.Errors.Select(e => e.Description));
            }
        }

        return ServiceResult.Success();
    }
}
