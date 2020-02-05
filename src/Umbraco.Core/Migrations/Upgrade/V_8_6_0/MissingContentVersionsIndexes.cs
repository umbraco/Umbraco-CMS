using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_6_0
{
    public class MissingContentVersionsIndexes : MigrationBase
    {
        public MissingContentVersionsIndexes(IMigrationContext context) : base(context)
        {
        }

        public override void Migrate()
        {
            Create
                .Index("IX_" + ContentVersionDto.TableName + "_NodeId")
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
