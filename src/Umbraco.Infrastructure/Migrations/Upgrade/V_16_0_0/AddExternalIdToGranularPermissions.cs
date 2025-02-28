using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_1_0;

public class AddExternalIdToGranularPermissions : UnscopedMigrationBase
{
    private readonly IScopeProvider _scopeProvider;
    private const string NewColumnName = "umbracoExternalIds";
    private const string newIndex = "IX_umbracoUserGroup2GranularPermissionDto_ExternalUniqueId";

    public AddExternalIdToGranularPermissions(
        IMigrationContext context,
        IScopeProvider scopeProvider)
        : base(context)
    {
        _scopeProvider = scopeProvider;
    }

    protected override void Migrate()
    {
        using IScope scope = _scopeProvider.CreateScope();
        if (ColumnExists(Constants.DatabaseSchema.Tables.UserGroup2GranularPermission, NewColumnName) is false)
        {
            Alter.Table(Constants.DatabaseSchema.Tables.UserGroup2GranularPermission)
                .AddColumn(NewColumnName)
                .AsGuid()
                .Nullable()
                .Do();
        }

        if (IndexExists(newIndex) is false)
        {
            CreateIndex<UserGroup2GranularPermissionDto>("IX_umbracoUserGroup2GranularPermissionDto_ExternalUniqueId");
        }

        Context.Complete();
        scope.Complete();
    }
}
