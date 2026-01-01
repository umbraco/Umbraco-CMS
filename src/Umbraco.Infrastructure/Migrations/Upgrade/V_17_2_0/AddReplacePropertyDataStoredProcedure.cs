using NPoco;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_2_0;

/// <summary>
/// Creates a Table-Valued Type and stored procedure for efficient property data replacement.
/// Uses MERGE statement for atomic UPDATE/INSERT/DELETE in a single round trip.
/// This is SQL Server specific.
/// </summary>
/// <remarks>
/// Note that it's necessary to escape the @ characters indicating parameters, to ensure NPoco doesn't interpret them as
/// parameters for the SQL statement.
/// </remarks>
public class AddReplacePropertyDataStoredProcedure : AsyncMigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddReplacePropertyDataStoredProcedure"/> class.
    /// </summary>
    /// <param name="context"></param>
    public AddReplacePropertyDataStoredProcedure(IMigrationContext context)
        : base(context)
    {
    }

    /// <inheritdoc/>
    protected override async Task MigrateAsync()
    {
        // Only run for SQL Server.
        if (DatabaseType == DatabaseType.SQLite)
        {
            return;
        }

        Install.AdditionalSchema.CreateOperations.CreateSchemaForPropertyDataReplacementOperation(Database);
    }
}
