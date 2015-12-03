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
                        guid = new Guid(Constants.PropertyTypeGroups.Image);
                    }
                    else if (data.grId == 4)
                    {
                        guid = new Guid(Constants.PropertyTypeGroups.File);
                    }
                    else if (data.grId == 5)
                    {
                        guid = new Guid(Constants.PropertyTypeGroups.Contents);
                    }
                    else if (data.grId == 11)
                    {
                        guid = new Guid(Constants.PropertyTypeGroups.Membership);
                    }
                    else
                    {
                        // create a consistent guid from
                        // group name + content type alias + object type
                        string guidSource = data.grName + data.ctAlias + data.nObjType;
                        guid = guidSource.ToGuid();
                    }

                    // set the Unique Id to the one we've generated
                    Update.Table("cmsPropertyTypeGroup").Set(new { uniqueID = guid }).Where(new { id = data.grId });
                }
            }
        }

        public override void Down()
        { }
    }
}