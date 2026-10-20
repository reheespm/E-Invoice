using System.Linq.Expressions;
using EInvoice.Application.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace EInvoice.Infrastructure.Persistence;

/// <summary>
/// Blocks deleting a record while other records still reference it. The references are read from the EF model
/// (every non-cascading foreign key pointing at the entity), so new tables such as invoices are covered automatically.
/// </summary>
public static class DeleteGuard
{
    // Friendly names for referencing tables; anything not listed falls back to the table name.
    private static readonly Dictionary<string, (string One, string Many)> Labels = new()
    {
        ["Companies"] = ("company", "companies"),
        ["Customers"] = ("customer", "customers"),
        ["Cities"] = ("city", "cities"),
        ["UserCompanies"] = ("user assignment", "user assignments"),
        ["UserRoles"] = ("user", "users"),
    };

    /// <summary>Returns an error message listing what still references the record, or null when it can be deleted.</summary>
    public static async Task<string?> FindReferencesAsync<TEntity>(DbContext db, object key, string displayName)
        where TEntity : class
    {
        var entityType = db.Model.FindEntityType(typeof(TEntity))
            ?? throw new InvalidOperationException($"{typeof(TEntity).Name} is not part of the model.");

        var usages = new List<string>();
        foreach (var foreignKey in entityType.GetReferencingForeignKeys())
        {
            if (foreignKey.DeleteBehavior is DeleteBehavior.Cascade or DeleteBehavior.ClientCascade)
            {
                continue; // owned details (e.g. a role's permissions) are deleted with the record
            }

            var count = await CountAsync(db, foreignKey.DeclaringEntityType.ClrType, foreignKey.Properties.Single(), key);
            if (count > 0)
            {
                var table = foreignKey.DeclaringEntityType.GetTableName() ?? foreignKey.DeclaringEntityType.ClrType.Name;
                var (one, many) = Labels.TryGetValue(table, out var label) ? label : (table, table);
                usages.Add($"{count} {(count == 1 ? one : many)}");
            }
        }

        if (usages.Count == 0)
        {
            return null;
        }

        var list = usages.Count == 1 ? usages[0] : $"{string.Join(", ", usages[..^1])} and {usages[^1]}";
        return $"Cannot delete '{displayName}': it is still used by {list}. Remove or reassign them first.";
    }

    /// <summary>Deletes the record only when nothing references it.</summary>
    public static async Task<ServiceResult> DeleteAsync<TEntity>(DbContext db, TEntity entity, object key, string displayName)
        where TEntity : class
    {
        var blocked = await FindReferencesAsync<TEntity>(db, key, displayName);
        if (blocked is not null)
        {
            return ServiceResult.Failure(blocked);
        }

        db.Remove(entity);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // A reference was added between the check and the delete; the database's restrict rule refused it.
            // Undo the pending delete so it is not retried by the next save in this (circuit-scoped) context.
            db.Entry(entity).State = EntityState.Unchanged;
            return ServiceResult.Failure($"Cannot delete '{displayName}': other records still reference it.");
        }

        return ServiceResult.Success();
    }

    private static Task<int> CountAsync(DbContext db, Type dependentType, Microsoft.EntityFrameworkCore.Metadata.IProperty property, object key)
    {
        // db.Set<TDependent>().Count(e => EF.Property<TProperty>(e, "Fk") == key)
        var set = (IQueryable)typeof(DbContext).GetMethod(nameof(DbContext.Set), Type.EmptyTypes)!
            .MakeGenericMethod(dependentType)
            .Invoke(db, null)!;

        var parameter = Expression.Parameter(dependentType, "e");
        var predicate = Expression.Lambda(
            Expression.Equal(
                Expression.Call(typeof(EF), nameof(EF.Property), [property.ClrType], parameter, Expression.Constant(property.Name)),
                Expression.Constant(key, property.ClrType)),
            parameter);

        var count = Expression.Call(
            typeof(Queryable), nameof(Queryable.Count), [dependentType],
            Expression.Call(typeof(Queryable), nameof(Queryable.Where), [dependentType], set.Expression, Expression.Quote(predicate)));

        return ((IAsyncQueryProvider)set.Provider).ExecuteAsync<Task<int>>(count);
    }
}
