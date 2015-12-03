using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenThreeZero
{    
    [Migration("7.3.0", 13, GlobalSettings.UmbracoMigrationName)]
    public class AddUniqueIdPropertyTypeColumn : MigrationBase
    {
        public AddUniqueIdPropertyTypeColumn(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        { }

        public override void Up()
        {
            //Don't exeucte if the column is already there
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();

            if (columns.Any(x => x.TableName.InvariantEquals("cmsPropertyType") && x.ColumnName.InvariantEquals("uniqueID")) == false)
            {
                Create.Column("uniqueID").OnTable("cmsPropertyType").AsGuid().NotNullable().WithDefault(SystemMethods.NewGuid);

                // unique constraint on name + version
                Create.Index("IX_cmsPropertyTypeUniqueID").OnTable("cmsPropertyType")
                    .OnColumn("uniqueID").Ascending()
                    .WithOptions()
                    .NonClustered()
                    .WithOptions()
                    .Unique();

                // fill in the data in a way that is consistent over all environments
                // (ie cannot use random guids, http://issues.umbraco.org/issue/U4-6942)

                foreach (var data in Context.Database.Query<dynamic>(@"
SELECT cmsPropertyType.id ptId, cmsPropertyType.Alias ptAlias, cmsContentType.alias ctAlias, umbracoNode.nodeObjectType nObjType
FROM cmsPropertyType
INNER JOIN cmsContentType
ON cmsPropertyType.contentTypeId = cmsContentType.nodeId
INNER JOIN umbracoNode
ON cmsContentType.nodeId = umbracoNode.id"))
                {
                    // create a consistent guid from
                    // property alias + content type alias + object type
                    string guidSource = data.ptAlias + data.ctAlias + data.nObjType;
                    var guid = guidSource.ToGuid();

                    // set the Unique Id to the one we've generated
                    Update.Table("cmsPropertyType").Set(new { uniqueID = guid }).Where(new { id = data.ptId });
                }
            }
        }

        public override void Down()
        { }
    }
}