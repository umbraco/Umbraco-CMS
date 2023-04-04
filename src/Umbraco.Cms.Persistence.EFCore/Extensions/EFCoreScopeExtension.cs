using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Umbraco.Cms.Persistence.EFCore.Entities;
using Umbraco.Cms.Persistence.EFCore.Scoping;

namespace Umbraco.Extensions;

public static class EFCoreScopeExtension
{
    public static async Task MigrateDatabaseAsync(this IEfCoreScope<UmbracoEFContext> scope, string targetMigration) =>
        await scope.ExecuteWithContextAsync<Task>(async db =>
        {
            await db.GetService<IMigrator>().MigrateAsync(targetMigration);
        });
}
