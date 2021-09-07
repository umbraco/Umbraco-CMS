using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Dictionary;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.Mapping
{
    public abstract class TabsAndPropertiesMapper
    {
        protected ICultureDictionary CultureDictionary { get; }
        protected ILocalizedTextService LocalizedTextService { get; }
        protected IEnumerable<string> IgnoreProperties { get; set; }

        protected TabsAndPropertiesMapper(ICultureDictionary cultureDictionary, ILocalizedTextService localizedTextService)
            : this(cultureDictionary, localizedTextService, new List<string>())
        { }

        protected TabsAndPropertiesMapper(ICultureDictionary cultureDictionary, ILocalizedTextService localizedTextService, IEnumerable<string> ignoreProperties)
        {
            CultureDictionary = cultureDictionary ?? throw new ArgumentNullException(nameof(cultureDictionary));
            LocalizedTextService = localizedTextService ?? throw new ArgumentNullException(nameof(localizedTextService));
            IgnoreProperties = ignoreProperties ?? throw new ArgumentNullException(nameof(ignoreProperties));
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
        protected virtual void MapGenericProperties(IContentBase content, List<Tab<ContentPropertyDisplay>> tabs, MapperContext context)
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
                Label = LocalizedTextService.Localize("general", "properties"),
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
        protected virtual List<ContentPropertyDisplay> MapProperties(IContentBase content, List<IProperty> properties, MapperContext context)
        {
            return context.MapEnumerable<IProperty, ContentPropertyDisplay>(properties.OrderBy(x => x.PropertyType.SortOrder));
        }
    }

    /// <summary>
    /// Creates the tabs collection with properties assigned for display models
    /// </summary>
    public class TabsAndPropertiesMapper<TSource> : TabsAndPropertiesMapper
        where TSource : IContentBase
    {
        private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;

        public TabsAndPropertiesMapper(ICultureDictionary cultureDictionary, ILocalizedTextService localizedTextService, IContentTypeBaseServiceProvider contentTypeBaseServiceProvider)
            : base(cultureDictionary, localizedTextService)
        {
            _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider ?? throw new ArgumentNullException(nameof(contentTypeBaseServiceProvider));
        }

        public virtual IEnumerable<Tab<ContentPropertyDisplay>> Map(TSource source, MapperContext context)
        {
            var tabs = new List<Tab<ContentPropertyDisplay>>();

            // Property groups only exist on the content type (as it's only used for display purposes)
            var contentType = _contentTypeBaseServiceProvider.GetContentTypeOf(source);

            // Merge the groups, as compositions can introduce duplicate aliases
            var groups = contentType.CompositionPropertyGroups.OrderBy(x => x.SortOrder).ToArray();
            var parentAliases = groups.Select(x => x.GetParentAlias()).Distinct().ToArray();
            foreach (var groupsByAlias in groups.GroupBy(x => x.Alias))
            {
                var properties = new List<IProperty>();

                // Merge properties for groups with the same alias
                foreach (var group in groupsByAlias)
                {
                    var groupProperties = source.GetPropertiesForGroup(group)
                        .Where(x => IgnoreProperties.Contains(x.Alias) == false); // Skip ignored properties

                    properties.AddRange(groupProperties);
                }

                if (properties.Count == 0 && !parentAliases.Contains(groupsByAlias.Key))
                    continue;

                // Map the properties
                var mappedProperties = MapProperties(source, properties, context);

                // Add the tab (the first is closest to the content type, e.g. local, then direct composition)
                var g = groupsByAlias.First();

                tabs.Add(new Tab<ContentPropertyDisplay>
                {
                    Id = g.Id,
                    Key = g.Key,
                    Type = (int)g.Type,
                    Alias = g.Alias,
                    Label = LocalizedTextService.UmbracoDictionaryTranslate(CultureDictionary, g.Name),
                    Properties = mappedProperties
                });
            }

            MapGenericProperties(source, tabs, context);

            // Activate the first tab, if any
            if (tabs.Count > 0)
                tabs[0].IsActive = true;

            return tabs;
        }
    }
}
