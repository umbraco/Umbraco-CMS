using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Migrations.Install;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.Common;

/// <summary>
/// Represents a migration step that creates database keys and indexes during an upgrade.
/// </summary>
public class CreateKeysAndIndexes : MigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateKeysAndIndexes"/> class using the specified migration context.
    /// This constructor sets up the migration for creating keys and indexes during an upgrade.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> that provides information and services for the migration.</param>
    public CreateKeysAndIndexes(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        // remove those that may already have keys
        Delete.KeysAndIndexes(Constants.DatabaseSchema.Tables.KeyValue).Do();
        Delete.KeysAndIndexes(Constants.DatabaseSchema.Tables.PropertyData).Do();

        // re-create *all* keys and indexes
        foreach (Type x in DatabaseSchemaCreator._orderedTables)
        {
            Create.KeysAndIndexes(x).Do();
        }
    }
}
