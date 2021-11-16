using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.ContentApps
{
    internal class ListViewContentAppFactory : IContentAppFactory
    {
        // see note on ContentApp
        private const int Weight = -666;

        private readonly IDataTypeService _dataTypeService;
        private readonly PropertyEditorCollection _propertyEditors;

        public ListViewContentAppFactory(IDataTypeService dataTypeService, PropertyEditorCollection propertyEditors)
        {
            _dataTypeService = dataTypeService;
            _propertyEditors = propertyEditors;
        }

        public ContentApp GetContentAppFor(object o, IEnumerable<IReadOnlyUserGroup> userGroups)
        {
            string contentTypeAlias, entityType;
            int dtdId;

            switch (o)
            {
                case IContent content when !content.ContentType.IsContainer:
                    return null;
                case IContent content:
                    contentTypeAlias = content.ContentType.Alias;
                    entityType = "content";
                    dtdId = Core.Constants.DataTypes.DefaultContentListView;
                    break;
                case IMedia media when !media.ContentType.IsContainer && media.ContentType.Alias != Core.Constants.Conventions.MediaTypes.Folder:
                    return null;
                case IMedia media:
                    contentTypeAlias = media.ContentType.Alias;
                    entityType = "media";
                    dtdId = Core.Constants.DataTypes.DefaultMediaListView;
                    break;
                default:
                    return null;
            }

            return CreateContentApp(_dataTypeService, _propertyEditors, entityType, contentTypeAlias, dtdId);
        }

        public static ContentApp CreateContentApp(IDataTypeService dataTypeService,
            PropertyEditorCollection propertyEditors,
            string entityType, string contentTypeAlias,
            int defaultListViewDataType)
        {
            if (dataTypeService == null) throw new ArgumentNullException(nameof(dataTypeService));
            if (propertyEditors == null) throw new ArgumentNullException(nameof(propertyEditors));
            if (string.IsNullOrWhiteSpace(entityType)) throw new ArgumentException("message", nameof(entityType));
            if (string.IsNullOrWhiteSpace(contentTypeAlias)) throw new ArgumentException("message", nameof(contentTypeAlias));
            if (defaultListViewDataType == default) throw new ArgumentException("defaultListViewDataType", nameof(defaultListViewDataType));

            var contentApp = new ContentApp
            {
                Alias = "umbListView",
                Name = "Child items",
                Icon = "icon-list",
                View = "views/content/apps/listview/listview.html",
                Weight = Weight
            };

            var customDtdName = Core.Constants.Conventions.DataTypes.ListViewPrefix + contentTypeAlias;
            
            //first try to get the custom one if there is one
            var dt = dataTypeService.GetDataType(customDtdName)
                     ?? dataTypeService.GetDataType(defaultListViewDataType);

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

            //Override Icon if icon is provided
            if (listViewConfig.ContainsKey("icon"))
            {
                var configIcon = listViewConfig["icon"];
                if (configIcon != null && String.IsNullOrWhiteSpace(configIcon.ToString()) == false)
                    contentApp.Icon = configIcon.ToString();
            }

            // if the list view is configured to show umbContent first, update the list view content app weight accordingly
            if(listViewConfig.ContainsKey("showContentFirst") &&
               listViewConfig["showContentFirst"]?.ToString().TryConvertTo<bool>().Result == true)
            {
                contentApp.Weight = ContentEditorContentAppFactory.Weight + 1;
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
