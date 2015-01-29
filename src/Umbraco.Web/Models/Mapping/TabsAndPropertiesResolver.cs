using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Logging;
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
        private ICultureDictionary _cultureDictionary;
        protected IEnumerable<string> IgnoreProperties { get; set; }

        public TabsAndPropertiesResolver()
        {
            IgnoreProperties = new List<string>();
        }

        public TabsAndPropertiesResolver(IEnumerable<string> ignoreProperties)
        {
            if (ignoreProperties == null) throw new ArgumentNullException("ignoreProperties");
            IgnoreProperties = ignoreProperties;
        }

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
        /// setting up the properties such as Created date, updated date, template selected, etc...
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

            var labelEditor = PropertyEditorResolver.Current.GetByAlias(Constants.PropertyEditors.NoEditAlias).ValueEditor.View;

            var contentProps = new List<ContentPropertyDisplay>
                {
                    new ContentPropertyDisplay
                        {
                            Alias = string.Format("{0}id", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                            Label = "Id",
                            Value = Convert.ToInt32(display.Id).ToInvariantString(),
                            View = labelEditor
                        },
                    new ContentPropertyDisplay
                        {
                            Alias = string.Format("{0}creator", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                            Label = ui.Text("content", "createBy"),
                            Description = ui.Text("content", "createByDesc"), //TODO: Localize this
                            Value = display.Owner.Name,
                            View = labelEditor
                        },
                    new ContentPropertyDisplay
                        {
                            Alias = string.Format("{0}createdate", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                            Label = ui.Text("content", "createDate"),
                            Description = ui.Text("content", "createDateDesc"), 
                            Value = display.CreateDate.ToIsoString(),
                            View = labelEditor
                        },
                     new ContentPropertyDisplay
                        {
                            Alias = string.Format("{0}updatedate", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                            Label = ui.Text("content", "updateDate"),
                            Description = ui.Text("content", "updateDateDesc"), 
                            Value = display.UpdateDate.ToIsoString(),
                            View = labelEditor
                        },                    
                    new ContentPropertyDisplay
                        {
                            Alias = string.Format("{0}doctype", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                            Label = ui.Text("content", "documentType"),
                            Value = TranslateItem(display.ContentTypeName, CreateDictionary()),
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

        /// <summary>
        /// Adds the container (listview) tab to the document
        /// </summary>
        /// <typeparam name="TPersisted"></typeparam>
        /// <param name="display"></param>
        /// <param name="entityType">This must be either 'content' or 'media'</param>
        /// <param name="dataTypeService"></param>
        internal static void AddListView<TPersisted>(TabbedContentItem<ContentPropertyDisplay, TPersisted> display, string entityType, IDataTypeService dataTypeService)
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
            listViewTab.Label = ui.Text("content", "childItems");
            listViewTab.Id = 25;
            listViewTab.IsActive = true;

            var listViewConfig = editor.PreValueEditor.ConvertDbToEditor(editor.DefaultPreValues, preVals);
            //add the entity type to the config
            listViewConfig["entityType"] = entityType;

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

            //Is there a better way?
            var tabs = new List<Tab<ContentPropertyDisplay>>();
            tabs.Add(listViewTab);
            tabs.AddRange(display.Tabs);
            display.Tabs = tabs;

        }

        protected override IEnumerable<Tab<ContentPropertyDisplay>> ResolveCore(IContentBase content)
        {
            var aggregateTabs = new List<Tab<ContentPropertyDisplay>>();

            //now we need to aggregate the tabs and properties since we might have duplicate tabs (based on aliases) because
            // of how content composition works. 
            foreach (var propertyGroups in content.PropertyGroups.OrderBy(x => x.SortOrder).GroupBy(x => x.Name))
            {
                var aggregateProperties = new List<ContentPropertyDisplay>();
                
                //add the properties from each composite property group
                foreach (var current in propertyGroups)
                {
                    var propsForGroup = content.GetPropertiesForGroup(current)
                        .Where(x => IgnoreProperties.Contains(x.Alias) == false); //don't include ignored props

                    aggregateProperties.AddRange(
                        Mapper.Map<IEnumerable<Property>, IEnumerable<ContentPropertyDisplay>>(
                            propsForGroup));
                }
                
                if (aggregateProperties.Count == 0)
                    continue;

                    TranslateProperties(aggregateProperties);

                //then we'll just use the root group's data to make the composite tab
                var rootGroup = propertyGroups.First(x => x.ParentId == null);
                aggregateTabs.Add(new Tab<ContentPropertyDisplay>
                    {
                        Id = rootGroup.Id,
                        Alias = rootGroup.Name,
                        Label = TranslateItem(rootGroup.Name),
                        Properties = aggregateProperties,
                        IsActive = false
                    });
            }

            //now add the generic properties tab for any properties that don't belong to a tab
            var orphanProperties = content.GetNonGroupedProperties()
                .Where(x => IgnoreProperties.Contains(x.Alias) == false); //don't include ignored props

            //now add the generic properties tab
            var genericproperties = Mapper.Map<IEnumerable<Property>, IEnumerable<ContentPropertyDisplay>>(orphanProperties).ToList();
            TranslateProperties(genericproperties);

            aggregateTabs.Add(new Tab<ContentPropertyDisplay>
                {
                    Id = 0,
                    Label = ui.Text("general", "properties"),
                    Alias = "Generic properties",
                    Properties = genericproperties
                });

            //set the first tab to active
            aggregateTabs.First().IsActive = true;

            return aggregateTabs;
        }

        private void TranslateProperties(IEnumerable<ContentPropertyDisplay> properties)
        {
            // Not sure whether it's a good idea to add this to the ContentPropertyDisplay mapper
            foreach (var prop in properties)
            {
                prop.Label = TranslateItem(prop.Label);
                prop.Description = TranslateItem(prop.Description);
            }
        }

        // TODO: This should really be centralized and used anywhere globalization applies.
        internal string TranslateItem(string text)
        {
            var cultureDictionary = CultureDictionary;
            return TranslateItem(text, cultureDictionary);
        }

        private static string TranslateItem(string text, ICultureDictionary cultureDictionary)
        {
            if (text == null)
            {
                return null;
            }

            if (text.StartsWith("#") == false)
                return text;

            text = text.Substring(1);
            return cultureDictionary[text].IfNullOrWhiteSpace(text);
        }

        private ICultureDictionary CultureDictionary
        {
            get
            {
                return 
                    _cultureDictionary ?? 
                    (_cultureDictionary = CreateDictionary());
            }
        }

        private static ICultureDictionary CreateDictionary()
        {
            return CultureDictionaryFactoryResolver.Current.Factory.CreateDictionary();
        }
    }
}
