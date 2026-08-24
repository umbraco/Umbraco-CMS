using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_0_0;

/// <summary>
/// Defines a migration to convert database date default constraints from local time to UTC.
/// </summary>
public class SetDateDefaultsToUtcNow : UnscopedMigrationBase
{
    private const string CreateDateColumnName = "createDate";
    private const string UpdateDateColumnName = "updateDate";

    private readonly IScopeProvider _scopeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetDateDefaultsToUtcNow"/> class.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="scopeProvider"></param>
    public SetDateDefaultsToUtcNow(IMigrationContext context, IScopeProvider scopeProvider)
        : base(context) => _scopeProvider = scopeProvider;

    /// <inheritdoc/>
    protected override void Migrate()
    {
        if (DatabaseType == DatabaseType.SQLite)
        {
            MigrateSqlite();
            Context.Complete();
            return;
        }

        MigrateSqlServer();
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

        ModifySqlServerDefaultDateConstraint(Core.Constants.DatabaseSchema.Tables.Access, CreateDateColumnName);
        ModifySqlServerDefaultDateConstraint(Core.Constants.DatabaseSchema.Tables.Access, UpdateDateColumnName);
        ModifySqlServerDefaultDateConstraint(Core.Constants.DatabaseSchema.Tables.AccessRule, CreateDateColumnName);
        ModifySqlServerDefaultDateConstraint(Core.Constants.DatabaseSchema.Tables.AccessRule, UpdateDateColumnName);
        ModifySqlServerDefaultDateConstraint(Core.Constants.DatabaseSchema.Tables.AuditEntry, "eventDateUtc");
        ModifySqlServerDefaultDateConstraint(Core.Constants.DatabaseSchema.Tables.Consent, CreateDateColumnName);
        ModifySqlServerDefaultDateConstraint(Core.Constants.DatabaseSchema.Tables.ContentVersion, "versionDate");
        ModifySqlServerDefaultDateConstraint(Core.Constants.DatabaseSchema.Tables.CreatedPackageSchema, UpdateDateColumnName);
        ModifySqlServerDefaultDateConstraint(Core.Constants.DatabaseSchema.Tables.ExternalLogin, CreateDateColumnName);
        ModifySqlServerDefaultDateConstraint(Core.Constants.DatabaseSchema.Tables.ExternalLoginToken, CreateDateColumnName);
        ModifySqlServerDefaultDateConstraint(Core.Constants.DatabaseSchema.Tables.KeyValue, "updated");
        ModifySqlServerDefaultDateConstraint(Core.Constants.DatabaseSchema.Tables.Log, "Datestamp");
        ModifySqlServerDefaultDateConstraint(Core.Constants.DatabaseSchema.Tables.Node, CreateDateColumnName);
        ModifySqlServerDefaultDateConstraint(Core.Constants.DatabaseSchema.Tables.Relation, "datetime");
        ModifySqlServerDefaultDateConstraint(Core.Constants.DatabaseSchema.Tables.Server, "registeredDate");
        ModifySqlServerDefaultDateConstraint(Core.Constants.DatabaseSchema.Tables.User, CreateDateColumnName);
        ModifySqlServerDefaultDateConstraint(Core.Constants.DatabaseSchema.Tables.User, UpdateDateColumnName);
        ModifySqlServerDefaultDateConstraint(Core.Constants.DatabaseSchema.Tables.UserGroup, CreateDateColumnName);
        ModifySqlServerDefaultDateConstraint(Core.Constants.DatabaseSchema.Tables.UserGroup, UpdateDateColumnName);

        Context.Complete();

        scope.Complete();
    }

    private void ModifySqlServerDefaultDateConstraint(string tableName, string columnName)
    {
        var quotedTableName = SqlSyntax.GetQuotedTableName(tableName);
        var quotedColumnName = SqlSyntax.GetQuotedColumnName(columnName);
        var expectedConstraintName = $"DF_{tableName}_{columnName}";

        // A database whose schema was materialised outside of Umbraco can carry a system generated constraint name,
        // or no default constraint at all, so the name we would generate for the column can't be assumed to be the
        // one in use. Recreating under the expected name realigns the schema as a side effect.
        if (SqlSyntax.TryGetDefaultConstraint(Database, tableName, columnName, out var existingConstraintName))
        {
            if (existingConstraintName.Equals(expectedConstraintName, StringComparison.Ordinal) is false)
            {
                Logger.LogInformation(
                    "The default constraint on {TableName}.{ColumnName} is named {ExistingConstraintName} rather than {ExpectedConstraintName}. It will be recreated under the expected name.",
                    tableName,
                    columnName,
                    existingConstraintName,
                    expectedConstraintName);
            }

            Database.Execute(new Sql($"ALTER TABLE {quotedTableName} DROP CONSTRAINT {SqlSyntax.GetQuotedName(existingConstraintName)}"));
        }
        else
        {
            Logger.LogInformation(
                "No default constraint was found on {TableName}.{ColumnName}. One will be created named {ExpectedConstraintName}.",
                tableName,
                columnName,
                expectedConstraintName);
        }

        Database.Execute(new Sql($"ALTER TABLE {quotedTableName} ADD CONSTRAINT {SqlSyntax.GetQuotedName(expectedConstraintName)} DEFAULT GETUTCDATE() FOR {quotedColumnName}"));
    }
}
