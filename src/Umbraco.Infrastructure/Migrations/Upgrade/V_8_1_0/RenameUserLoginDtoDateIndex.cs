using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_1_0;

public class RenameUserLoginDtoDateIndex : MigrationBase
{
    public RenameUserLoginDtoDateIndex(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        // there has been some confusion with an index name, resulting in
        // different names depending on which migration path was followed,
        // and discrepancies between an upgraded or an installed database.
        // better normalize
        if (IndexExists("IX_umbracoUserLogin_lastValidatedUtc"))
        {
            return;
        }

        if (IndexExists("IX_userLoginDto_lastValidatedUtc"))
        {
            Delete
                .Index("IX_userLoginDto_lastValidatedUtc")
                .OnTable(UserLoginDto.TableName)
                .Do();
        }

        Create
            .Index("IX_umbracoUserLogin_lastValidatedUtc")
            .OnTable(UserLoginDto.TableName)
            .OnColumn("lastValidatedUtc")
            .Ascending()
            .WithOptions().NonClustered()
            .Do();
    }
}
