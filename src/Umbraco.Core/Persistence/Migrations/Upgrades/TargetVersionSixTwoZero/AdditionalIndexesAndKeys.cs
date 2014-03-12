using System;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence.Migrations.Initial;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSixTwoZero
{
    [Migration("6.2.0", 0, GlobalSettings.UmbracoMigrationName)]
    public class AdditionalIndexesAndKeys : MigrationBase
    {
        public override void Up()
        {
            var dbSchema = new DatabaseSchemaCreation(Context.Database);
            var schemaResult = dbSchema.ValidateSchema();

            //do not create any indexes if they already exist in the database

            if (schemaResult.DbIndexDefinitions.Any(x => x.IndexName == "IX_umbracoNodeTrashed") == false)
            {
                Create.Index("IX_umbracoNodeTrashed").OnTable("umbracoNode").OnColumn("trashed").Ascending().WithOptions().NonClustered();    
            }
            if (schemaResult.DbIndexDefinitions.Any(x => x.IndexName == "IX_cmsContentVersion_ContentId") == false)
            {
                Create.Index("IX_cmsContentVersion_ContentId").OnTable("cmsContentVersion").OnColumn("ContentId").Ascending().WithOptions().NonClustered();
            }
            if (schemaResult.DbIndexDefinitions.Any(x => x.IndexName == "IX_cmsDocument_published") == false)
            {
                Create.Index("IX_cmsDocument_published").OnTable("cmsDocument").OnColumn("published").Ascending().WithOptions().NonClustered();
            }
            if (schemaResult.DbIndexDefinitions.Any(x => x.IndexName == "IX_cmsDocument_newest") == false)
            {
                Create.Index("IX_cmsDocument_newest").OnTable("cmsDocument").OnColumn("newest").Ascending().WithOptions().NonClustered();
            }
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }

    [Migration("6.2.0", 2, GlobalSettings.UmbracoMigrationName)]
    public class ChangePasswordColumn : MigrationBase
    {
        public override void Up()
        {
            //up to 500 chars
            Alter.Table("umbracoUser").AlterColumn("userPassword").AsString(500).NotNullable();
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}