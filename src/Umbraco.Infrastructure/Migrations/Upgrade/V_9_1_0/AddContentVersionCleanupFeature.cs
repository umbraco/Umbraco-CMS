using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_1_0
{
    class AddContentVersionCleanupFeature : MigrationBase
    {
        public AddContentVersionCleanupFeature(IMigrationContext context)
            : base(context) { }

        /// <remarks>
        /// The conditionals are useful to enable the same migration to be used in multiple
        /// migration paths x.x -> 8.18 and x.x -> 9.x
        /// </remarks>
        protected override void Migrate()
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
