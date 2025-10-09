using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;
using PropertyEditorAliases = Umbraco.Cms.Core.Constants.PropertyEditors.Aliases;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_14_0_0;

public class MigrateDataTypeConfigurations : MigrationBase
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IMemberTypeService _memberTypeService;
    private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;
    private readonly ILogger<MigrateDataTypeConfigurations> _logger;

    public MigrateDataTypeConfigurations(
        IMigrationContext context,
        IContentTypeService contentTypeService,
        IMediaTypeService mediaTypeService,
        IMemberTypeService memberTypeService,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer,
        ILogger<MigrateDataTypeConfigurations> logger)
        : base(context)
    {
        _contentTypeService = contentTypeService;
        _mediaTypeService = mediaTypeService;
        _memberTypeService = memberTypeService;
        _configurationEditorJsonSerializer = configurationEditorJsonSerializer;
        _logger = logger;
    }

    protected override void Migrate()
    {
        IContentType[] allContentTypes = _contentTypeService.GetAll().ToArray();
        IMediaType[] allMediaTypes = _mediaTypeService.GetAll().ToArray();
        IMemberType[] allMemberTypes = _memberTypeService.GetAll().ToArray();

        Sql<ISqlContext> sql = Sql()
            .Select<DataTypeDto>()
            .AndSelect<NodeDto>()
            .From<DataTypeDto>()
            .InnerJoin<NodeDto>()
            .On<DataTypeDto, NodeDto>(left => left.NodeId, right => right.NodeId)
            .Where<DataTypeDto>(x => x.EditorAlias.Contains("Umbraco."));

        List<DataTypeDto> dataTypeDtos = Database.Fetch<DataTypeDto>(sql);

        foreach (DataTypeDto dataTypeDto in dataTypeDtos)
        {
            var updated = false;
            Dictionary<string, object>? configurationData = null;
            try
            {
                configurationData = dataTypeDto.Configuration.IsNullOrWhiteSpace()
                    ? new Dictionary<string, object>()
                    : _configurationEditorJsonSerializer
                          .Deserialize<Dictionary<string, object?>>(dataTypeDto.Configuration)?
                          .Where(item => item.Value is not null)
                          .ToDictionary(item => item.Key, item => item.Value!)
                      ?? new Dictionary<string, object>();

                // do not attempt to migrate the configuration data twice (it *will* fail for some editors)
                if (configurationData.ContainsKey("umbMigrationV14"))
                {
                    continue;
                }

                // fix config key casing - should always be camelCase, but some have been saved as PascalCase over the years
                var badlyCasedKeys = configurationData.Keys.Where(key => key.ToFirstLowerInvariant() != key).ToArray();
                updated = badlyCasedKeys.Any();
                foreach (var incorrectKey in badlyCasedKeys)
                {
                    configurationData[incorrectKey.ToFirstLowerInvariant()] = configurationData[incorrectKey];
                    configurationData.Remove(incorrectKey);
                }

                // handle special cases, i.e. missing configs (list view), weirdly serialized configs (color picker), min/max for multiple text strings, etc. etc.
                updated |= dataTypeDto.EditorAlias switch
                {
                    PropertyEditorAliases.Boolean => HandleBoolean(ref configurationData),
                    PropertyEditorAliases.CheckBoxList => HandleCheckBoxList(ref configurationData),
                    PropertyEditorAliases.ColorPicker => HandleColorPicker(ref configurationData),
                    PropertyEditorAliases.ContentPicker => HandleContentPicker(ref configurationData),
                    PropertyEditorAliases.DateTime => HandleDateTime(ref configurationData),
                    PropertyEditorAliases.DropDownListFlexible => HandleDropDown(ref configurationData),
                    PropertyEditorAliases.EmailAddress => HandleEmailAddress(ref configurationData),
                    PropertyEditorAliases.Label => HandleLabel(ref configurationData),
                    PropertyEditorAliases.ListView => HandleListView(ref configurationData, dataTypeDto.NodeDto?.UniqueId, allMediaTypes),
                    PropertyEditorAliases.MediaPicker3 => HandleMediaPicker(ref configurationData, allMediaTypes),
                    PropertyEditorAliases.MultiNodeTreePicker => HandleMultiNodeTreePicker(ref configurationData, allContentTypes, allMediaTypes, allMemberTypes),
                    PropertyEditorAliases.MultiUrlPicker => HandleMultiUrlPicker(ref configurationData),
                    PropertyEditorAliases.MultipleTextstring => HandleMultipleTextstring(ref configurationData),
                    PropertyEditorAliases.RadioButtonList => HandleRadioButton(ref configurationData),
                    PropertyEditorAliases.RichText => HandleRichText(ref configurationData),
                    PropertyEditorAliases.TextBox => HandleTextBoxAndTextArea(ref configurationData),
                    PropertyEditorAliases.TextArea => HandleTextBoxAndTextArea(ref configurationData),
                    "Umbraco.TinyMCE" => HandleRichText(ref configurationData),
                    PropertyEditorAliases.UploadField => HandleUploadField(ref configurationData),
                    _ => false
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Migration failed for data type: {dataTypeName} (id: {dataTypeId}, editor alias: {dataTypeEditorAlias})", dataTypeDto.NodeDto?.Text, dataTypeDto.NodeId, dataTypeDto.EditorAlias);
            }

            if (updated && configurationData is not null)
            {
                // tag the configuration data as migrated, so we don't attempt to migrate it twice
                configurationData["umbMigrationV14"] = DateTimeOffset.UtcNow;
                dataTypeDto.Configuration = _configurationEditorJsonSerializer.Serialize(configurationData);
                Database.Update(dataTypeDto);
                RebuildCache = true;
            }
        }
    }

    // translate "default value" to a proper boolean value
    private bool HandleBoolean(ref Dictionary<string, object> configurationData)
        => ReplaceIntegerStringWithBoolean(ref configurationData, "default");

    // translate "selectable items" from old "value list" format to string array
    private bool HandleCheckBoxList(ref Dictionary<string, object> configurationData)
        => ReplaceValueListArrayWithStringArray(ref configurationData, "items");

    // translate "allowed colors" configuration from multiple old formats
    private bool HandleColorPicker(ref Dictionary<string, object> configurationData)
    {
        if (configurationData.Any() is false)
        {
            return false;
        }

        // convert "useLabel" from 0/1 to false/true
        var changed = false;
        var useLabel = ConfigurationValue(configurationData, "useLabel");
        if (useLabel is not null && int.TryParse(useLabel, out var intValue))
        {
            configurationData["useLabel"] = intValue == 1;
            changed = true;
        }

        if (configurationData.All(item => item.Value is string { Length: 3 or 6 }))
        {
            // V7.0 format:
            // {
            //     "0": "FFF",
            //     "1": "F00",
            //     "2": "0F0",
            //     ...
            // }

            configurationData["items"] = configurationData.Select(item =>
            {
                var hex = ((string)item.Value).ToLowerInvariant();
                if (hex.Length is 3)
                {
                    hex = string.Join(string.Empty, hex.Select(c => $"{c}{c}"));
                }

                return new ColorPickerItem
                {
                    Label = hex,
                    Value = hex
                };
            }).ToArray();

            configurationData.RemoveAll(item => item.Key is not "items");
            return true;
        }

        if (configurationData.ContainsKey("items") is false && configurationData.Any(item => int.TryParse(item.Key, out _) && item.Value is JsonObject))
        {
            // V7.15 format:
            // {
            //   "useLabel": "0",
            //   "0": {
            //     "value": "000000",
            //     "label": "Black",
            //     "sortOrder": 0
            //   },
            //   "1": {
            //     "value": "ff0000",
            //     "label": "Red",
            //     "sortOrder": 1
            //   },
            //   ...
            // }

            configurationData["items"] = configurationData
                .Where(item => int.TryParse(item.Key, out _) && item.Value is JsonObject)
                .Select(item =>
                {
                    // we'll wrap this in an explicit try/catch here because we really can't be entirely sure of the validity of
                    // each item in the data - but we want to port over as many as we can
                    try
                    {
                        return _configurationEditorJsonSerializer.Deserialize<ColorPickerItem>(((JsonObject)item.Value)
                            .ToJsonString());
                    }
                    catch
                    {
                        // silently ignore
                        return null;
                    }
                })
                .WhereNotNull()
                .ToArray();

            configurationData.RemoveAll(item => int.TryParse(item.Key, out _));
            return true;
        }

        // V8.0+ format (yes, it became a "value list" where the values are serialized objects of "value" and "label")
        // {
        //   "useLabel": true,
        //   "items": [
        //     {
        //       "id": 1,
        //       "value": "{\"value\":\"000000\",\"label\":\"Black\"}"
        //     },
        //     {
        //       "id": 2,
        //       "value": "{\"value\":\"ff0000\",\"label\":\"Red\"}"
        //     },
        //     ...
        //   ]
        // }
        // ... OR potentially (mentioned in the old codebase, but yet to be seen in an actual test database):
        // {
        //   "useLabel": false,
        //   "items": [
        //     {
        //       "id": 1,
        //       "value": "000000"
        //     },
        //     {
        //       "id": 2,
        //       "value": "ff0000"
        //     },
        //     ...
        //   ]
        // }

        var items = ConfigurationValue(configurationData, "items", true);
        if (items is null)
        {
            return changed;
        }

        ValueListItem[]? valueListItems = _configurationEditorJsonSerializer.Deserialize<ValueListItem[]>(items);
        if (valueListItems is null)
        {
            // this exception is caught by the calling method, which logs the error and continues with the rest of the data type migration
            throw new InvalidOperationException("The color picker \"items\" configuration could not be parsed.");
        }

        ColorPickerItem[] colorPickerValues = valueListItems
            .Select(item => item.Value.DetectIsJson()
                ? _configurationEditorJsonSerializer.Deserialize<ColorPickerItem>(item.Value)
                : new ColorPickerItem { Label = item.Value, Value = item.Value })
            .WhereNotNull()
            .ToArray();

        configurationData["items"] = colorPickerValues;
        return true;
    }

    // translate start node from UDI
    private bool HandleContentPicker(ref Dictionary<string, object> configurationData)
        => ReplaceUdiWithKey(ref configurationData, "startNodeId");

    // translate "offsetTime" and "defaultEmpty" to a property boolean value
    private bool HandleDateTime(ref Dictionary<string, object> configurationData)
        => ReplaceIntegerStringWithBoolean(ref configurationData, "offsetTime")
           | ReplaceIntegerStringWithBoolean(ref configurationData, "defaultEmpty");

    // translate "selectable items" from old "value list" format to string array
    private bool HandleDropDown(ref Dictionary<string, object> configurationData)
        => ReplaceValueListArrayWithStringArray(ref configurationData, "items");

    // remove old (obsolete) "isRequired" configuration
    private bool HandleEmailAddress(ref Dictionary<string, object> configurationData)
        => configurationData.Remove("isRequired");

    // enforce default "umbracoDataValueType" for label (may be empty for old data types)
    private static bool HandleLabel(ref Dictionary<string, object> configurationData)
    {
        if (configurationData.ContainsKey(Constants.PropertyEditors.ConfigurationKeys.DataValueType))
        {
            if (configurationData[Constants.PropertyEditors.ConfigurationKeys.DataValueType] is string value && value.IsNullOrWhiteSpace() == false)
            {
                return false;
            }
        }

        configurationData[Constants.PropertyEditors.ConfigurationKeys.DataValueType] = ValueTypes.String;
        return true;
    }

    // ensure that list view configs have all configurations, as some have never been added by means of migration.
    // also performs a re-formatting of "layouts" and "includeProperties" to a V14 format
    private bool HandleListView(ref Dictionary<string, object> configurationData, Guid? dataTypeKey, IMediaType[] allMediaTypes)
    {
        var collectionViewType = dataTypeKey == Constants.DataTypes.Guids.ListViewMediaGuid || allMediaTypes.Any(mt => mt.ListView == dataTypeKey)
            ? "Media"
            : "Document";

        string? LayoutPathToCollectionView(string? path)
            => "views/propertyeditors/listview/layouts/list/list.html".InvariantEquals(path)
                ? TableCollectionView()
                : "views/propertyeditors/listview/layouts/grid/grid.html".InvariantEquals(path)
                    ? GridCollectionView()
                    : null;

        string TableCollectionView() => $"Umb.CollectionView.{collectionViewType}.Table";

        string GridCollectionView() => $"Umb.CollectionView.{collectionViewType}.Grid";

        var layoutsValue = ConfigurationValue(configurationData, "layouts", true);
        if (layoutsValue is not null)
        {
            OldListViewLayout[]? layouts = _configurationEditorJsonSerializer.Deserialize<OldListViewLayout[]>(layoutsValue);
            if (layouts is null)
            {
                // this exception is caught by the calling method, which logs the error and continues with the rest of the data type migration
                throw new InvalidOperationException("The list view \"layouts\" configuration could not be parsed.");
            }

            configurationData["layouts"] = layouts.Select(layout => new NewListViewLayout
            {
                Name = layout.Name,
                IsSystem = layout.IsSystem == 1,
                Selected = layout.Selected,
                Icon = layout.Icon,
                CollectionView = LayoutPathToCollectionView(layout.Path)
            }).ToArray();
        }
        else
        {
            configurationData["layouts"] = new[]
            {
                new NewListViewLayout
                {
                    Name = "List",
                    CollectionView = TableCollectionView(),
                    Icon = "icon-list",
                    IsSystem = true,
                    Selected = true
                },
                new NewListViewLayout
                {
                    Name = "Grid",
                    CollectionView = GridCollectionView(),
                    Icon = "icon-thumbnails-small",
                    IsSystem = true,
                    Selected = true
                }
            };
        }

        var includePropertiesValue = ConfigurationValue(configurationData, "includeProperties", true);
        if (includePropertiesValue is not null)
        {
            OldListViewProperty[]? properties = _configurationEditorJsonSerializer.Deserialize<OldListViewProperty[]>(includePropertiesValue);
            if (properties is null)
            {
                // this exception is caught by the calling method, which logs the error and continues with the rest of the data type migration
                throw new InvalidOperationException("The list view \"includePropertiesValue\" configuration could not be parsed.");
            }

            configurationData["includeProperties"] = properties.Select(property => new NewListViewProperty
            {
                // the "owner" property alias is "creator" from V14
                Alias = property.Alias is "owner" ? "creator" : property.Alias,
                Header = property.Header ?? property.Alias switch
                {
                    "email" => "Email",
                    "username" => "User name",
                    "createDate" => "Created at",
                    "published" => "Published at",
                    "updater" => "Edited by",
                    _ => string.Empty
                },
                NameTemplate = property.NameTemplate,
                IsSystem = property.IsSystem == 1
            }).ToArray();
        }
        else
        {
            configurationData["includeProperties"] = new[]
            {
                new NewListViewProperty { Alias = "sortOrder", Header = "Sort order", IsSystem = true },
                new NewListViewProperty { Alias = "updateDate", Header = "Last edited", IsSystem = true },
                new NewListViewProperty { Alias = "creator", Header = "Created by", IsSystem = true }
            };
        }

        configurationData.TryAdd("bulkActionPermissions", new ListViewDefaults.BulkActionPermissions());
        configurationData.TryAdd("icon", ListViewDefaults.Icon);
        configurationData.TryAdd("showContentFirst", ListViewDefaults.ShowContentFirst);
        configurationData.TryAdd("useInfiniteEditor", ListViewDefaults.UseInfiniteEditor);
        configurationData.TryAdd("pageSize", ListViewDefaults.PageSize);
        configurationData.TryAdd("orderDirection", ListViewDefaults.OrderDirection);
        configurationData.TryAdd("orderBy", ListViewDefaults.OrderBy);

        // with the reformatting of "layouts" and "includeProperties", the list view configs will always have changed
        return true;
    }

    // translate start node from UDI to key and replace docType aliases with their keys
    private bool HandleMediaPicker(ref Dictionary<string, object> configurationData, IMediaType[] allMediaTypes)
        => ReplaceUdiWithKey(ref configurationData, "startNodeId")
           | ReplaceContentTypeAliasesWithKeys(ref configurationData, "filter", allMediaTypes);

    // translate start node from UDI and replace docType aliases with their keys
    private bool HandleMultiNodeTreePicker(ref Dictionary<string, object> configurationData, IContentType[] allContentTypes, IMediaType[] allMediaTypes, IMemberType[] allMemberTypes)
    {
        var changed = false;

        var startNodeValue = ConfigurationValue(configurationData, "startNode", true);
        OldTreeSource? treeSource = startNodeValue is not null
            ? _configurationEditorJsonSerializer.Deserialize<OldTreeSource>(startNodeValue)
            : null;
        if (treeSource is not null)
        {
            configurationData["startNode"] = new NewTreeSource
            {
                Type = treeSource.Type,
                Id = treeSource.Id?.Guid,
                DynamicRoot = treeSource.DynamicRoot
            };
            changed = true;
        }

        changed |= ReplaceContentTypeAliasesWithKeys(
            ref configurationData,
            "filter",
            treeSource?.Type.ToLowerInvariant() switch
            {
                "media" => allMediaTypes,
                "member" => allMemberTypes,
                _ => allContentTypes
            });

        // old, server-side calculated property that should never be part of config
        changed |= configurationData.Remove("multiPicker");

        changed |= ReplaceIntegerStringWithBoolean(ref configurationData, "ignoreUserStartNodes")
                   | ReplaceIntegerStringWithBoolean(ref configurationData, "showOpenButton");

        return changed;
    }

    // replace "ignoreUserStartNodes" with a proper boolean value
    private bool HandleMultiUrlPicker(ref Dictionary<string, object> configurationData)
        => ReplaceIntegerStringWithBoolean(ref configurationData, "ignoreUserStartNodes");

    // convert the stored keys "minimum" and "maximum" to the expected keys "min" and "max for multiple textstrings
    private static bool HandleMultipleTextstring(ref Dictionary<string, object> configurationData)
    {
        Dictionary<string, object> data = configurationData;

        bool ReplaceKey(string oldKey, string newKey)
        {
            if (data.ContainsKey(oldKey))
            {
                data[newKey] = data[oldKey];
                data.Remove(oldKey);
                return true;
            }

            return false;
        }

        return ReplaceKey("minimum", "min") | ReplaceKey("maximum", "max");
    }

    // translate "selectable items" from old "value list" format to string array
    private bool HandleRadioButton(ref Dictionary<string, object> configurationData)
        => ReplaceValueListArrayWithStringArray(ref configurationData, "items");


    // translate media parent UDI and split "editor" value into separate configuration data values
    private bool HandleRichText(ref Dictionary<string, object> configurationData)
    {
        var changed = ReplaceUdiWithKey(ref configurationData, "mediaParentId");

        var editor = ConfigurationValue(configurationData, "editor", true);
        if (editor is null)
        {
            return changed;
        }

        RichTextEditorConfiguration? richTextEditorConfiguration = _configurationEditorJsonSerializer.Deserialize<RichTextEditorConfiguration>(editor);
        if (richTextEditorConfiguration is null)
        {
            // this exception is caught by the calling method, which logs the error and continues with the rest of the data type migration
            throw new InvalidOperationException("The rich text \"editor\" configuration could not be parsed.");
        }

        if (richTextEditorConfiguration.Toolbar is not null && richTextEditorConfiguration.Toolbar.Any())
        {
            configurationData["toolbar"] = richTextEditorConfiguration.Toolbar;
        }

        if (richTextEditorConfiguration.Stylesheets is not null && richTextEditorConfiguration.Stylesheets.Any())
        {
            configurationData["stylesheets"] = richTextEditorConfiguration.Stylesheets;
        }

        if (richTextEditorConfiguration.Mode.IsNullOrWhiteSpace() is false)
        {
            configurationData["mode"] = richTextEditorConfiguration.Mode.ToFirstUpperInvariant();
        }

        if (richTextEditorConfiguration.MaxImageSize is not null)
        {
            configurationData["maxImageSize"] = richTextEditorConfiguration.MaxImageSize;
        }

        if (richTextEditorConfiguration.Dimensions is not null)
        {
            configurationData["dimensions"] = richTextEditorConfiguration.Dimensions;
        }

        configurationData.Remove("editor");

        ReplaceIntegerStringWithBoolean(ref configurationData, "ignoreUserStartNodes");

        return true;
    }

    // enforce integer values for text area and text box (may be saved as string values from old times)
    private static bool HandleTextBoxAndTextArea(ref Dictionary<string, object> configurationData)
    {
        Dictionary<string, object> data = configurationData;
        bool ReplaceStringWithIntValue(string key)
        {
            if (data.ContainsKey(key) && data[key] is string stringValue && int.TryParse(stringValue, out var intValue))
            {
                data[key] = intValue;
                return true;
            }

            return false;
        }

        return ReplaceStringWithIntValue("maxChars") | ReplaceStringWithIntValue("rows");
    }

    // translate "allowed file extensions" from old "value list" format to string array
    private bool HandleUploadField(ref Dictionary<string, object> configurationData)
        => ReplaceValueListArrayWithStringArray(ref configurationData, "fileExtensions");

    private string? ConfigurationValue(IReadOnlyDictionary<string, object> configurationData, string key, bool mustBeJson = false)
    {
        if (configurationData.TryGetValue(key, out var configurationValue) is false)
        {
            return null;
        }

        var value = configurationValue.ToString();
        if (value.IsNullOrWhiteSpace() || (mustBeJson && value.DetectIsJson() is false))
        {
            return null;
        }

        return value;
    }

    private bool ReplaceUdiWithKey(ref Dictionary<string, object> configurationData, string key)
    {
        var configurationValue = ConfigurationValue(configurationData, key);
        if (configurationValue is null || UdiParser.TryParse(configurationValue, out GuidUdi? udi) is false)
        {
            return false;
        }

        configurationData[key] = udi.Guid;
        return true;
    }

    private bool ReplaceContentTypeAliasesWithKeys(ref Dictionary<string, object> configurationData, string key, IEnumerable<IContentTypeBase> allContentTypes)
    {
        var value = ConfigurationValue(configurationData, key);
        if (value is null)
        {
            return false;
        }

        var aliases = value.Split(Constants.CharArrays.Comma).ToArray();
        Guid[] keys = aliases
            .Select(alias => allContentTypes.FirstOrDefault(c => c.Alias.InvariantEquals(alias))?.Key)
            .Where(contentTypeKey => contentTypeKey.HasValue)
            .Select(contentTypeKey => contentTypeKey!.Value)
            .ToArray();

        configurationData[key] = string.Join(",", keys);
        return true;
    }

    private bool ReplaceValueListArrayWithStringArray(ref Dictionary<string, object> configurationData, string key)
    {
        var items = ConfigurationValue(configurationData, key, true);
        if (items is null)
        {
            return false;
        }

        ValueListItem[]? valueListItems = _configurationEditorJsonSerializer.Deserialize<ValueListItem[]>(items);
        if (valueListItems is null)
        {
            // this exception is caught by the calling method, which logs the error and continues with the rest of the data type migration
            throw new InvalidOperationException($"The configuration key \"{key}\" configuration could not be parsed as value list items.");
        }

        configurationData[key] = valueListItems.Select(item => item.Value).ToArray();
        return true;
    }

    private bool ReplaceIntegerStringWithBoolean(ref Dictionary<string, object> configurationData, string key)
    {
        var value = ConfigurationValue(configurationData, key);
        if (value.IsNullOrWhiteSpace())
        {
            return false;
        }

        switch (value)
        {
            case "0":
                configurationData[key] = false;
                return true;
            case "1":
                configurationData[key] = true;
                return true;
            default:
                return false;
        }
    }

    private class RichTextEditorConfiguration
    {
        public string[]? Toolbar { get; set; }

        public string[]? Stylesheets { get; set; }

        public int? MaxImageSize { get; set; }

        public string? Mode { get; set; }

        public EditorDimensions? Dimensions { get; set; }

        public class EditorDimensions
        {
            public int? Width { get; set; }

            public int? Height { get; set; }
        }
    }

    private class ValueListItem
    {
        public int Id { get; set; }

        public required string Value { get; set; }
    }

    private class ColorPickerItem
    {
        public required string Value { get; set; }

        public required string Label { get; set; }
    }

    private class OldTreeSource
    {
        public required string Type { get; set; }

        public GuidUdi? Id { get; set; }

        public object? DynamicRoot { get; set; }
    }

    private class NewTreeSource
    {
        public required string Type { get; set; }

        public Guid? Id { get; set; }

        public object? DynamicRoot { get; set; }
    }

    private class ListViewDefaults
    {
        public class BulkActionPermissions
        {
            public bool AllowBulkPublish { get; } = true;

            public bool AllowBulkUnpublish { get; } = true;

            public bool AllowBulkCopy { get; } = true;

            public bool AllowBulkMove { get; } = true;

            public bool AllowBulkDelete { get; } = true;
        }

        public const string Icon = "icon-badge color-black";

        public const bool ShowContentFirst = false;

        public const bool UseInfiniteEditor = false;

        public const string OrderBy = "updateDate";

        public const string OrderDirection = "desc";

        public const int PageSize = 10;
    }

    private class OldListViewLayout
    {
        public string? Name { get; set; }

        public string? Path { get; set; }

        public string? Icon { get; set; }

        public int IsSystem { get; set; }

        public bool Selected { get; set; }
    }

    private class NewListViewLayout
    {
        public string? Name { get; set; }

        public string? CollectionView { get; set; }

        public string? Icon { get; set; }

        public bool IsSystem { get; set; }

        public bool Selected { get; set; }
    }

    private class OldListViewProperty
    {
        public required string Alias { get; set; }

        public string? Header { get; set; }

        public string? NameTemplate { get; set; }

        public required int IsSystem { get; set; }
    }

    private class NewListViewProperty
    {
        public required string Alias { get; set; }

        public required string Header { get; set; }

        public string? NameTemplate { get; set; }

        public required bool IsSystem { get; set; }
    }
}
