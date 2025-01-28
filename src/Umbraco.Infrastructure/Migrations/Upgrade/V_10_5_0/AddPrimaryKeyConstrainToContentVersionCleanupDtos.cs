using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_10_5_0;

[Obsolete("This is no longer used and will be removed in V14.")]
public class AddPrimaryKeyConstrainToContentVersionCleanupDtos : MigrationBase
{
    public AddPrimaryKeyConstrainToContentVersionCleanupDtos(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        IEnumerable<ContentVersionCleanupPolicyDto>? contentVersionCleanupPolicyDtos = null;
        if (TableExists(ContentVersionCleanupPolicyDto.TableName))
        {
            if (PrimaryKeyExists(ContentVersionCleanupPolicyDto.TableName, "PK_umbracoContentVersionCleanupPolicy"))
            {
                return;
            }

            contentVersionCleanupPolicyDtos =
                Database
                    .Fetch<ContentVersionCleanupPolicyDto>()
                    .OrderByDescending(x => x.Updated)
                    .DistinctBy(x => x.ContentTypeId);

            Delete.Table(ContentVersionCleanupPolicyDto.TableName).Do();
        }

        Create.Table<ContentVersionCleanupPolicyDto>().Do();

        if (contentVersionCleanupPolicyDtos is not null)
        {
            Database.InsertBatch(contentVersionCleanupPolicyDtos);
        }
    }
}
