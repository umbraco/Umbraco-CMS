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
            if (base.Context != null && base.Context.Database != null)
            {
                var propertyTypes = base.Context.Database.Fetch<PropertyTypeDto>("WHERE propertyTypeGroupId > 0");
                var propertyGroups = base.Context.Database.Fetch<PropertyTypeGroupDto>("WHERE id > 0");

                foreach (var propertyType in propertyTypes)
                {
                    var parentPropertyTypeGroup = propertyGroups.FirstOrDefault(x => x.Id == propertyType.PropertyTypeGroupId);
                    if (parentPropertyTypeGroup != null)
                    {
                        if(parentPropertyTypeGroup.ContentTypeNodeId == propertyType.ContentTypeId) continue;

                        var propertyGroup = new PropertyTypeGroupDto
                                                {
                                                    ContentTypeNodeId = propertyType.ContentTypeId,
                                                    ParentGroupId = parentPropertyTypeGroup.Id,
                                                    Text = parentPropertyTypeGroup.Text,
                                                    SortOrder = parentPropertyTypeGroup.SortOrder
                                                };

                        int id = Convert.ToInt16(base.Context.Database.Insert(propertyGroup));
                        propertyType.PropertyTypeGroupId = id;
                        base.Context.Database.Update(propertyType);
                    }
                }
            }
        }

        public override void Down()
        {
            
        }
    }
}