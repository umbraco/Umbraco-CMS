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

        /// <summary>
        /// Adds the container (listview) tab to the document
        /// </summary>
        /// <typeparam name="TPersisted"></typeparam>
        /// <param name="display"></param>
        /// <param name="entityType">This must be either 'content' or 'media'</param>
        /// <param name="dataTypeService"></param>
        /// <param name="localizedTextService"></param>
        internal static void AddListView<TPersisted>(TabbedContentItem<ContentPropertyDisplay, TPersisted> display, string entityType, IDataTypeService dataTypeService, ILocalizedTextService localizedTextService)
             where TPersisted : IContentBase
        {
            int dtdId;
            var customDtdName = Constants.Conventions.DataTypes.ListViewPrefix + display.ContentTypeAlias;
            switch (entityType)
            {
                case "content":
                    dtdId = Constants.DataTypes.DefaultContentListView;

                    break;
                case "media":
                    dtdId = Constants.DataTypes.DefaultMediaListView;
                    break;
                case "member":
                    dtdId = Constants.DataTypes.DefaultMembersListView;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(entityType), "entityType does not match a required value");
            }

            //first try to get the custom one if there is one
            var dt = dataTypeService.GetDataType(customDtdName)
                ?? dataTypeService.GetDataType(dtdId);

            if (dt == null)
            {
                throw new InvalidOperationException("No list view data type was found for this document type, ensure that the default list view data types exists and/or that your custom list view data type exists");
            }

            var editor = Current.PropertyEditors[dt.EditorAlias];
            if (editor == null)
            {
                throw new NullReferenceException("The property editor with alias " + dt.EditorAlias + " does not exist");
            }

            var listViewTab = new Tab<ContentPropertyDisplay>
            {
                Alias = Constants.Conventions.PropertyGroups.ListViewGroupName,
                Label = localizedTextService.Localize("content/childItems"),
                Id = display.Tabs.Count() + 1,
                IsActive = true
            };

            var listViewConfig = editor.GetConfigurationEditor().ToConfigurationEditor(dt.Configuration);
            //add the entity type to the config
            listViewConfig["entityType"] = entityType;

            //Override Tab Label if tabName is provided
            if (listViewConfig.ContainsKey("tabName"))
            {
                var configTabName = listViewConfig["tabName"];
                if (configTabName != null && string.IsNullOrWhiteSpace(configTabName.ToString()) == false)
                    listViewTab.Label = configTabName.ToString();
            }

            var listViewProperties = new List<ContentPropertyDisplay>();
            listViewProperties.Add(new ContentPropertyDisplay
            {
                Alias = $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}containerView",
                Label = "",
                Value = null,
                View = editor.GetValueEditor().View,
                HideLabel = true,
                Config = listViewConfig
            });
            listViewTab.Properties = listViewProperties;

            SetChildItemsTabPosition(display, listViewConfig, listViewTab);
        }

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

        private static void SetChildItemsTabPosition<TPersisted>(TabbedContentItem<ContentPropertyDisplay, TPersisted> display,
                IDictionary<string, object> listViewConfig,
                Tab<ContentPropertyDisplay> listViewTab)
            where TPersisted : IContentBase
        {
            // Find position of tab from config
            var tabIndexForChildItems = GetTabNumberFromConfig(listViewConfig);
            if (tabIndexForChildItems != -1)
            {
                // Tab position is recorded 1-based but we insert into collection 0-based
                tabIndexForChildItems--;

                // Ensure within bounds
                if (tabIndexForChildItems < 0)
                {
                    tabIndexForChildItems = 0;
                }

                if (tabIndexForChildItems > display.Tabs.Count())
                {
                    tabIndexForChildItems = display.Tabs.Count();
                }
            }
            else tabIndexForChildItems = 0;

            // Recreate tab list with child items tab at configured position
            var tabs = new List<Tab<ContentPropertyDisplay>>();
            tabs.AddRange(display.Tabs);
            tabs.Insert(tabIndexForChildItems, listViewTab);
            display.Tabs = tabs;
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
        /// <param name="umbracoContext"></param>
        /// <param name="content"></param>
        /// <param name="tabs"></param>
        /// <param name="context"></param>
        /// <remarks>
        /// The generic properties tab is responsible for 
        /// setting up the properties such as Created date, updated date, template selected, etc...
        /// </remarks>
        protected virtual void MapGenericProperties(UmbracoContext umbracoContext, IContentBase content, List<Tab<ContentPropertyDisplay>> tabs, ResolutionContext context)
        {
            // add the generic properties tab, for properties that don't belong to a tab
            // get the properties, map and translate them, then add the tab
            var noGroupProperties = content.GetNonGroupedProperties()
                .Where(x => IgnoreProperties.Contains(x.Alias) == false) // skip ignored
                .ToList();
            var genericproperties = MapProperties(umbracoContext, content, noGroupProperties, context);

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

            //Show or hide properties tab based on wether it has or not any properties 
            if (genericProps.Properties.Any() == false)
            {
                //loop throug the tabs, remove the one with the id of zero and exit the loop
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
        /// <param name="umbracoContext"></param>
        /// <param name="content"></param>
        /// <param name="properties"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual List<ContentPropertyDisplay> MapProperties(UmbracoContext umbracoContext, IContentBase content, List<Property> properties, ResolutionContext context)
        {
            //we need to map this way to pass the context through, I don't like it but we'll see what AutoMapper says: https://github.com/AutoMapper/AutoMapper/issues/2588
            var result = context.Mapper.Map<IEnumerable<Property>, IEnumerable<ContentPropertyDisplay>>(
                    // Sort properties so items from different compositions appear in correct order (see U4-9298). Map sorted properties.
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
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public TabsAndPropertiesResolver(IUmbracoContextAccessor umbracoContextAccessor, ILocalizedTextService localizedTextService)
            : base(localizedTextService)
        {
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
        }

        public TabsAndPropertiesResolver(IUmbracoContextAccessor umbracoContextAccessor, ILocalizedTextService localizedTextService, IEnumerable<string> ignoreProperties)
            : base(localizedTextService, ignoreProperties)
        {
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
        }

        public virtual IEnumerable<Tab<ContentPropertyDisplay>> Resolve(TSource source, TDestination destination, IEnumerable<Tab<ContentPropertyDisplay>> destMember, ResolutionContext context)
        {
            var umbracoContext = _umbracoContextAccessor.UmbracoContext;
            if (umbracoContext == null) throw new InvalidOperationException("Cannot resolve value without an UmbracoContext available");

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
                var mappedProperties = MapProperties(umbracoContext, source, properties, context);

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

            MapGenericProperties(umbracoContext, source, tabs, context);

            // activate the first tab, if any
            if (tabs.Count > 0)
                tabs[0].IsActive = true;

            return tabs;
        }
    }
}
