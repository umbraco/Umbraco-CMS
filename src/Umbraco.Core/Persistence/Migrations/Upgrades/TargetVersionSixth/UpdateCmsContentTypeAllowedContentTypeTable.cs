namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSixth
{
    [MigrationAttribute("6.0.0", 3)]
    public class UpdateCmsContentTypeAllowedContentTypeTable : MigrationBase
    {
        public override void Up()
        {
            Alter.Table("cmsContentTypeAllowedContentType").AddColumn("sortOrder").AsInt16().NotNullable().WithDefaultValue(1);
        }

        public override void Down()
        {
            Delete.Column("sortOrder").FromTable("cmsContentTypeAllowedContentType");
        }
    }
}