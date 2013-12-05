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

        /// <summary>
        /// Adds the container (listview) tab to the document
        /// </summary>
        /// <typeparam name="TPersisted"></typeparam>
        /// <param name="display"></param>
        /// <param name="entityType">This must be either 'content' or 'media'</param>
        internal static void AddContainerView<TPersisted>(TabbedContentItem<ContentPropertyDisplay, TPersisted> display, string entityType)
             where TPersisted : IContentBase
        {
            var listViewTab = new Tab<ContentPropertyDisplay>();
            listViewTab.Alias = "umbContainerView";
            listViewTab.Label = ui.Text("content", "childItems");
            listViewTab.Id = 25;
            listViewTab.IsActive = true;

            var listViewProperties = new List<ContentPropertyDisplay>();
            listViewProperties.Add(new ContentPropertyDisplay
            {
                Alias = string.Format("{0}containerView", Constants.PropertyEditors.InternalGenericPropertiesPrefix),
                Label = "",
                Value = null,
                View = "listview",
                HideLabel = true,
                Config = new Dictionary<string, object>
                    {
                        {"entityType", entityType}
                    }
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
                
                //then we'll just use the root group's data to make the composite tab
                var rootGroup = propertyGroups.Single(x => x.ParentId == null);
                aggregateTabs.Add(new Tab<ContentPropertyDisplay>
                    {
                        Id = rootGroup.Id,
                        Alias = rootGroup.Name,
                        Label = TranslateTab(rootGroup.Name),
                        Properties = aggregateProperties,
                        IsActive = false
                    });
            }

            //now add the generic properties tab for any properties that don't belong to a tab
            var orphanProperties = content.GetNonGroupedProperties()
                .Where(x => IgnoreProperties.Contains(x.Alias) == false); //don't include ignored props

            //now add the generic properties tab
            aggregateTabs.Add(new Tab<ContentPropertyDisplay>
                {
                    Id = 0,
                    Label = ui.Text("general", "properties"),
                    Alias = "Generic properties",
                    Properties = Mapper.Map<IEnumerable<Property>, IEnumerable<ContentPropertyDisplay>>(orphanProperties)
                });


            //set the first tab to active
            aggregateTabs.First().IsActive = true;

            return aggregateTabs;
        }

        private string TranslateTab(string tabName)
        {
            
            if (!tabName.StartsWith("#"))
                return tabName;

            return tabName.Substring(1);

            /*
             * The below currently doesnt work on my machine, since the dictonary always creates an entry with lang id = 0, but I dont have a lang id zero
             * so the query always fails, which is odd
             * 
            var local = ApplicationContext.Current.Services.LocalizationService;
            var dic = local.GetDictionaryItemByKey(tabName);
            if (dic == null || !dic.Translations.Any())
                return tabName;

            var lang = local.GetLanguageByCultureCode(UmbracoContext.Current.Security.CurrentUser.Language);
            if (lang == null)
                return tabName;


            var translation = dic.Translations.Where(x => x.Language == lang).FirstOrDefault();
            if (translation == null)
                return tabName;

            return translation.Value;*/
        }
    }
}
