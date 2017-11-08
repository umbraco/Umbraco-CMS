using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using umbraco;

namespace Umbraco.Web.Models.Mapping
{
    /// <summary>
    /// Creates the tabs collection with properties assigned for display models
    /// </summary>
    internal class TabsAndPropertiesResolver : ValueResolver<IContentBase, IEnumerable<Tab<ContentPropertyDisplay>>>
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
        /// Maps properties on to the generic properties tab
        /// </summary>
        /// <param name="content"></param>
        /// <param name="display"></param>
        /// <param name="localizedTextService"></param>
        /// <param name="customProperties">
        /// Any additional custom properties to assign to the generic properties tab. 
        /// </param>
        /// <param name="onGenericPropertiesMapped"></param>
        /// <remarks>
        /// The generic properties tab is mapped during AfterMap and is responsible for 
        /// setting up the properties such as Created date, updated date, template selected, etc...
        /// </remarks>
        public static void MapGenericProperties<TPersisted>(
            TPersisted content,
            ContentItemDisplayBase<ContentPropertyDisplay, TPersisted> display,
            ILocalizedTextService localizedTextService,
            IEnumerable<ContentPropertyDisplay> customProperties = null,
            Action<List<ContentPropertyDisplay>> onGenericPropertiesMapped = null)
            where TPersisted : IContentBase
        {
            var genericProps = display.Tabs.Single(x => x.Id == 0);

            //store the current props to append to the newly inserted ones
            var currProps = genericProps.Properties.ToArray();

            var labelEditor = PropertyEditorResolver.Current.GetByAlias(Constants.PropertyEditors.NoEditAlias).ValueEditor.View;

            var contentProps = new List<ContentPropertyDisplay>
            {
                new ContentPropertyDisplay
                {
                    Alias = string.Format("{0}id", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                    Label = "Id",
                    Value = Convert.ToInt32(display.Id).ToInvariantString() + "<br/><small class='muted'>" + display.Key + "</small>",
                    View = labelEditor
                },
                new ContentPropertyDisplay
                {
                    Alias = string.Format("{0}creator", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                    Label = localizedTextService.Localize("content/createBy"),
                    Description = localizedTextService.Localize("content/createByDesc"),
                    Value = display.Owner.Name,
                    View = labelEditor
                },
                new ContentPropertyDisplay
                {
                    Alias = string.Format("{0}createdate", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                    Label = localizedTextService.Localize("content/createDate"),
                    Description = localizedTextService.Localize("content/createDateDesc"),
                    Value = display.CreateDate.ToString(CultureInfo.CurrentCulture),
                    View = labelEditor
                },
                new ContentPropertyDisplay
                {
                    Alias = string.Format("{0}updatedate", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                    Label = localizedTextService.Localize("content/updateDate"),
                    Description = localizedTextService.Localize("content/updateDateDesc"),
                    Value = display.UpdateDate.ToString(CultureInfo.CurrentCulture),
                    View = labelEditor
                }
            };

            if (customProperties != null)
            {
                //add the custom ones
                contentProps.AddRange(customProperties);
            }

            //now add the user props
            contentProps.AddRange(currProps);

            //callback
            if (onGenericPropertiesMapped != null)
            {
                onGenericPropertiesMapped(contentProps);
            }

            //re-assign
            genericProps.Properties = contentProps;
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

        protected override IEnumerable<Tab<ContentPropertyDisplay>> ResolveCore(IContentBase content)
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

                // Sort properties so items from different compositions appear in correct order (see U4-9298). Map sorted properties.
                var mappedProperties = Mapper.Map<IEnumerable<Property>, IEnumerable<ContentPropertyDisplay>>(properties.OrderBy(prop => prop.PropertyType.SortOrder));

                TranslateProperties(mappedProperties);

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

            // add the generic properties tab, for properties that don't belong to a tab
            // get the properties, map and translate them, then add the tab
            var noGroupProperties = content.GetNonGroupedProperties()
                .Where(x => IgnoreProperties.Contains(x.Alias) == false); // skip ignored
            var genericproperties = Mapper.Map<IEnumerable<Property>, IEnumerable<ContentPropertyDisplay>>(noGroupProperties).ToList();
            TranslateProperties(genericproperties);

            tabs.Add(new Tab<ContentPropertyDisplay>
            {
                Id = 0,
                Label = _localizedTextService.Localize("general/properties"),
                Alias = "Generic properties",
                Properties = genericproperties
            });

            // activate the first tab
            tabs.First().IsActive = true;

            return tabs;
        }

        private void TranslateProperties(IEnumerable<ContentPropertyDisplay> properties)
        {
            // Not sure whether it's a good idea to add this to the ContentPropertyDisplay mapper
            foreach (var prop in properties)
            {
                prop.Label = _localizedTextService.UmbracoDictionaryTranslate(prop.Label);
                prop.Description = _localizedTextService.UmbracoDictionaryTranslate(prop.Description);
            }
        }
    }
}
