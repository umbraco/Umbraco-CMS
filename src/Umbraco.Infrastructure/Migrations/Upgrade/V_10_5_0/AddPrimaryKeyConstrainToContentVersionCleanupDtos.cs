using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_10_5_0;

public class AddPrimaryKeyConstrainToContentVersionCleanupDtos : MigrationBase
{
    public AddPrimaryKeyConstrainToContentVersionCleanupDtos(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        IEnumerable<ContentVersionCleanupPolicyDto> contentVersionCleanupPolicyDtos =
            Database
                .Fetch<ContentVersionCleanupPolicyDto>()
                .OrderByDescending(x => x.Updated)
                .DistinctBy(x => x.ContentTypeId);

        if (TableExists(ContentVersionCleanupPolicyDto.TableName))
        {
            Delete.Table(ContentVersionCleanupPolicyDto.TableName).Do();
        }

        Create.Table<ContentVersionCleanupPolicyDto>().Do();
        Database.InsertBatch(contentVersionCleanupPolicyDtos);
    }
}
