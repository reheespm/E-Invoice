using EInvoice.Application.Common;

namespace EInvoice.Application.Roles;

public interface IRoleManagementService
{
    Task<IReadOnlyList<RoleListItemDto>> GetRolesAsync();

    Task<RoleDetailDto?> GetRoleByIdAsync(Guid id);

    Task<ServiceResult<Guid>> CreateRoleAsync(SaveRoleDto dto);

    Task<ServiceResult> UpdateRoleAsync(SaveRoleDto dto);

    Task<ServiceResult> DeleteRoleAsync(Guid id);
}
