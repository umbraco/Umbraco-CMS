﻿using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.ContentApps
{
    internal class ListViewContentAppDefinition : IContentAppDefinition
    {
        // see note on ContentApp
        private const int Weight = -666;

        private readonly IDataTypeService _dataTypeService;
        private readonly PropertyEditorCollection _propertyEditors;

        public ListViewContentAppDefinition(IDataTypeService dataTypeService, PropertyEditorCollection propertyEditors)
        {
            _dataTypeService = dataTypeService;
            _propertyEditors = propertyEditors;
        }

        public ContentApp GetContentAppFor(object o)
        {
            string contentTypeAlias, entityType;

            switch (o)
            {
                case IContent content when !content.ContentType.IsContainer:
                    return null;
                case IContent content:
                    contentTypeAlias = content.ContentType.Alias;
                    entityType = "content";
                    break;
                case IMedia media when !media.ContentType.IsContainer && media.ContentType.Alias != Core.Constants.Conventions.MediaTypes.Folder:
                    return null;
                case IMedia media:
                    contentTypeAlias = media.ContentType.Alias;
                    entityType = "media";
                    break;
                default:
                    throw new NotSupportedException($"Object type {o.GetType()} is not supported here.");
            }

            return CreateContentApp(_dataTypeService, _propertyEditors, entityType, contentTypeAlias);
        }

        public static ContentApp CreateContentApp(IDataTypeService dataTypeService, PropertyEditorCollection propertyEditors, string entityType, string contentTypeAlias)
        {
            var contentApp = new ContentApp
            {
                Alias = "umbListView",
                Name = "Child items",
                Icon = "icon-list",
                View = "views/content/apps/listview/listview.html",
                Weight = Weight
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
                if (configTabName != null && String.IsNullOrWhiteSpace(configTabName.ToString()) == false)
                    contentApp.Name = configTabName.ToString();
            }

            //This is the view model used for the list view app
            contentApp.ViewModel = new List<ContentPropertyDisplay>
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

            return contentApp;
        }
    }
}
