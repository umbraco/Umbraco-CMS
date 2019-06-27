using Umbraco.Core.Composing;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0.DataTypes
{
    public class PreValueMigratorCollectionBuilder : OrderedCollectionBuilderBase<PreValueMigratorCollectionBuilder, PreValueMigratorCollection, IPreValueMigrator>
    {
        protected override PreValueMigratorCollectionBuilder This => this;
    }
}
