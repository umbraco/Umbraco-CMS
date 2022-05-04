using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_6_0;

public class MissingContentVersionsIndexes : MigrationBase
{
    private const string IndexName = "IX_" + ContentVersionDto.TableName + "_NodeId";

    public MissingContentVersionsIndexes(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
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
