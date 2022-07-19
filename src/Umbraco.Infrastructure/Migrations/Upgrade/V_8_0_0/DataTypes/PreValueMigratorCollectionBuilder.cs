using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0.DataTypes;

public class PreValueMigratorCollectionBuilder : OrderedCollectionBuilderBase<PreValueMigratorCollectionBuilder,
    PreValueMigratorCollection, IPreValueMigrator>
{
    protected override PreValueMigratorCollectionBuilder This => this;
}
