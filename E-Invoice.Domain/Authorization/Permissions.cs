using System.Reflection;

namespace EInvoice.Domain.Authorization;

/// <summary>
/// Central registry of every permission in the system. Add a new nested static class
/// per module and it is automatically discovered by <see cref="GetAll"/> - no other
/// wiring is required for it to show up in the role/permission management UI.
/// </summary>
public static class Permissions
{
    public static class Dashboard
    {
        public const string View = "Dashboard.View";
    }

    public static class User
    {
        public const string View = "User.View";
        public const string Create = "User.Create";
        public const string Edit = "User.Edit";
        public const string Delete = "User.Delete";
    }

    public static class Role
    {
        public const string View = "Role.View";
        public const string Create = "Role.Create";
        public const string Edit = "Role.Edit";
        public const string Delete = "Role.Delete";
    }

    public static class Company
    {
        public const string View = "Company.View";
        public const string Create = "Company.Create";
        public const string Edit = "Company.Edit";
        public const string Delete = "Company.Delete";
    }

    public static class Country
    {
        public const string View = "Country.View";
        public const string Create = "Country.Create";
        public const string Edit = "Country.Edit";
        public const string Delete = "Country.Delete";
    }

    public static class City
    {
        public const string View = "City.View";
        public const string Create = "City.Create";
        public const string Edit = "City.Edit";
        public const string Delete = "City.Delete";
    }

    public static IReadOnlyList<PermissionInfo> GetAll()
    {
        var result = new List<PermissionInfo>();

        foreach (var moduleType in typeof(Permissions).GetNestedTypes(BindingFlags.Public | BindingFlags.Static))
        {
            foreach (var field in moduleType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
            {
                if (field.FieldType != typeof(string) || !field.IsLiteral)
                {
                    continue;
                }

                var value = (string)field.GetRawConstantValue()!;
                result.Add(new PermissionInfo(moduleType.Name, field.Name, value));
            }
        }

        return result.OrderBy(p => p.Module, StringComparer.Ordinal).ThenBy(p => p.Name, StringComparer.Ordinal).ToList();
    }

    public static IReadOnlyList<string> GetAllValues() => GetAll().Select(p => p.Value).ToList();
}

public sealed record PermissionInfo(string Module, string Name, string Value);
