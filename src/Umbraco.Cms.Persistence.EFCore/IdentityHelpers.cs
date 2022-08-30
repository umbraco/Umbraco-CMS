using Microsoft.EntityFrameworkCore;

namespace Umbraco.Extensions;

public static class IdentityHelpers
{
    public static Task EnableIdentityInsert<T>(this DbContext context) => SetIdentityInsert<T>(context, enable: true);
    public static Task DisableIdentityInsert<T>(this DbContext context) => SetIdentityInsert<T>(context, enable: false);

    private static Task SetIdentityInsert<T>(DbContext context, bool enable)
    {
        if (context.Database.IsSqlite())
        {
            return Task.CompletedTask;
        }

        var entityType = context.Model.FindEntityType(typeof(T));
        if (entityType is null)
        {
            throw new NotSupportedException("T needs to ne an EF Core entity");
        }
        var value = enable ? "ON" : "OFF";
        return context.Database.ExecuteSqlRawAsync(
            $"SET IDENTITY_INSERT {entityType.GetSchema()}.{entityType.GetTableName()} {value}");
    }

    public static void SaveChangesWithIdentityInsert<T>(this DbContext context)
    {
        using var transaction = context.Database.BeginTransaction();
        context.EnableIdentityInsert<T>();
        context.SaveChanges();
        context.DisableIdentityInsert<T>();
        transaction.Commit();
    }

}
