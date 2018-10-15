using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Creates the tabs collection with properties assigned for display models
    /// </summary>
    internal class TabsAndPropertiesResolver<TSource> : IValueResolver
        where TSource : IContentBase
    {
        private readonly ILocalizedTextService _localizedTextService;
        protected IEnumerable<string> IgnoreProperties { get; set; }

        public TabsAndPropertiesResolver(ILocalizedTextService localizedTextService)
        {
            if (localizedTextService == null) throw new ArgumentNullException("localizedTextService");
            _localizedTextService = localizedTextService;
            IgnoreProperties = new List<string>();
        }

        public TabsAndPropertiesResolver(ILocalizedTextService localizedTextService, IEnumerable<string> ignoreProperties)
            : this(localizedTextService)
        {
            if (ignoreProperties == null) throw new ArgumentNullException("ignoreProperties");
            IgnoreProperties = ignoreProperties;
        }

        /// <summary>
        /// Implements the <see cref="IValueResolver"/>
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public ResolutionResult Resolve(ResolutionResult source)
        {
            if (source.Value != null && (source.Value is TSource) == false)
                throw new AutoMapperMappingException(string.Format("Value supplied is of type {0} but expected {1}.\nChange the value resolver source type, or redirect the source value supplied to the value resolver using FromMember.", new object[]
                {
                    source.Value.GetType(),
                    typeof (TSource)
                }));
            return source.New(
                //perform the mapping with the current umbraco context
                ResolveCore(source.Context.GetUmbracoContext(), (TSource)source.Value), typeof(List<Tab<ContentPropertyDisplay>>));
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
                    dtdId = Constants.System.DefaultContentListViewDataTypeId;

                    break;
                case "media":
                    dtdId = Constants.System.DefaultMediaListViewDataTypeId;
                    break;
                case "member":
                    dtdId = Constants.System.DefaultMembersListViewDataTypeId;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("entityType does not match a required value");
            }

            //first try to get the custom one if there is one
            var dt = dataTypeService.GetDataTypeDefinitionByName(customDtdName)
                ?? dataTypeService.GetDataTypeDefinitionById(dtdId);

            if (dt == null)
            {
                throw new InvalidOperationException("No list view data type was found for this document type, ensure that the default list view data types exists and/or that your custom list view data type exists");
            }

            var preVals = dataTypeService.GetPreValuesCollectionByDataTypeId(dt.Id);

            var editor = PropertyEditorResolver.Current.GetByAlias(dt.PropertyEditorAlias);
            if (editor == null)
            {
                throw new NullReferenceException("The property editor with alias " + dt.PropertyEditorAlias + " does not exist");
            }

            var listViewTab = new Tab<ContentPropertyDisplay>();
            listViewTab.Alias = Constants.Conventions.PropertyGroups.ListViewGroupName;
            listViewTab.Label = localizedTextService.Localize("content/childItems");
            listViewTab.Id = display.Tabs.Count() + 1;
            listViewTab.IsActive = true;

            var listViewConfig = editor.PreValueEditor.ConvertDbToEditor(editor.DefaultPreValues, preVals);
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
                Alias = string.Format("{0}containerView", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                Label = "",
                Value = null,
                View = editor.ValueEditor.View,
                HideLabel = true,
                Config = listViewConfig
            });
            listViewTab.Properties = listViewProperties;

            SetChildItemsTabPosition(display, listViewConfig, listViewTab);
        }

        private static void SetChildItemsTabPosition<TPersisted>(TabbedContentItem<ContentPropertyDisplay, TPersisted> display,
                IDictionary<string, object> listViewConfig,
                Tab<ContentPropertyDisplay> listViewTab)
            where TPersisted : IContentBase
        {
            // Find position of tab from config
            var tabIndexForChildItems = 0;
            if (listViewConfig["displayAtTabNumber"] != null && int.TryParse((string)listViewConfig["displayAtTabNumber"], out tabIndexForChildItems))
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

            // Recreate tab list with child items tab at configured position
            var tabs = new List<Tab<ContentPropertyDisplay>>();
            tabs.AddRange(display.Tabs);
            tabs.Insert(tabIndexForChildItems, listViewTab);
            display.Tabs = tabs;
        }

        /// <summary>
        /// Create the list of tabs for the <see cref="IContentBase"/>
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="content">Source value</param>
        /// <returns>Destination</returns>
        protected virtual List<Tab<ContentPropertyDisplay>> ResolveCore(UmbracoContext umbracoContext, TSource content)
        {
            var tabs = new List<Tab<ContentPropertyDisplay>>();

            // add the tabs, for properties that belong to a tab
            // need to aggregate the tabs, as content.PropertyGroups contains all the composition tabs,
            // and there might be duplicates (content does not work like contentType and there is no 
            // content.CompositionPropertyGroups).
            var groupsGroupsByName = content.PropertyGroups.OrderBy(x => x.SortOrder).GroupBy(x => x.Name);
            foreach (var groupsByName in groupsGroupsByName)
            {
                var properties = new List<Property>();

                // merge properties for groups with the same name
                foreach (var group in groupsByName)
                {
                    var groupProperties = content.GetPropertiesForGroup(group)
                        .Where(x => IgnoreProperties.Contains(x.Alias) == false); // skip ignored

                    properties.AddRange(groupProperties);
                }

                if (properties.Count == 0)
                    continue;

                //map the properties
                var mappedProperties = MapProperties(umbracoContext, content, properties);

                // add the tab
                // we need to pick an identifier... there is no "right" way...
                var g = groupsByName.FirstOrDefault(x => x.Id == content.ContentTypeId) // try local
                    ?? groupsByName.First(); // else pick one randomly
                var groupId = g.Id;
                var groupName = groupsByName.Key;
                tabs.Add(new Tab<ContentPropertyDisplay>
                {
                    Id = groupId,
                    Alias = groupName,
                    Label = _localizedTextService.UmbracoDictionaryTranslate(groupName),
                    Properties = mappedProperties,
                    IsActive = false
                });
            }

            MapGenericProperties(umbracoContext, content, tabs);

            // activate the first tab
            if (tabs.Count > 0)
                tabs[0].IsActive = true;

            return tabs;
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
        /// <remarks>
        /// The generic properties tab is responsible for 
        /// setting up the properties such as Created date, updated date, template selected, etc...
        /// </remarks>
        protected virtual void MapGenericProperties(UmbracoContext umbracoContext, IContentBase content, List<Tab<ContentPropertyDisplay>> tabs)
        {
            // add the generic properties tab, for properties that don't belong to a tab
            // get the properties, map and translate them, then add the tab
            var noGroupProperties = content.GetNonGroupedProperties()
                .Where(x => IgnoreProperties.Contains(x.Alias) == false) // skip ignored
                .ToList();
            var genericproperties = MapProperties(umbracoContext, content, noGroupProperties);

            tabs.Add(new Tab<ContentPropertyDisplay>
            {
                Id = 0,
                Label = _localizedTextService.Localize("general/properties"),
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
        /// <returns></returns>
        protected virtual List<ContentPropertyDisplay> MapProperties(UmbracoContext umbracoContext, IContentBase content, List<Property> properties)
        {
            var result = Mapper.Map<IEnumerable<Property>, IEnumerable<ContentPropertyDisplay>>(
                    // Sort properties so items from different compositions appear in correct order (see U4-9298). Map sorted properties.
                    properties.OrderBy(prop => prop.PropertyType.SortOrder))
                .ToList();

            return result;
        }

    }
}
