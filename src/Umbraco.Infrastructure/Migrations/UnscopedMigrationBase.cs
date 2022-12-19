namespace Umbraco.Cms.Infrastructure.Migrations;

/// <summary>
/// Base class for creating a migration that does not have a scope provided for it
/// </summary>
/// <remarks>
/// This is just a marker class, and has all the same functionality as the underlying MigrationBase
/// </remarks>
public abstract class UnscopedMigrationBase : MigrationBase
{
    protected UnscopedMigrationBase(IMigrationContext context) : base(context)
    {
    }
}
