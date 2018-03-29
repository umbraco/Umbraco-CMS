using System.Linq;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Migrations.Upgrade.V_7_8_0
{
    internal class AddIndexToPropertyTypeAliasColumn : MigrationBase
    {
        public AddIndexToPropertyTypeAliasColumn(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            var dbIndexes = SqlSyntax.GetDefinedIndexesDefinitions(Context.Database);

            //make sure it doesn't already exist
            if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_cmsPropertyTypeAlias")) == false)
            {
                //we can apply the index
                Create.Index("IX_cmsPropertyTypeAlias").OnTable(Constants.DatabaseSchema.Tables.PropertyType)
                    .OnColumn("Alias")
                    .Ascending().WithOptions().NonClustered()
                    .Do();
            }
        }
    }
}
