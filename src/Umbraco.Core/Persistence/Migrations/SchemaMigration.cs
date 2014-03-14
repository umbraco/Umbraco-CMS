namespace Umbraco.Core.Persistence.Migrations
{
    /// <summary>
    /// A migration class that specifies that it is used for db schema migrations only - these need to execute first and MUST
    /// have a downgrade plan.
    /// </summary>
    public abstract class SchemaMigration : MigrationBase
    {

    }
}