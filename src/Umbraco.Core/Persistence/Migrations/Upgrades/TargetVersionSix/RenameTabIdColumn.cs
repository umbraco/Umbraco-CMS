using System.Data;
using NPoco;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSix
{
    [Migration("6.0.0", 7, GlobalSettings.UmbracoMigrationName)]
    public class RenameTabIdColumn : MigrationBase
    {
        public RenameTabIdColumn(IMigrationContext context) 
            : base(context)
        { }

        public override void Up()
        {
            //Conditional Create-column for Sql Ce databases
            IfDatabase(DatabaseType.SQLCe)
                .Create.Column("propertyTypeGroupId").OnTable("cmsPropertyType").AsInt16().Nullable();

            //Conditional Create-foreign for Sql Ce databases
            IfDatabase(DatabaseType.SQLCe)
                .Create.ForeignKey("FK_cmsPropertyType_cmsPropertyTypeGroup")
                .FromTable("cmsPropertyType").ForeignColumn("propertyTypeGroupId")
                .ToTable("cmsPropertyTypeGroup").PrimaryColumn("id").OnDeleteOrUpdate(Rule.None);

            //Conditional Delete-foreignkey for MySql databases
            IfDatabase(DatabaseType.MySQL)
                .Delete.ForeignKey().FromTable("cmsPropertyType").ForeignColumn("tabId").ToTable("cmsPropertyTypeGroup").PrimaryColumn("id");

            Rename.Column("tabId").OnTable("cmsPropertyType").To("propertyTypeGroupId");

            //Conditional Create-foreign for MySql databases
            IfDatabase(DatabaseType.MySQL)
                .Create.ForeignKey("FK_cmsPropertyType_cmsPropertyTypeGroup")
                .FromTable("cmsPropertyType").ForeignColumn("propertyTypeGroupId")
                .ToTable("cmsPropertyTypeGroup").PrimaryColumn("id").OnDeleteOrUpdate(Rule.None);

            //Conditional Delete-foreignkey for Sql Ce databases
            IfDatabase(DatabaseType.SQLCe)
                .Delete.ForeignKey("FK_cmsPropertyType_cmsTab").OnTable("cmsPropertyType");

            //Conditional Delete-column for Sql Ce databases
            IfDatabase(DatabaseType.SQLCe)
                .Delete.Column("tabId").FromTable("cmsPropertyType");
        }

        public override void Down()
        {
            //Conditional Create-column for Sql Ce databases
            IfDatabase(DatabaseType.SQLCe)
                .Create.Column("tabId").OnTable("cmsPropertyType").AsInt16().Nullable();

            //Conditional Create-foreign for Sql Ce databases
            IfDatabase(DatabaseType.SQLCe)
                .Create.ForeignKey("FK_cmsPropertyType_cmsTab")
                .FromTable("cmsPropertyType").ForeignColumn("tabId")
                .ToTable("cmsTab").PrimaryColumn("id").OnDeleteOrUpdate(Rule.None);

            //Conditional Delete-foreignkey for MySql databases
            IfDatabase(DatabaseType.MySQL)
                .Delete.ForeignKey().FromTable("cmsPropertyType").ForeignColumn("propertyTypeGroupId").ToTable("cmsPropertyTypeGroup").PrimaryColumn("id");

            Rename.Column("propertyTypeGroupId").OnTable("cmsPropertyType").To("tabId");

            //Conditional Create-foreign for MySql databases
            IfDatabase(DatabaseType.MySQL)
                .Create.ForeignKey("FK_cmsPropertyType_cmsPropertyTypeGroup")
                .FromTable("cmsPropertyType").ForeignColumn("tabId")
                .ToTable("cmsPropertyTypeGroup").PrimaryColumn("id").OnDeleteOrUpdate(Rule.None);

            //Conditional Delete-foreignkey for Sql Ce databases
            IfDatabase(DatabaseType.SQLCe)
                .Delete.ForeignKey("FK_cmsPropertyType_cmsPropertyTypeGroup").OnTable("cmsPropertyType");

            //Conditional Delete-column for Sql Ce databases
            IfDatabase(DatabaseType.SQLCe)
                .Delete.Column("propertyTypeGroupId").FromTable("propertyTypeGroupId");
        }
    }
}