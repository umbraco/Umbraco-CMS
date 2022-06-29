using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.ContentApps;

public class ListViewContentAppFactory : IContentAppFactory
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

    public static ContentApp CreateContentApp(
        IDataTypeService dataTypeService,
        PropertyEditorCollection propertyEditors,
        string entityType,
        string contentTypeAlias,
        int defaultListViewDataType)
    {
        if (dataTypeService == null)
        {
            throw new ArgumentNullException(nameof(dataTypeService));
        }

        if (propertyEditors == null)
        {
            throw new ArgumentNullException(nameof(propertyEditors));
        }

        if (string.IsNullOrWhiteSpace(entityType))
        {
            throw new ArgumentException("message", nameof(entityType));
        }

        if (string.IsNullOrWhiteSpace(contentTypeAlias))
        {
            throw new ArgumentException("message", nameof(contentTypeAlias));
        }

        if (defaultListViewDataType == default)
        {
            throw new ArgumentException("defaultListViewDataType", nameof(defaultListViewDataType));
        }

        var contentApp = new ContentApp
        {
            Alias = "umbListView",
            Name = "Child items",
            Icon = "icon-list",
            View = "views/content/apps/listview/listview.html",
            Weight = Weight,
        };

        var customDtdName = Constants.Conventions.DataTypes.ListViewPrefix + contentTypeAlias;

        // first try to get the custom one if there is one
        IDataType? dt = dataTypeService.GetDataType(customDtdName)
                        ?? dataTypeService.GetDataType(defaultListViewDataType);

        if (dt == null)
        {
            throw new InvalidOperationException(
                "No list view data type was found for this document type, ensure that the default list view data types exists and/or that your custom list view data type exists");
        }

        IDataEditor? editor = propertyEditors[dt.EditorAlias];
        if (editor == null)
        {
            throw new NullReferenceException("The property editor with alias " + dt.EditorAlias + " does not exist");
        }

        IDictionary<string, object?> listViewConfig = editor.GetConfigurationEditor().ToConfigurationEditorNullable(dt.Configuration);

        // add the entity type to the config
        listViewConfig["entityType"] = entityType;

        // Override Tab Label if tabName is provided
        if (listViewConfig.ContainsKey("tabName"))
        {
            var configTabName = listViewConfig["tabName"];
            if (string.IsNullOrWhiteSpace(configTabName?.ToString()) == false)
            {
                contentApp.Name = configTabName.ToString();
            }
        }

        // Override Icon if icon is provided
        if (listViewConfig.ContainsKey("icon"))
        {
            var configIcon = listViewConfig["icon"];
            if (string.IsNullOrWhiteSpace(configIcon?.ToString()) == false)
            {
                contentApp.Icon = configIcon.ToString();
            }
        }

        // if the list view is configured to show umbContent first, update the list view content app weight accordingly
        if (listViewConfig.ContainsKey("showContentFirst") &&
            listViewConfig["showContentFirst"]?.ToString().TryConvertTo<bool>().Result == true)
        {
            contentApp.Weight = ContentEditorContentAppFactory.Weight + 1;
        }

        // This is the view model used for the list view app
        contentApp.ViewModel = new List<ContentPropertyDisplay>
        {
            new()
            {
                Alias = $"{Constants.PropertyEditors.InternalGenericPropertiesPrefix}containerView",
                Label = string.Empty,
                Value = null,
                View = editor.GetValueEditor().View,
                HideLabel = true,
                ConfigNullable = listViewConfig,
            },
        };

        return contentApp;
    }

    public ContentApp? GetContentAppFor(object o, IEnumerable<IReadOnlyUserGroup> userGroups)
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
                dtdId = Constants.DataTypes.DefaultContentListView;
                break;
            case IMedia media when !media.ContentType.IsContainer &&
                                   media.ContentType.Alias != Constants.Conventions.MediaTypes.Folder:
                return null;
            case IMedia media:
                contentTypeAlias = media.ContentType.Alias;
                entityType = "media";
                dtdId = Constants.DataTypes.DefaultMediaListView;
                break;
            default:
                return null;
        }

        return CreateContentApp(_dataTypeService, _propertyEditors, entityType, contentTypeAlias, dtdId);
    }
}
