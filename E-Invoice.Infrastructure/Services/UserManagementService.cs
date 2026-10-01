using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EInvoice.Application.Common;
using EInvoice.Application.Users;
using EInvoice.Infrastructure.Identity;
using EInvoice.Infrastructure.Persistence;

namespace EInvoice.Infrastructure.Services;

public class UserManagementService : IUserManagementService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly EInvoiceDbContext _dbContext;

    public UserManagementService(UserManager<ApplicationUser> userManager, EInvoiceDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<UserListItemDto>> GetUsersAsync()
    {
        var users = await _userManager.Users.OrderBy(u => u.UserName).ToListAsync();
        var result = new List<UserListItemDto>(users.Count);

        foreach (var user in users)
        {
            result.Add(await MapAsync(user));
        }

        return result;
    }

    public async Task<UserDetailDto?> GetUserByIdAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        var companyIds = await _dbContext.UserCompanies
            .Where(uc => uc.UserId == user.Id)
            .Select(uc => uc.CompanyId)
            .ToListAsync();
        var companyNames = await _dbContext.Companies
            .Where(c => companyIds.Contains(c.Id))
            .OrderBy(c => c.Name)
            .Select(c => c.Name)
            .ToListAsync();

        var reportedToUserName = user.ReportedToUserId is null
            ? null
            : await _userManager.Users
                .Where(u => u.Id == user.ReportedToUserId)
                .Select(u => u.UserName)
                .FirstOrDefaultAsync();

        return new UserDetailDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            IsLockedOut = await _userManager.IsLockedOutAsync(user),
            PhoneNumber = user.PhoneNumber,
            Roles = roles.ToList(),
            Companies = companyNames,
            CompanyIds = companyIds,
            SapId = user.SapId,
            PoornataId = user.PoornataId,
            ReportedToUserId = user.ReportedToUserId,
            ReportedToUserName = reportedToUserName,
        };
    }

    public async Task<ServiceResult<Guid>> CreateUserAsync(CreateUserDto dto)
    {
        if (dto.ReportedToUserId is not null && !await _userManager.Users.AnyAsync(u => u.Id == dto.ReportedToUserId))
        {
            return ServiceResult<Guid>.Failure("Selected 'Reported To' user does not exist.");
        }

        var user = new ApplicationUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            IsActive = true,
            EmailConfirmed = true,
            SapId = dto.SapId,
            PoornataId = dto.PoornataId,
            ReportedToUserId = dto.ReportedToUserId,
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            return ServiceResult<Guid>.Failure(createResult.Errors.Select(e => e.Description));
        }

        if (dto.Roles.Count > 0)
        {
            var roleResult = await _userManager.AddToRolesAsync(user, dto.Roles);
            if (!roleResult.Succeeded)
            {
                return ServiceResult<Guid>.Failure(roleResult.Errors.Select(e => e.Description));
            }
        }

        if (dto.CompanyIds.Count > 0)
        {
            foreach (var companyId in dto.CompanyIds.Distinct())
            {
                _dbContext.UserCompanies.Add(new UserCompany { UserId = user.Id, CompanyId = companyId });
            }

            await _dbContext.SaveChangesAsync();
        }

        return ServiceResult<Guid>.Success(user.Id);
    }

    public async Task<ServiceResult> UpdateUserAsync(UpdateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.Id.ToString());
        if (user is null)
        {
            return ServiceResult.Failure("User not found.");
        }

        if (dto.ReportedToUserId == dto.Id)
        {
            return ServiceResult.Failure("A user cannot report to themselves.");
        }

        if (dto.ReportedToUserId is not null && !await _userManager.Users.AnyAsync(u => u.Id == dto.ReportedToUserId))
        {
            return ServiceResult.Failure("Selected 'Reported To' user does not exist.");
        }

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Email = dto.Email;
        user.NormalizedEmail = dto.Email.ToUpperInvariant();
        user.PhoneNumber = dto.PhoneNumber;
        user.SapId = dto.SapId;
        user.PoornataId = dto.PoornataId;
        user.ReportedToUserId = dto.ReportedToUserId;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return ServiceResult.Failure(updateResult.Errors.Select(e => e.Description));
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        var rolesToRemove = currentRoles.Except(dto.Roles).ToList();
        var rolesToAdd = dto.Roles.Except(currentRoles).ToList();

        if (rolesToRemove.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            if (!removeResult.Succeeded)
            {
                return ServiceResult.Failure(removeResult.Errors.Select(e => e.Description));
            }
        }

        if (rolesToAdd.Count > 0)
        {
            var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addResult.Succeeded)
            {
                return ServiceResult.Failure(addResult.Errors.Select(e => e.Description));
            }
        }

        var currentCompanyIds = await _dbContext.UserCompanies
            .Where(uc => uc.UserId == user.Id)
            .Select(uc => uc.CompanyId)
            .ToListAsync();
        var companyIdsToRemove = currentCompanyIds.Except(dto.CompanyIds).ToList();
        var companyIdsToAdd = dto.CompanyIds.Except(currentCompanyIds).ToList();

        if (companyIdsToRemove.Count > 0)
        {
            var rowsToRemove = await _dbContext.UserCompanies
                .Where(uc => uc.UserId == user.Id && companyIdsToRemove.Contains(uc.CompanyId))
                .ToListAsync();
            _dbContext.UserCompanies.RemoveRange(rowsToRemove);
        }

        foreach (var companyId in companyIdsToAdd)
        {
            _dbContext.UserCompanies.Add(new UserCompany { UserId = user.Id, CompanyId = companyId });
        }

        if (companyIdsToRemove.Count > 0 || companyIdsToAdd.Count > 0)
        {
            await _dbContext.SaveChangesAsync();
        }

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> SetActiveStatusAsync(Guid userId, bool isActive)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return ServiceResult.Failure("User not found.");
        }

        user.IsActive = isActive;

        if (!isActive)
        {
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
        }
        else
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
        }

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded
            ? ServiceResult.Success()
            : ServiceResult.Failure(result.Errors.Select(e => e.Description));
    }

    public async Task<ServiceResult> ResetPasswordAsync(Guid userId, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return ServiceResult.Failure("User not found.");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        return result.Succeeded
            ? ServiceResult.Success()
            : ServiceResult.Failure(result.Errors.Select(e => e.Description));
    }

    private async Task<UserListItemDto> MapAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var companyNames = await _dbContext.UserCompanies
            .Where(uc => uc.UserId == user.Id)
            .Join(_dbContext.Companies, uc => uc.CompanyId, c => c.Id, (uc, c) => c.Name)
            .OrderBy(name => name)
            .ToListAsync();

        var reportedToUserName = user.ReportedToUserId is null
            ? null
            : await _userManager.Users
                .Where(u => u.Id == user.ReportedToUserId)
                .Select(u => u.UserName)
                .FirstOrDefaultAsync();

        return new UserListItemDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            IsLockedOut = await _userManager.IsLockedOutAsync(user),
            Roles = roles.ToList(),
            Companies = companyNames,
            ReportedToUserName = reportedToUserName,
        };
    }
}
