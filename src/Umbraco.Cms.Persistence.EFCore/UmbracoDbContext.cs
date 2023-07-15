using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Persistence.EFCore;

/// <remarks>
/// To autogenerate migrations use the following commands
/// and insure the 'src/Umbraco.Web.UI/appsettings.json' have a connection string set with the right provider.
///
/// Add a migration for each provider
///
/// <code>dotnet ef migrations add %Name% -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.SqlServer -c UmbracoDbContext  -- --provider SqlServer</code>
/// <code>dotnet ef migrations add %Name% -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.Sqlite -c UmbracoDbContext  -- --provider Sqlite</code>
///
/// Remove the last migration for each provider
///
/// <code>dotnet ef migrations remove -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.SqlServer -c UmbracoDbContext -- --provider SqlServer</code>
///
/// <code>dotnet ef migrations remove -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.Sqlite -c UmbracoDbContext -- --provider Sqlite</code>
/// 
/// To find documentation about this way of working with the context see
/// https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/providers?tabs=dotnet-core-cli#using-one-context-type
/// </remarks>
public class UmbracoDbContext : DbContext
{
    public UmbracoDbContext(DbContextOptions<UmbracoDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (IMutableEntityType entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(Constants.DatabaseSchema.TableNamePrefix + entity.GetTableName());
        }
    }
}
