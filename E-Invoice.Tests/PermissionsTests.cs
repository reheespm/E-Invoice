using EInvoice.Domain.Authorization;

namespace EInvoice.Tests;

public class PermissionsTests
{
    [Fact]
    public void CustomerPermissions_ShouldBeRegistered()
    {
        var permissions = Permissions.GetAllValues();

        Assert.Contains(Permissions.Customer.View, permissions);
        Assert.Contains(Permissions.Customer.Create, permissions);
        Assert.Contains(Permissions.Customer.Edit, permissions);
        Assert.Contains(Permissions.Customer.Delete, permissions);
    }
}
