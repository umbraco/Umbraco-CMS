using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_0_0;

/// <summary>
/// Represents a migration that adds GUID columns to audit entries in the database to improve identification and tracking of records.
/// </summary>
public class AddGuidsToAuditEntries : UnscopedMigrationBase
{
    private const string NewPerformingUserKeyColumnName = "performingUserKey";
    private const string NewAffectedUserKeyColumnName = "affectedUserKey";
    private readonly IScopeProvider _scopeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddGuidsToAuditEntries"/> class.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> for the migration.</param>
    /// <param name="scopeProvider">The <see cref="IScopeProvider"/> used to manage database scopes.</param>
    public AddGuidsToAuditEntries(IMigrationContext context, IScopeProvider scopeProvider)
        : base(context)
    {
        _scopeProvider = scopeProvider;
    }

    protected override void Migrate()
    {
        // If the new column already exists, we'll do nothing.
        if (ColumnExists(Constants.DatabaseSchema.Tables.AuditEntry, NewPerformingUserKeyColumnName))
        {
            Context.Complete();
            return;
        }

        using IScope scope = _scopeProvider.CreateScope();
        using IDisposable notificationSuppression = scope.Notifications.Suppress();
        ScopeDatabase(scope);

        var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToList();
        AddColumnIfNotExists<AuditEntryDto>(columns, NewPerformingUserKeyColumnName);
        AddColumnIfNotExists<AuditEntryDto>(columns, NewAffectedUserKeyColumnName);

        Database.Execute(
            new Sql(
                "UPDATE umbracoAudit " +
                "SET performingUserKey = (" +
                $"SELECT umbracoUser.{Database.SqlContext.SqlSyntax.GetQuotedColumnName("key")} FROM umbracoUser WHERE umbracoUser.id= umbracoAudit.performingUserId);"));

        Database.Execute(
            new Sql(
                "UPDATE umbracoAudit " +
                "SET affectedUserKey = (" +
                $"SELECT umbracoUser.{Database.SqlContext.SqlSyntax.GetQuotedColumnName("key")} FROM umbracoUser WHERE umbracoUser.id= umbracoAudit.affectedUserId);"));

        scope.Complete();
        Context.Complete();
    }
}
