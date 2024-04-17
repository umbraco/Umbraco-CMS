using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_11_3_0;

[Obsolete("This is no longer used and will be removed in V14.")]
public class AddDomainSortOrder : MigrationBase
{
    public AddDomainSortOrder(IMigrationContext context)
        : base(context)
    { }

    protected override void Migrate()
    {
        if (ColumnExists(DomainDto.TableName, "sortOrder") == false)
        {
            // Use a custom SQL query to prevent selecting explicit columns (sortOrder doesn't exist yet)
            List<DomainDto> domainDtos = Database.Fetch<DomainDto>($"SELECT * FROM {DomainDto.TableName}");

            Delete.Table(DomainDto.TableName).Do();
            Create.Table<DomainDto>().Do();

            foreach (DomainDto domainDto in domainDtos)
            {
                bool isWildcard = string.IsNullOrWhiteSpace(domainDto.DomainName) || domainDto.DomainName.StartsWith("*");
                if (isWildcard)
                {
                    // Set sort order of wildcard domains to -1
                    domainDto.SortOrder = -1;
                }
                else
                {
                    // Keep exising sort order by setting it to the id
                    domainDto.SortOrder = domainDto.Id;
                }
            }

            Database.InsertBatch(domainDtos);
        }
    }
}
