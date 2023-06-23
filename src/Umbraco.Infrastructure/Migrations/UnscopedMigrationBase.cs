using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Migrations;

/// <summary>
/// Base class for creating a migration that does not have a scope provided for it.
/// </summary>
public abstract class UnscopedMigrationBase : MigrationBase
{
    protected UnscopedMigrationBase(IMigrationContext context)
        : base(context)
    {
    }

    /// <summary>
    /// <para>Scope the database used by the migration builder.</para>
    /// <para>This is used with <see cref="UnscopedMigrationBase"/> when you need to execute something before the scope is created
    /// but later need to have your queries scoped in a transaction.</para>
    /// </summary>
    /// <param name="scope">The scope to get the database from.</param>
    /// <exception cref="InvalidOperationException">If the migration is missing or has a malformed MigrationContext, this exception is thrown.</exception>
    protected void ScopeDatabase(IScope scope)
    {
        if (Context is not MigrationContext context)
        {
            throw new InvalidOperationException("Cannot scope database because context is not a MigrationContext");
        }

        context.Database = scope.Database;
    }
}
