using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Creates the tabs collection with properties assigned for display models
    /// </summary>
    internal class TabsAndPropertiesResolver : ValueResolver<IContentBase, IEnumerable<Tab<ContentPropertyDisplay>>>
    {
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