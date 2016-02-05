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

                // need to use dynamic, as PropertyTypeGroupDto has new properties
                var propertyGroups = database.Fetch<dynamic>("SELECT * FROM cmsPropertyTypeGroup WHERE id > 0");

                foreach (var propertyType in propertyTypes)
                {
                    // get the PropertyTypeGroup of the current PropertyType, skip if not found
                    var propertyTypeGroup = propertyGroups.FirstOrDefault(x => x.id == propertyType.propertyTypeGroupId);
                    if (propertyTypeGroup == null) continue;

                    // if the PropretyTypeGroup belongs to the same content type as the PropertyType, then fine
                    if (propertyTypeGroup.contenttypeNodeId == propertyType.contentTypeId) continue;

                    // else we want to assign the PropertyType to a proper PropertyTypeGroup
                    // ie one that does belong to the same content - look for it
                    var okPropertyTypeGroup = propertyGroups.FirstOrDefault(x =>
                        x.text == propertyTypeGroup.text && // same name
                        x.contenttypeNodeId == propertyType.contentTypeId); // but for proper content type

                    if (okPropertyTypeGroup == null)
                    {
                        // does not exist, create a new PropertyTypeGroup
                        // cannot use a PropertyTypeGroupDto because of the new (not-yet-existing) uniqueID property
                        // cannot use a dynamic because database.Insert fails to set the value of property
                        var propertyGroup = new PropertyTypeGroupDtoTemp
                        {
                            id = 0,
                            contenttypeNodeId = propertyType.contentTypeId,
                            text = propertyTypeGroup.text,
                            sortorder = propertyTypeGroup.sortorder
                        };

                        // save + add to list of groups
                        int id = Convert.ToInt16(database.Insert("cmsPropertyTypeGroup", "id", propertyGroup));
                        propertyGroup.id = id;
                        propertyGroups.Add(propertyGroup);

                        // update the PropertyType to use the new PropertyTypeGroup
                        propertyType.propertyTypeGroupId = id;
                    }
                    else
                    {
                        // exists, update PropertyType to use the PropertyTypeGroup
                        propertyType.propertyTypeGroupId = okPropertyTypeGroup.id;
                    }
                    database.Update("cmsPropertyType", "id", propertyType);
                }
            }

            return string.Empty;
        }

        private class PropertyTypeGroupDtoTemp
        {
            public int id { get; set; }
            public int contenttypeNodeId { get; set; }
            public string text { get; set; }
            public int sortorder { get; set; }
        }
    }
}