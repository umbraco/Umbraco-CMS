using NPoco;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_1_0;

/// <summary>
/// Ensures the propertyTypeId column in umbracoPropertyData has correct camel case naming.
/// </summary>
public class EnsureUmbracoPropertyDataColumnCasing : AsyncMigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnsureUmbracoPropertyDataColumnCasing"/> class.
    /// </summary>
    public EnsureUmbracoPropertyDataColumnCasing(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc/>
    protected override async Task MigrateAsync()
    {
        if (DatabaseType == DatabaseType.SQLite)
        {
            return;
        }

        // SQL Server is case sensitive for columns used in a SQL Bulk insert statement (which is used in publishing
        // operations on umbracoPropertyData).
        // Earlier versions of Umbraco used all lower case for the propertyTypeId column name (propertytypeid), whereas newer versions
        // use camel case (propertyTypeId).
        Sql<ISqlContext> sql = Database.SqlContext.Sql(
            "EXEC sp_rename N'umbracoPropertyData.propertytypeid', N'propertyTypeId', N'COLUMN'");
        await Database.ExecuteAsync(sql);
    }
}
