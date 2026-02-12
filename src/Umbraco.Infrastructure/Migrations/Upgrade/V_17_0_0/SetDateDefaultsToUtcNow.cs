using NPoco;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_0_0;

/// <summary>
/// Defines a migration to convert database date default constraints from local time to UTC.
/// </summary>
public class SetDateDefaultsToUtcNow : UnscopedMigrationBase
{
    private readonly IScopeProvider _scopeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetDateDefaultsToUtcNow"/> class.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="scopeProvider"></param>
    public SetDateDefaultsToUtcNow(IMigrationContext context, IScopeProvider scopeProvider)
        : base(context) => _scopeProvider = scopeProvider;

    protected override void Migrate()
    {
        if (DatabaseType == DatabaseType.SQLite)
        {
            MigrateSqlite();
        }
        else
        {
            MigrateSqlServer();
        }

        Context.Complete();
    }

    private void MigrateSqlite()
    {
        // SQLite doesn't fully support ALTER TABLE so to migrate we would need to create a new table and copy in the data.
        // However the previous defaults have been set to "DATE()", which isn't a sensible choice anyway as it has no time component.
        // Given that, it seems very unlikely we are using these database defaults in any meaningful way, and are instead providing
        // values for all date fields when saving.
        // As such we don't need to migrate these.
    }

    private void MigrateSqlServer()
    {
        using IScope scope = _scopeProvider.CreateScope();
        using IDisposable notificationSuppression = scope.Notifications.Suppress();
        ScopeDatabase(scope);

        ModifySqlServerDefaultDateConstraint(scope, Core.Constants.DatabaseSchema.Tables.Access, "createDate");
        ModifySqlServerDefaultDateConstraint(scope, Core.Constants.DatabaseSchema.Tables.Access, "updateDate");
        ModifySqlServerDefaultDateConstraint(scope, Core.Constants.DatabaseSchema.Tables.AccessRule, "createDate");
        ModifySqlServerDefaultDateConstraint(scope, Core.Constants.DatabaseSchema.Tables.AccessRule, "updateDate");
        ModifySqlServerDefaultDateConstraint(scope, Core.Constants.DatabaseSchema.Tables.AuditEntry, "eventDateUtc");
        ModifySqlServerDefaultDateConstraint(scope, Core.Constants.DatabaseSchema.Tables.Consent, "createDate");
        ModifySqlServerDefaultDateConstraint(scope, Core.Constants.DatabaseSchema.Tables.ContentVersion, "versionDate");
        ModifySqlServerDefaultDateConstraint(scope, Core.Constants.DatabaseSchema.Tables.CreatedPackageSchema, "updateDate");
        ModifySqlServerDefaultDateConstraint(scope, Core.Constants.DatabaseSchema.Tables.ExternalLogin, "createDate");
        ModifySqlServerDefaultDateConstraint(scope, Core.Constants.DatabaseSchema.Tables.ExternalLoginToken, "createDate");
        ModifySqlServerDefaultDateConstraint(scope, Core.Constants.DatabaseSchema.Tables.KeyValue, "updated");
        ModifySqlServerDefaultDateConstraint(scope, Core.Constants.DatabaseSchema.Tables.Log, "Datestamp");
        ModifySqlServerDefaultDateConstraint(scope, Core.Constants.DatabaseSchema.Tables.Node, "createDate");
        ModifySqlServerDefaultDateConstraint(scope, Core.Constants.DatabaseSchema.Tables.Relation, "datetime");
        ModifySqlServerDefaultDateConstraint(scope, Core.Constants.DatabaseSchema.Tables.Server, "registeredDate");
        ModifySqlServerDefaultDateConstraint(scope, Core.Constants.DatabaseSchema.Tables.User, "createDate");
        ModifySqlServerDefaultDateConstraint(scope, Core.Constants.DatabaseSchema.Tables.User, "updateDate");
        ModifySqlServerDefaultDateConstraint(scope, Core.Constants.DatabaseSchema.Tables.UserGroup, "createDate");
        ModifySqlServerDefaultDateConstraint(scope, Core.Constants.DatabaseSchema.Tables.UserGroup, "updateDate");

        Context.Complete();

        scope.Complete();
    }

    private static void ModifySqlServerDefaultDateConstraint(IScope scope, string tableName, string columnName)
    {
        var constraintName = $"DF_{tableName}_{columnName}";
        scope.Database.Execute($"ALTER TABLE {tableName} DROP CONSTRAINT {constraintName}");
        scope.Database.Execute($"ALTER TABLE {tableName} ADD CONSTRAINT {constraintName} DEFAULT GETUTCDATE() FOR {columnName}");
    }
}
