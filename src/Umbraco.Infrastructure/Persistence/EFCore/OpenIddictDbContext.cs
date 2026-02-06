using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Umbraco.Cms.Infrastructure.Persistence.EFCore;

/// <summary>
/// A dedicated DbContext for OpenIddict runtime operations.
/// </summary>
/// <remarks>
/// OpenIddict's built-in EF Core stores rely on change tracking for CRUD operations,
/// so this context keeps the default tracking behavior enabled — unlike <see cref="UmbracoDbContext"/>
/// which disables change tracking globally.
/// </remarks>
public class OpenIddictDbContext : DbContext
{
    public OpenIddictDbContext(DbContextOptions<OpenIddictDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Match UmbracoDbContext's table prefixing for OpenIddict tables.
        foreach (IMutableEntityType entity in modelBuilder.Model.GetEntityTypes())
        {
            if (entity.ClrType.Assembly.FullName!.StartsWith("OpenIddict"))
            {
                entity.SetTableName(Core.Constants.DatabaseSchema.TableNamePrefix + entity.GetTableName());
            }
        }
    }
}
