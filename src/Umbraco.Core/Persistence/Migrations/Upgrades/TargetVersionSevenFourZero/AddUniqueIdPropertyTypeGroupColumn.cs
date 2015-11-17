using System;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSevenFourZero
{
    [Migration("7.4.0", 2, GlobalSettings.UmbracoMigrationName)]
    public class AddUniqueIdPropertyTypeGroupColumn : MigrationBase
    {
        public AddUniqueIdPropertyTypeGroupColumn(ISqlSyntaxProvider sqlSyntax, ILogger logger)
            : base(sqlSyntax, logger)
        { }

        public override void Up()
        {
            // don't execute if the column is already there
            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToArray();

            if (columns.Any(x => x.TableName.InvariantEquals("cmsPropertyTypeGroup") && x.ColumnName.InvariantEquals("uniqueID")) == false)
            {
                Create.Column("uniqueID").OnTable("cmsPropertyTypeGroup").AsGuid().NotNullable().WithDefault(SystemMethods.NewGuid);

                // unique constraint on name + version
                Create.Index("IX_cmsPropertyTypeGroupUniqueID").OnTable("cmsPropertyTypeGroup")
                    .OnColumn("uniqueID").Ascending()
                    .WithOptions()
                    .NonClustered()
                    .WithOptions()
                    .Unique();

                // fill in the data in a way that is consistent over all environments
                // (ie cannot use random guids, http://issues.umbraco.org/issue/U4-6942)

                foreach (var data in Context.Database.Query<dynamic>(@"
SELECT cmsPropertyTypeGroup.id grId, cmsPropertyTypeGroup.text grName, cmsContentType.alias ctAlias, umbracoNode.nodeObjectType nObjType
FROM cmsPropertyTypeGroup
INNER JOIN cmsContentType
ON cmsPropertyTypeGroup.contentTypeNodeId = cmsContentType.nodeId
INNER JOIN umbracoNode
ON cmsContentType.nodeId = umbracoNode.id"))
                {
                    Guid guid;
                    // see BaseDataCreation... built-in groups have their own guids
                    if (data.grId == 3)
                    {
                        guid = new Guid("79ED4D07-254A-42CF-8FA9-EBE1C116A596");
                    }
                    else if (data.grId == 4)
                    {
                        guid = new Guid("50899F9C-023A-4466-B623-ABA9049885FE");
                    }
                    else if (data.grId == 5)
                    {
                        guid = new Guid("79995FA2-63EE-453C-A29B-2E66F324CDBE");
                    }
                    else if (data.grId == 11)
                    {
                        guid = new Guid("0756729D-D665-46E3-B84A-37ACEAA614F8");
                    }
                    else
                    {
                        // create a consistent guid from
                        // group name + content type alias + object type
                        string guidSource = data.grName + data.ctAlias + data.nObjType;
                        guid = guidSource.ToGuid();
                    }

                    // set the Unique Id to the one we've generated
                    Update.Table("cmsPropertyTypeGroup").Set(new { uniqueID = guid }).Where(new { id = data.ptId });
                }
            }
        }

        public override void Down()
        { }
    }
}