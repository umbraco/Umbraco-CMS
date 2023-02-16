using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_13_0_0;

/// <summary>
/// This is an unscoped migration to support migrating sqlite, since it doesn't support adding columns.
/// See <see cref="AddGuidsToUserGroups"/> for more information.
/// </summary>
public class AddGuidsToUsers : UnscopedMigrationBase
{
    private const string NewColumnName = "key";
    private readonly IScopeProvider _scopeProvider;

    public AddGuidsToUsers(IMigrationContext context, IScopeProvider scopeProvider)
        : base(context)
    {
        _scopeProvider = scopeProvider;
    }

    protected override void Migrate()
    {
        if (DatabaseType != DatabaseType.SQLite)
        {
            MigrateSqlServer();
            return;
        }
    }

    private void MigrateSqlServer()
    {
        using IScope scope = _scopeProvider.CreateScope();
        using IDisposable notificationSuppression = scope.Notifications.Suppress();
        ScopeDatabase(scope);

        var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToList();
        AddColumnIfNotExists<UserDto>(columns, NewColumnName);
        scope.Complete();
    }
}
