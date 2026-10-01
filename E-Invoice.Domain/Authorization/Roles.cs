namespace EInvoice.Domain.Authorization;

public static class Roles
{
    public const string Administrator = "Administrator";
    public const string Manager = "Manager";
    public const string User = "User";

    public static readonly IReadOnlyList<string> All = new[] { Administrator, Manager, User };
}
