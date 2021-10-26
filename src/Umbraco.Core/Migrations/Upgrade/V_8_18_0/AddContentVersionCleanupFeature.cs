using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_18_0
{
    class AddContentVersionCleanupFeature : MigrationBase
    {
        public AddContentVersionCleanupFeature(IMigrationContext context)
            : base(context) { }

        /// <remarks>
        /// The conditionals are useful to enable the same migration to be used in multiple
        /// migration paths x.x -> 8.18 and x.x -> 9.x
        /// </remarks>
        public override void Migrate()
        {
            var tables = SqlSyntax.GetTablesInSchema(Context.Database);
            if (!tables.InvariantContains(ContentVersionCleanupPolicyDto.TableName))
            {
                Create.Table<ContentVersionCleanupPolicyDto>().Do();
            }

            var columns = SqlSyntax.GetColumnsInSchema(Context.Database);
            AddColumnIfNotExists<ContentVersionDto>(columns, "preventCleanup");
        }
    }
}
