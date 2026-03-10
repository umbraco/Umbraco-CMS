using Microsoft.Extensions.Logging;
using NPoco;
using static Umbraco.Cms.Core.Constants;
using ColumnInfo = Umbraco.Cms.Infrastructure.Persistence.SqlSyntax.ColumnInfo;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_0_1;

/// <summary>
/// Ensures the propertyTypeId column in umbracoPropertyData has correct camel case naming.
/// </summary>
/// <remarks>
/// SQL Server is case sensitive for columns used in a SQL Bulk insert statement (which is used in publishing
/// operations on umbracoPropertyData).
/// Earlier versions of Umbraco used all lower case for the propertyTypeId column name (propertytypeid), whereas newer versions
/// use camel case (propertyTypeId).
/// </remarks>
public class EnsureUmbracoPropertyDataColumnCasing : AsyncMigrationBase
{
    private readonly ILogger<EnsureUmbracoPropertyDataColumnCasing> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnsureUmbracoPropertyDataColumnCasing"/> class.
    /// </summary>
    public EnsureUmbracoPropertyDataColumnCasing(IMigrationContext context, ILogger<EnsureUmbracoPropertyDataColumnCasing> logger)
        : base(context) => _logger = logger;

    /// <inheritdoc/>
    protected override Task MigrateAsync()
    {
        // We only need to do this for SQL Server.
        if (DatabaseType == DatabaseType.SQLite)
        {
            return Task.CompletedTask;
        }

        const string oldColumnName = "propertytypeid";
        const string newColumnName = "propertyTypeId";
        ColumnInfo[] columns = [.. SqlSyntax.GetColumnsInSchema(Context.Database)];
        ColumnInfo? targetColumn = columns
            .FirstOrDefault(x => x.TableName == DatabaseSchema.Tables.PropertyData && string.Equals(x.ColumnName, oldColumnName, StringComparison.InvariantCulture));
        if (targetColumn is not null)
        {
            // The column exists with incorrect casing, we need to rename it.
            Rename.Column(oldColumnName)
                .OnTable(DatabaseSchema.Tables.PropertyData)
                .To(newColumnName)
                .Do();

            _logger.LogInformation("Renamed column {OldColumnName} to {NewColumnName} on table {TableName}", oldColumnName, newColumnName, DatabaseSchema.Tables.PropertyData);
        }

        return Task.CompletedTask;
    }
}
