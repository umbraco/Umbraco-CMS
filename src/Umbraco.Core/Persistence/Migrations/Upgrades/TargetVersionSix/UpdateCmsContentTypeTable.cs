using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSix
{
    [Migration("6.0.0", 2, GlobalSettings.UmbracoMigrationName)]
    public class UpdateCmsContentTypeTable : MigrationBase
    {
        public override void Up()
        {
            Alter.Table("cmsContentType").AddColumn("isContainer").AsBoolean().NotNullable().WithDefaultValue(0);

            Alter.Table("cmsContentType").AddColumn("allowAtRoot").AsBoolean().NotNullable().WithDefaultValue(0);

            //Some very old schemas don't have an index on the cmsContentType.nodeId column, I'm not actually sure when it was added but 
            // it is absolutely required to exist in order to add other foreign keys and much better for perf, so we'll need to check it's existence
            // this came to light from this issue: http://issues.umbraco.org/issue/U4-4133
            var dbIndexes = SqlSyntaxContext.SqlSyntaxProvider.GetDefinedIndexes(Context.Database)
                .Select(x => new DbIndexDefinition()
                {
                    TableName = x.Item1,
                    IndexName = x.Item2,
                    ColumnName = x.Item3,
                    IsUnique = x.Item4
                }).ToArray();
            if (dbIndexes.Any(x => x.IndexName.InvariantEquals("IX_cmsContentType")) == false)
            {
                Create.Index("IX_cmsContentType").OnTable("cmsContentType").OnColumn("nodeId").Ascending().WithOptions().Unique();
            }
            if (dbIndexes.Any(x => x.TableName.InvariantEquals("cmsContentType") && x.ColumnName.InvariantEquals("icon")) == false)
            {
                Create.Index("IX_cmsContentType_icon").OnTable("cmsContentType").OnColumn("icon").Ascending().WithOptions().NonClustered();                
            }
        }

        public override void Down()
        {
            Delete.Column("allowAtRoot").FromTable("cmsContentType");

            Delete.Column("isContainer").FromTable("cmsContentType");
        }
    }
}