namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSixth
{
    [MigrationAttribute("6.0.0", 5)]
    public class MoveMasterContentTypeData : MigrationBase
    {
        public override void Up()
        {
            Execute.Sql(
                "INSERT INTO [cmsContentType2ContentType] (parentContentTypeId, childContentTypeId) SELECT masterContentType, nodeId FROM [cmsContentType] WHERE not [masterContentType] is null and [masterContentType] != 0");
        }

        public override void Down()
        {
        }
    }
}