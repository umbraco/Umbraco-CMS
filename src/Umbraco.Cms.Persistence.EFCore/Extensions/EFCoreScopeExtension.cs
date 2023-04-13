using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Umbraco.Extensions;

internal static class DbContextExtension
{
    public static async Task MigrateDatabaseAsync(this DbContext context, string targetMigration) =>
        await context.GetService<IMigrator>().MigrateAsync(targetMigration);
}
