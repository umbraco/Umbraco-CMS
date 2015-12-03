using System;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSixZeroOne
{
    [Migration("6.0.2", 0, GlobalSettings.UmbracoMigrationName)]
    public class UpdatePropertyTypesAndGroups : MigrationBase
    {
        public UpdatePropertyTypesAndGroups(ISqlSyntaxProvider sqlSyntax, ILogger logger) : base(sqlSyntax, logger)
        {
        }

        public override void Up()
        {
            Execute.Code(UpdatePropertyTypesAndGroupsDo);
        }

        public override void Down()
        {
            
        }

        public static string UpdatePropertyTypesAndGroupsDo(Database database)
        {
            if (database != null)
            {
                //Fetch all PropertyTypes that belongs to a PropertyTypeGroup                
                //NOTE: We are writing the full query because we've added a column to the PropertyTypeDto in later versions so one of the columns
                // won't exist yet
                var propertyTypes = database.Fetch<dynamic>("SELECT * FROM cmsPropertyType WHERE propertyTypeGroupId > 0");

                var propertyGroups = database.Fetch<PropertyTypeGroupDto>("WHERE id > 0");

                foreach (var propertyType in propertyTypes)
                {
                    // get the PropertyTypeGroup of the current PropertyType, skip if not found
                    var propertyTypeGroup = propertyGroups.FirstOrDefault(x => x.Id == propertyType.propertyTypeGroupId);
                    if (propertyTypeGroup == null) continue;

                    // if the PropretyTypeGroup belongs to the same content type as the PropertyType, then fine
                    if (propertyTypeGroup.ContentTypeNodeId == propertyType.contentTypeId) continue;

                    // else we want to assign the PropertyType to a proper PropertyTypeGroup
                    // ie one that does belong to the same content - look for it
                    var okPropertyTypeGroup = propertyGroups.FirstOrDefault(x =>
                        x.Text == propertyTypeGroup.Text && // same name
                        x.ContentTypeNodeId == propertyType.contentTypeId); // but for proper content type

                    if (okPropertyTypeGroup == null)
                    {
                        // does not exist, create a new PropertyTypeGroup,
                        var propertyGroup = new PropertyTypeGroupDto
                        {
                            ContentTypeNodeId = propertyType.contentTypeId,
                            Text = propertyTypeGroup.Text,
                            SortOrder = propertyTypeGroup.SortOrder
                        };

                        // save + add to list of groups
                        int id = Convert.ToInt16(database.Insert(propertyGroup));
                        propertyGroup.Id = id;
                        propertyGroups.Add(propertyGroup);

                        // update the PropertyType to use the new PropertyTypeGroup
                        propertyType.propertyTypeGroupId = id;
                    }
                    else
                    {
                        // exists, update PropertyType to use the PropertyTypeGroup
                        propertyType.propertyTypeGroupId = okPropertyTypeGroup.Id;
                    }
                    database.Update("cmsPropertyType", "id", propertyType);
                }
            }

            return string.Empty;
        }
    }
}