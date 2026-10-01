using EInvoice.Application.Common;

namespace EInvoice.Application.Users;

public interface IUserManagementService
{
    Task<IReadOnlyList<UserListItemDto>> GetUsersAsync();

    Task<UserDetailDto?> GetUserByIdAsync(Guid id);

    Task<ServiceResult<Guid>> CreateUserAsync(CreateUserDto dto);

    Task<ServiceResult> UpdateUserAsync(UpdateUserDto dto);

    Task<ServiceResult> SetActiveStatusAsync(Guid userId, bool isActive);

    Task<ServiceResult> ResetPasswordAsync(Guid userId, string newPassword);
}
