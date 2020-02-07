using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_6_0
{
    public class MissingContentVersionsIndexes : MigrationBase
    {
        private const string IndexName = "IX_" + ContentVersionDto.TableName + "_NodeId";

        public MissingContentVersionsIndexes(IMigrationContext context) : base(context)
        {
        }

        public override void Migrate()
        {
            // We must check before we create an index because if we are upgrading from v7 we force re-create all
            // indexes in the whole DB and then this would throw

            if (!IndexExists(IndexName))
            {
                Create
                    .Index(IndexName)
                    .OnTable(ContentVersionDto.TableName)
                    .OnColumn("nodeId")
                    .Ascending()
                    .OnColumn("current")
                    .Ascending()
                    .WithOptions().NonClustered()
                    .Do();
            }

        }
    }
}
