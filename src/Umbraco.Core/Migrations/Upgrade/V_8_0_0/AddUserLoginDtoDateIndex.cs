using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{
    public class AddUserLoginDtoDateIndex : MigrationBase
    {
        public AddUserLoginDtoDateIndex(IMigrationContext context)
            : base(context)
        { }

        public override void Migrate()
        {
            if (!IndexExists("IX_umbracoUserLogin_lastValidatedUtc"))
                Create.Index("IX_umbracoUserLogin_lastValidatedUtc")
                    .OnTable(UserLoginDto.TableName)
                    .OnColumn("lastValidatedUtc")
                    .Ascending()
                    .WithOptions().NonClustered()
                    .Do();
        }
    }
}
