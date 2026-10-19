using EInvoice.Application.Common;

namespace EInvoice.Application.Customers;

public interface ICustomerManagementService
{
    Task<IReadOnlyList<CustomerListItemDto>> GetCustomersAsync();

    Task<CustomerDetailDto?> GetCustomerByIdAsync(Guid id);

    Task<ServiceResult<Guid>> CreateCustomerAsync(SaveCustomerDto dto);

    Task<ServiceResult> UpdateCustomerAsync(SaveCustomerDto dto);

    Task<ServiceResult> DeleteCustomerAsync(Guid id);
}
