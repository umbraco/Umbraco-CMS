using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models.ContentEditing;
using umbraco;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Creates the tabs collection with properties assigned for display models
    /// </summary>
    internal class TabsAndPropertiesResolver : ValueResolver<IContentBase, IEnumerable<Tab<ContentPropertyDisplay>>>
    {
        /// <summary>
        /// Maps properties on to the generic properties tab
        /// </summary>
        /// <param name="content"></param>
        /// <param name="display"></param>
        /// <param name="customProperties">
        /// Any additional custom properties to assign to the generic properties tab. 
        /// </param>
        /// <remarks>
        /// The generic properties tab is mapped during AfterMap and is responsible for 
        /// setting up the properties such as Created date, udpated date, template selected, etc...
        /// </remarks>
        public static void MapGenericProperties<TPersisted>(
            TPersisted content, 
            ContentItemDisplayBase<ContentPropertyDisplay, TPersisted> display,
            params ContentPropertyDisplay[] customProperties) 
            where TPersisted : IContentBase
        {
            var genericProps = display.Tabs.Single(x => x.Id == 0);

            //store the current props to append to the newly inserted ones
            var currProps = genericProps.Properties.ToArray();

            var labelEditor = PropertyEditorResolver.Current.GetById(new Guid(Constants.PropertyEditors.NoEdit)).ValueEditor.View;

            var contentProps = new List<ContentPropertyDisplay>
                {
                    new ContentPropertyDisplay
                        {
                            Alias = string.Format("{0}id", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                            Label = "Id",
                            Value = display.Id.ToInvariantString(),
                            View = labelEditor
                        },
                    new ContentPropertyDisplay
                        {
                            Alias = string.Format("{0}creator", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                            Label = ui.Text("content", "createBy"),
                            Description = "Original author", //TODO: Localize this
                            Value = display.Owner.Name,
                            View = labelEditor
                        },
                    new ContentPropertyDisplay
                        {
                            Alias = string.Format("{0}createdate", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                            Label = ui.Text("content", "createDate"),
                            Description = "Date/time this document was created", //TODO: Localize this
                            Value = display.CreateDate.ToIsoString(),
                            View = labelEditor
                        },
                     new ContentPropertyDisplay
                        {
                            Alias = string.Format("{0}updatedate", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                            Label = ui.Text("content", "updateDate"),
                            Description = "Date/time this document was created", //TODO: Localize this
                            Value = display.UpdateDate.ToIsoString(),
                            View = labelEditor
                        },                    
                    new ContentPropertyDisplay
                        {
                            Alias = string.Format("{0}doctype", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                            Label = ui.Text("content", "documentType"),
                            Value = display.ContentTypeName,
                            View = labelEditor
                        }
                };

            //add the custom ones
            contentProps.AddRange(customProperties);

            //now add the user props
            contentProps.AddRange(currProps);

            //re-assign
            genericProps.Properties = contentProps;
        }

        protected override IEnumerable<Tab<ContentPropertyDisplay>> ResolveCore(IContentBase content)
        {
            var aggregateTabs = new List<Tab<ContentPropertyDisplay>>();

            //now we need to aggregate the tabs and properties since we might have duplicate tabs (based on aliases) because
            // of how content composition works. 
            foreach (var propertyGroups in content.PropertyGroups.GroupBy(x => x.Name))
            {
                var aggregateProperties = new List<ContentPropertyDisplay>();

                //there will always be one group with a null parent id (the top-most)
                //then we'll iterate over all of the groups and ensure the properties are
                //added in order so that when they render they are rendered with highest leve
                //parent properties first.
                int? currentParentId = null;
                for (var i = 0; i < propertyGroups.Count(); i++)
                {
                    var current = propertyGroups.Single(x => x.ParentId == currentParentId);
                    aggregateProperties.AddRange(
                        Mapper.Map<IEnumerable<Property>, IEnumerable<ContentPropertyDisplay>>(
                            content.GetPropertiesForGroup(current)));
                    currentParentId = current.Id;
                }

                //then we'll just use the root group's data to make the composite tab
                var rootGroup = propertyGroups.Single(x => x.ParentId == null);
                aggregateTabs.Add(new Tab<ContentPropertyDisplay>
                    {
                        Id = rootGroup.Id,
                        Alias = rootGroup.Name,
                        Label = rootGroup.Name,
                        Properties = aggregateProperties,
                        IsActive = false
                    });
            }

            //now add the generic properties tab for any properties that don't belong to a tab
            var orphanProperties = content.GetNonGroupedProperties();

            //now add the generic properties tab
            aggregateTabs.Add(new Tab<ContentPropertyDisplay>
                {
                    Id = 0,
                    Label = "Generic properties",
                    Alias = "Generic properties",
                    Properties = Mapper.Map<IEnumerable<Property>, IEnumerable<ContentPropertyDisplay>>(orphanProperties)
                });

            //set the first tab to active
            aggregateTabs.First().IsActive = true;

            return aggregateTabs;
        }
    }
}
