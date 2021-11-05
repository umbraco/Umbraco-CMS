using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Composing;

namespace Umbraco.Web.Models.Mapping
{
    internal abstract class TabsAndPropertiesMapper
    {
        protected ILocalizedTextService LocalizedTextService { get; }

        protected IEnumerable<string> IgnoreProperties { get; set; }

        protected TabsAndPropertiesMapper(ILocalizedTextService localizedTextService)
            : this(localizedTextService, new List<string>())
        { }

        protected TabsAndPropertiesMapper(ILocalizedTextService localizedTextService, IEnumerable<string> ignoreProperties)
        {
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
            var genericProperties = MapProperties(content, noGroupProperties, context);

            var customProperties = GetCustomGenericProperties(content);
            if (customProperties != null)
            {
                genericProperties.AddRange(customProperties);
            }

            if (genericProperties.Count > 0)
            {
                tabs.Add(new Tab<ContentPropertyDisplay>
                {
                    Id = 0,
                    Label = LocalizedTextService.Localize("general", "properties"),
                    Alias = "Generic properties",
                    Properties = genericProperties
                });
            }
        }

        /// <summary>
        /// Maps a list of <see cref="Property"/> to a list of <see cref="ContentPropertyDisplay"/>
        /// </summary>
        /// <param name="content"></param>
        /// <param name="properties"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual List<ContentPropertyDisplay> MapProperties(IContentBase content, List<Property> properties, MapperContext context)
        {
            return context.MapEnumerable<Property, ContentPropertyDisplay>(properties.OrderBy(x => x.PropertyType.SortOrder));
        }
    }

    /// <summary>
    /// Creates the tabs collection with properties assigned for display models
    /// </summary>
    internal class TabsAndPropertiesMapper<TSource> : TabsAndPropertiesMapper
        where TSource : IContentBase
    {
        public TabsAndPropertiesMapper(ILocalizedTextService localizedTextService)
            : base(localizedTextService)
        { }

        public virtual IEnumerable<Tab<ContentPropertyDisplay>> Map(TSource source, MapperContext context)
        {
            var tabs = new List<Tab<ContentPropertyDisplay>>();

            // Property groups only exist on the content type (as it's only used for display purposes)
            var contentType = Current.Services.ContentTypeBaseServices.GetContentTypeOf(source);

            // Merge the groups, as compositions can introduce duplicate aliases
            var groups = contentType.CompositionPropertyGroups.OrderBy(x => x.SortOrder).ToArray();
            var parentAliases = groups.Select(x => x.GetParentAlias()).Distinct().ToArray();
            foreach (var groupsByAlias in groups.GroupBy(x => x.Alias))
            {
                var properties = new List<Property>();

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
                    Label = LocalizedTextService.UmbracoDictionaryTranslate(g.Name),
                    Alias = g.Alias,
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
