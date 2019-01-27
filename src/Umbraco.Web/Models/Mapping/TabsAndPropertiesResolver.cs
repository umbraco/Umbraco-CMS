using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Composing;

namespace Umbraco.Web.Models.Mapping
{
    internal abstract class TabsAndPropertiesResolver
    {
        protected ILocalizedTextService LocalizedTextService { get; }
        protected IEnumerable<string> IgnoreProperties { get; set; }

        protected TabsAndPropertiesResolver(ILocalizedTextService localizedTextService)
        {
            LocalizedTextService = localizedTextService ?? throw new ArgumentNullException(nameof(localizedTextService));
            IgnoreProperties = new List<string>();
        }

        protected TabsAndPropertiesResolver(ILocalizedTextService localizedTextService, IEnumerable<string> ignoreProperties)
            : this(localizedTextService)
        {
            IgnoreProperties = ignoreProperties ?? throw new ArgumentNullException(nameof(ignoreProperties));
        }
        
        // TODO: This should deserialize to ListViewConfiguration
        private static int GetTabNumberFromConfig(IDictionary<string, object> listViewConfig)
        {
            if (!listViewConfig.TryGetValue("displayAtTabNumber", out var displayTabNum))
                return -1;
            switch (displayTabNum)
            {
                case int i:
                    return i;
                case string s when int.TryParse(s, out var parsed):
                    return parsed;
            }
            return -1;
        }

        /// <summary>
        /// Returns a collection of custom generic properties that exist on the generic properties tab
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerable<ContentPropertyDisplay> GetCustomGenericProperties(IContentBase content)
        {
            return Enumerable.Empty<ContentPropertyDisplay>();
        }

        /// <summary>
        /// Maps properties on to the generic properties tab
        /// </summary>
        /// <param name="content"></param>
        /// <param name="tabs"></param>
        /// <param name="context"></param>
        /// <remarks>
        /// The generic properties tab is responsible for 
        /// setting up the properties such as Created date, updated date, template selected, etc...
        /// </remarks>
        protected virtual void MapGenericProperties(IContentBase content, List<Tab<ContentPropertyDisplay>> tabs, ResolutionContext context)
        {
            // add the generic properties tab, for properties that don't belong to a tab
            // get the properties, map and translate them, then add the tab
            var noGroupProperties = content.GetNonGroupedProperties()
                .Where(x => IgnoreProperties.Contains(x.Alias) == false) // skip ignored
                .ToList();
            var genericproperties = MapProperties(content, noGroupProperties, context);

            tabs.Add(new Tab<ContentPropertyDisplay>
            {
                Id = 0,
                Label = LocalizedTextService.Localize("general/properties"),
                Alias = "Generic properties",
                Properties = genericproperties
            });

            var genericProps = tabs.Single(x => x.Id == 0);

            //store the current props to append to the newly inserted ones
            var currProps = genericProps.Properties.ToArray();

            var contentProps = new List<ContentPropertyDisplay>();

            var customProperties = GetCustomGenericProperties(content);
            if (customProperties != null)
            {
                //add the custom ones
                contentProps.AddRange(customProperties);
            }

            //now add the user props
            contentProps.AddRange(currProps);

            //re-assign
            genericProps.Properties = contentProps;

            //Show or hide properties tab based on whether it has or not any properties 
            if (genericProps.Properties.Any() == false)
            {
                //loop through the tabs, remove the one with the id of zero and exit the loop
                for (var i = 0; i < tabs.Count; i++)
                {
                    if (tabs[i].Id != 0) continue;
                    tabs.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// Maps a list of <see cref="Property"/> to a list of <see cref="ContentPropertyDisplay"/>
        /// </summary>
        /// <param name="content"></param>
        /// <param name="properties"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual List<ContentPropertyDisplay> MapProperties(IContentBase content, List<Property> properties, ResolutionContext context)
        {
            //we need to map this way to pass the context through, I don't like it but we'll see what AutoMapper says: https://github.com/AutoMapper/AutoMapper/issues/2588
            var result = context.Mapper.Map<IEnumerable<Property>, IEnumerable<ContentPropertyDisplay>>(
                    properties.OrderBy(prop => prop.PropertyType.SortOrder),
                    null,
                    context)
                .ToList();

            return result;
        }
    }

    /// <summary>
    /// Creates the tabs collection with properties assigned for display models
    /// </summary>
    internal class TabsAndPropertiesResolver<TSource, TDestination> : TabsAndPropertiesResolver, IValueResolver<TSource, TDestination, IEnumerable<Tab<ContentPropertyDisplay>>>
        where TSource : IContentBase
    {
        public TabsAndPropertiesResolver(ILocalizedTextService localizedTextService)
            : base(localizedTextService)
        { }

        public virtual IEnumerable<Tab<ContentPropertyDisplay>> Resolve(TSource source, TDestination destination, IEnumerable<Tab<ContentPropertyDisplay>> destMember, ResolutionContext context)
        {
            var tabs = new List<Tab<ContentPropertyDisplay>>();

            // add the tabs, for properties that belong to a tab
            // need to aggregate the tabs, as content.PropertyGroups contains all the composition tabs,
            // and there might be duplicates (content does not work like contentType and there is no
            // content.CompositionPropertyGroups).
            var groupsGroupsByName = source.PropertyGroups.OrderBy(x => x.SortOrder).GroupBy(x => x.Name);
            foreach (var groupsByName in groupsGroupsByName)
            {
                var properties = new List<Property>();

                // merge properties for groups with the same name
                foreach (var group in groupsByName)
                {
                    var groupProperties = source.GetPropertiesForGroup(group)
                        .Where(x => IgnoreProperties.Contains(x.Alias) == false); // skip ignored

                    properties.AddRange(groupProperties);
                }

                if (properties.Count == 0)
                    continue;

                //map the properties
                var mappedProperties = MapProperties(source, properties, context);

                // add the tab
                // we need to pick an identifier... there is no "right" way...
                var g = groupsByName.FirstOrDefault(x => x.Id == source.ContentTypeId) // try local
                    ?? groupsByName.First(); // else pick one randomly
                var groupId = g.Id;
                var groupName = groupsByName.Key;
                tabs.Add(new Tab<ContentPropertyDisplay>
                {
                    Id = groupId,
                    Alias = groupName,
                    Label = LocalizedTextService.UmbracoDictionaryTranslate(groupName),
                    Properties = mappedProperties,
                    IsActive = false
                });
            }

            MapGenericProperties(source, tabs, context);

            // activate the first tab, if any
            if (tabs.Count > 0)
                tabs[0].IsActive = true;

            return tabs;
        }
    }
}
