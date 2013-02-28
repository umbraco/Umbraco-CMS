using System;
using System.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSixZeroOne
{
    [Migration("6.0.1", 0, GlobalSettings.UmbracoMigrationName)]
    public class UpdatePropertyTypesAndGroups : MigrationBase
    {
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
                var propertyTypes = database.Fetch<PropertyTypeDto>("WHERE propertyTypeGroupId > 0");
                var propertyGroups = database.Fetch<PropertyTypeGroupDto>("WHERE id > 0");

                foreach (var propertyType in propertyTypes)
                {
                    var parentPropertyTypeGroup = propertyGroups.FirstOrDefault(x => x.Id == propertyType.PropertyTypeGroupId);
                    if (parentPropertyTypeGroup != null)
                    {
                        if (parentPropertyTypeGroup.ContentTypeNodeId == propertyType.ContentTypeId) continue;

                        var propertyGroup = new PropertyTypeGroupDto
                                                {
                                                    ContentTypeNodeId = propertyType.ContentTypeId,
                                                    ParentGroupId = parentPropertyTypeGroup.Id,
                                                    Text = parentPropertyTypeGroup.Text,
                                                    SortOrder = parentPropertyTypeGroup.SortOrder
                                                };

                        int id = Convert.ToInt16(database.Insert(propertyGroup));
                        propertyType.PropertyTypeGroupId = id;
                        database.Update(propertyType);
                    }
                }
            }

            return string.Empty;
        }
    }
}