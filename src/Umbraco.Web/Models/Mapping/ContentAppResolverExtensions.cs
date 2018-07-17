using System;
using System.Collections.Generic;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal static class ContentAppResolverExtensions
    {
        /// <summary>
        /// Helper method to append a list view app to the content app collection
        /// </summary>
        /// <param name="resolver"></param>
        public static void AppendListViewApp(
            this ICollection<ContentApp> list,
            IDataTypeService dataTypeService, PropertyEditorCollection propertyEditors,
            string contentTypeAlias, string entityType)
        {
            var listViewApp = new ContentApp
            {
                Alias = "childItems",
                Name = "Child items",
                Icon = "icon-list",
                View = "views/content/apps/listview/listview.html"
            };

            var customDtdName = Core.Constants.Conventions.DataTypes.ListViewPrefix + contentTypeAlias;
            var dtdId = Core.Constants.DataTypes.DefaultContentListView;
            //first try to get the custom one if there is one
            var dt = dataTypeService.GetDataType(customDtdName)
                ?? dataTypeService.GetDataType(dtdId);

            if (dt == null)
            {
                throw new InvalidOperationException("No list view data type was found for this document type, ensure that the default list view data types exists and/or that your custom list view data type exists");
            }

            var editor = propertyEditors[dt.EditorAlias];
            if (editor == null)
            {
                throw new NullReferenceException("The property editor with alias " + dt.EditorAlias + " does not exist");
            }

            var listViewConfig = editor.GetConfigurationEditor().ToConfigurationEditor(dt.Configuration);
            //add the entity type to the config
            listViewConfig["entityType"] = entityType;

            //Override Tab Label if tabName is provided
            if (listViewConfig.ContainsKey("tabName"))
            {
                var configTabName = listViewConfig["tabName"];
                if (configTabName != null && string.IsNullOrWhiteSpace(configTabName.ToString()) == false)
                    listViewApp.Name = configTabName.ToString();
            }

            //This is the view model used for the list view app
            listViewApp.ViewModel = new List<ContentPropertyDisplay>
                {
                    new ContentPropertyDisplay
                    {
                        Alias = $"{Core.Constants.PropertyEditors.InternalGenericPropertiesPrefix}containerView",
                        Label = "",
                        Value = null,
                        View = editor.GetValueEditor().View,
                        HideLabel = true,
                        Config = listViewConfig
                    }
                };

            list.Add(listViewApp);
        }
    }
    
}
