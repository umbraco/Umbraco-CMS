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
                var propertyTypes = database.Fetch<PropertyTypeDto>("WHERE propertyTypeGroupId > 0");
                var propertyGroups = database.Fetch<PropertyTypeGroupDto>("WHERE id > 0");

                foreach (var propertyType in propertyTypes)
                {
                    //Get the PropertyTypeGroup that the current PropertyType references
                    var parentPropertyTypeGroup = propertyGroups.FirstOrDefault(x => x.Id == propertyType.PropertyTypeGroupId);
                    if (parentPropertyTypeGroup != null)
                    {
                        //If the ContentType is the same on the PropertyType and the PropertyTypeGroup the group is valid and we skip to the next
                        if (parentPropertyTypeGroup.ContentTypeNodeId == propertyType.ContentTypeId) continue;

                        //Check if the 'new' PropertyTypeGroup has already been created
                        var existingPropertyTypeGroup =
                            propertyGroups.FirstOrDefault(
                                x =>
                                x.ParentGroupId == parentPropertyTypeGroup.Id && x.Text == parentPropertyTypeGroup.Text &&
                                x.ContentTypeNodeId == propertyType.ContentTypeId);

                        //This should ensure that we don't create duplicate groups for a single ContentType
                        if (existingPropertyTypeGroup == null)
                        {

                            //Create a new PropertyTypeGroup that references the parent group that the PropertyType was referencing pre-6.0.1
                            var propertyGroup = new PropertyTypeGroupDto
                                                    {
                                                        ContentTypeNodeId = propertyType.ContentTypeId,
                                                        ParentGroupId = parentPropertyTypeGroup.Id,
                                                        Text = parentPropertyTypeGroup.Text,
                                                        SortOrder = parentPropertyTypeGroup.SortOrder
                                                    };

                            //Save the PropertyTypeGroup in the database and update the list of groups with this new group
                            int id = Convert.ToInt16(database.Insert(propertyGroup));
                            propertyGroup.Id = id;
                            propertyGroups.Add(propertyGroup);
                            //Update the reference to the new PropertyTypeGroup on the current PropertyType
                            propertyType.PropertyTypeGroupId = id;
                            database.Update(propertyType);
                        }
                        else
                        {
                            //Update the reference to the existing PropertyTypeGroup on the current PropertyType
                            propertyType.PropertyTypeGroupId = existingPropertyTypeGroup.Id;
                            database.Update(propertyType);
                        }
                    }
                }
            }

            return string.Empty;
        }
    }
}