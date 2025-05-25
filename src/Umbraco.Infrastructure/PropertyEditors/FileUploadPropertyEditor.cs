// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

[DataEditor(
    Constants.PropertyEditors.Aliases.UploadField,
    ValueEditorIsReusable = true)]
public class FileUploadPropertyEditor : DataEditor, IMediaUrlGenerator,
    INotificationHandler<ContentCopiedNotification>, INotificationHandler<ContentDeletedNotification>,
    INotificationHandler<MediaDeletedNotification>, INotificationHandler<MediaSavingNotification>,
    INotificationHandler<MemberDeletedNotification>
{
    private readonly IContentService _contentService;
    private readonly IOptionsMonitor<ContentSettings> _contentSettings;
    private readonly IIOHelper _ioHelper;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly MediaFileManager _mediaFileManager;
    private readonly UploadAutoFillProperties _uploadAutoFillProperties;
    private readonly BlockEditorValues<BlockListValue, BlockListLayoutItem> _blockListEditorValues;
    private readonly BlockEditorValues<BlockGridValue, BlockGridLayoutItem> _blockGridEditorValues;
    private readonly FileUploadValueParser _fileUploadValueParser;

    public FileUploadPropertyEditor(
        IBlockEditorElementTypeCache blockEditorElementTypeCache,
        IJsonSerializer jsonSerializer,
        ILogger<FileUploadPropertyEditor> logger,
        IDataValueEditorFactory dataValueEditorFactory,
        MediaFileManager mediaFileManager,
        IOptionsMonitor<ContentSettings> contentSettings,
        UploadAutoFillProperties uploadAutoFillProperties,
        IContentService contentService,
        IIOHelper ioHelper)
        : base(dataValueEditorFactory)
    {
        _jsonSerializer = jsonSerializer;
        _mediaFileManager = mediaFileManager ?? throw new ArgumentNullException(nameof(mediaFileManager));
        _contentSettings = contentSettings;
        _uploadAutoFillProperties = uploadAutoFillProperties;
        _contentService = contentService;
        _ioHelper = ioHelper;
        SupportsReadOnly = true;
        _blockListEditorValues = new BlockEditorValues<BlockListValue, BlockListLayoutItem>(new BlockListEditorDataConverter(jsonSerializer), blockEditorElementTypeCache, logger);
        _blockGridEditorValues = new BlockEditorValues<BlockGridValue, BlockGridLayoutItem>(new BlockGridEditorDataConverter(jsonSerializer), blockEditorElementTypeCache, logger);
        _fileUploadValueParser = new FileUploadValueParser(jsonSerializer);
    }

    public bool TryGetMediaPath(string? propertyEditorAlias, object? value, [MaybeNullWhen(false)] out string mediaPath)
    {
        if (propertyEditorAlias == Alias &&
            value?.ToString() is var mediaPathValue &&
            !string.IsNullOrWhiteSpace(mediaPathValue))
        {
            mediaPath = mediaPathValue;
            return true;
        }

        mediaPath = null;
        return false;
    }

    #region Handle Copied Notifications

    public void Handle(ContentCopiedNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var isUpdated = false;

        foreach (IProperty property in notification.Original.Properties)
        {
            if (IsUploadFieldPropertyType(property.PropertyType))
            {
                isUpdated |= UpdateUploadFieldProperty(notification, property);

                continue;
            }

            if (IsBlockListPropertyType(property.PropertyType))
            {
                isUpdated |= UpdateBlockProperty(notification, property, _blockListEditorValues);

                continue;
            }

            if (IsBlockGridPropertyType(property.PropertyType))
            {
                isUpdated |= UpdateBlockProperty(notification, property, _blockGridEditorValues);

                continue;
            }
        }

        // if updated, re-save the copy with the updated value
        if (isUpdated)
        {
            _contentService.Save(notification.Copy);
        }
    }

    private bool UpdateUploadFieldProperty(ContentCopiedNotification notification, IProperty property)
    {
        var isUpdated = false;

        // copy each of the property values (variants, segments) to the destination
        foreach (IPropertyValue propertyValue in property.Values)
        {
            var rawFileUrl = property.GetValue(propertyValue.Culture, propertyValue.Segment);
            if (rawFileUrl == null || rawFileUrl is not string originalFileUrl || string.IsNullOrWhiteSpace(originalFileUrl))
            {
                continue;
            }

            var copyFileUrl = CopyFile(originalFileUrl, notification, property.PropertyType);

            notification.Copy.SetValue(property.Alias, copyFileUrl, propertyValue.Culture, propertyValue.Segment);

            isUpdated = true;
        }

        return isUpdated;
    }

    private bool UpdateBlockProperty<TValue, TLayout>(ContentCopiedNotification notification, IProperty property, BlockEditorValues<TValue, TLayout> blockEditorValues)
        where TValue : BlockValue<TLayout>, new()
        where TLayout : class, IBlockLayoutItem, new()
    {
        var isUpdated = false;

        foreach (IPropertyValue blockPropertyValue in property.Values)
        {
            var rawBlockPropertyValue = property.GetValue(blockPropertyValue.Culture, blockPropertyValue.Segment);

            BlockEditorData<TValue, TLayout>? blockEditorData = GetBlockEditorData(rawBlockPropertyValue, blockEditorValues);

            (bool hasUpdates, string? updatedValue) = UpdateBlockEditorData(notification, blockEditorData);

            if (hasUpdates)
            {
                notification.Copy.SetValue(property.Alias, updatedValue, blockPropertyValue.Culture, blockPropertyValue.Segment);
            }

            isUpdated |= hasUpdates;
        }

        return isUpdated;
    }

    private (bool, string?) UpdateBlockEditorData<TValue, TLayout>(ContentCopiedNotification notification, BlockEditorData<TValue, TLayout>? blockEditorData)
        where TValue : BlockValue<TLayout>, new()
        where TLayout : class, IBlockLayoutItem, new()
    {
        var isUpdated = false;

        if (blockEditorData == null)
        {
            return (isUpdated, null);
        }

        IEnumerable<BlockPropertyValue> blockPropertyValues = blockEditorData.BlockValue.ContentData
            .Concat(blockEditorData.BlockValue.SettingsData)
            .SelectMany(x => x.Values);

        foreach (BlockPropertyValue blockPropertyValue in blockPropertyValues)
        {
            if (blockPropertyValue.Value == null)
            {
                continue;
            }

            IPropertyType? propertyType = blockPropertyValue.PropertyType;

            if (propertyType == null)
            {
                continue;
            }

            if (IsUploadFieldPropertyType(propertyType))
            {
                isUpdated |= UpdateUploadFieldBlockPropertyValue(blockPropertyValue, notification, propertyType);

                continue;
            }

            if (IsBlockListPropertyType(propertyType))
            {
                (bool hasUpdates, string? newValue) = UpdateBlockPropertyValue(blockPropertyValue, notification, _blockListEditorValues);

                isUpdated |= hasUpdates;

                blockPropertyValue.Value = newValue;

                continue;
            }

            if (IsBlockGridPropertyType(propertyType))
            {
                (bool hasUpdates, string? newValue) = UpdateBlockPropertyValue(blockPropertyValue, notification, _blockGridEditorValues);

                isUpdated |= hasUpdates;

                blockPropertyValue.Value = newValue;

                continue;
            }
        }

        var updatedValue = _jsonSerializer.Serialize(blockEditorData.BlockValue);

        return (isUpdated, updatedValue);
    }

    private bool UpdateUploadFieldBlockPropertyValue(BlockPropertyValue blockItemDataValue, ContentCopiedNotification notification, IPropertyType propertyType)
    {
        FileUploadValue? originalValue = _fileUploadValueParser.Parse(blockItemDataValue.Value);

        // if original value is empty, we do not need to copy file
        if (string.IsNullOrWhiteSpace(originalValue?.Src))
        {
            return false;
        }

        var copyFileUrl = CopyFile(originalValue.Src, notification, propertyType);

        blockItemDataValue.Value = copyFileUrl;

        return true;
    }

    private (bool, string?) UpdateBlockPropertyValue<TValue, TLayout>(BlockPropertyValue blockItemDataValue, ContentCopiedNotification notification, BlockEditorValues<TValue, TLayout> blockEditorValues)
        where TValue : BlockValue<TLayout>, new()
        where TLayout : class, IBlockLayoutItem, new()
    {
        BlockEditorData<TValue, TLayout>? blockItemEditorDataValue = GetBlockEditorData(blockItemDataValue.Value, blockEditorValues);

        return UpdateBlockEditorData(notification, blockItemEditorDataValue);
    }

    private string CopyFile(string originalFileUrl, ContentCopiedNotification notification, IPropertyType propertyType)
    {
        var originalFilePath = _mediaFileManager.FileSystem.GetRelativePath(originalFileUrl);
        var originalFileName = Path.GetFileName(originalFilePath);
        // TODO: maybe we should use temporary file system to avoid dangling files if on content save something goes wrong
        var copyFilePath = _mediaFileManager.CopyFile(notification.Copy, propertyType, originalFilePath);
        var copyFileUrl = _mediaFileManager.FileSystem.GetUrl(copyFilePath);

        return copyFileUrl;
    }

    #endregion

    #region Handle Saving Notifications

    public void Handle(MediaSavingNotification notification)
    {
        foreach (IMedia entity in notification.SavedEntities)
        {
            AutoFillProperties(entity);
        }
    }

    /// <summary>
    ///     Auto-fill properties (or clear).
    /// </summary>
    private void AutoFillProperties(IContentBase model)
    {
        IEnumerable<IProperty> properties = model.Properties.Where(x => IsUploadFieldPropertyType(x.PropertyType));

        foreach (IProperty property in properties)
        {
            ImagingAutoFillUploadField? autoFillConfig = _contentSettings.CurrentValue.GetConfig(property.Alias);
            if (autoFillConfig == null)
            {
                continue;
            }

            foreach (IPropertyValue pvalue in property.Values)
            {
                var svalue = property.GetValue(pvalue.Culture, pvalue.Segment) as string;
                if (string.IsNullOrWhiteSpace(svalue))
                {
                    _uploadAutoFillProperties.Reset(model, autoFillConfig, pvalue.Culture, pvalue.Segment);
                }
                else
                {
                    _uploadAutoFillProperties.Populate(model, autoFillConfig,
                        _mediaFileManager.FileSystem.GetRelativePath(svalue), pvalue.Culture, pvalue.Segment);
                }
            }
        }
    }

    #endregion

    #region Handle Deleted Notifications

    public void Handle(ContentDeletedNotification notification) => DeleteContainedFiles(notification.DeletedEntities);
    public void Handle(MediaDeletedNotification notification) => DeleteContainedFiles(notification.DeletedEntities);
    public void Handle(MemberDeletedNotification notification) => DeleteContainedFiles(notification.DeletedEntities);

    private void DeleteContainedFiles(IEnumerable<IContentBase> deletedEntities)
    {
        IReadOnlyList<string> filePathsToDelete = ContainedFilePaths(deletedEntities);
        _mediaFileManager.DeleteMediaFiles(filePathsToDelete);
    }

    /// <summary>
    ///     The paths to all file upload property files contained within a collection of content entities
    /// </summary>
    /// <param name="entities"></param>
    private IReadOnlyList<string> ContainedFilePaths(IEnumerable<IContentBase> entities)
    {
        var paths = new List<string>();

        foreach (IProperty? property in entities.SelectMany(x => x.Properties))
        {
            if (IsUploadFieldPropertyType(property.PropertyType))
            {
                paths.AddRange(GetPathsFromUploadFieldProperty(property));

                continue;
            }

            if (IsBlockListPropertyType(property.PropertyType))
            {
                paths.AddRange(GetPathsFromBlockProperty(property, _blockListEditorValues));

                continue;
            }

            if (IsBlockGridPropertyType(property.PropertyType))
            {
                paths.AddRange(GetPathsFromBlockProperty(property, _blockGridEditorValues));

                continue;
            }
        }

        return paths;
    }

    private IEnumerable<string> GetPathsFromUploadFieldProperty(IProperty property)
    {
        foreach (IPropertyValue propertyValue in property.Values)
        {
            if (propertyValue.PublishedValue != null && propertyValue.PublishedValue is string publishedUrl && !string.IsNullOrWhiteSpace(publishedUrl))
            {
                yield return _mediaFileManager.FileSystem.GetRelativePath(publishedUrl);
            }

            if (propertyValue.EditedValue != null && propertyValue.EditedValue is string editedUrl && !string.IsNullOrWhiteSpace(editedUrl))
            {
                yield return _mediaFileManager.FileSystem.GetRelativePath(editedUrl);
            }
        }
    }

    private IReadOnlyCollection<string> GetPathsFromBlockProperty<TValue, TLayout>(IProperty property, BlockEditorValues<TValue, TLayout> blockEditorValues)
        where TValue : BlockValue<TLayout>, new()
        where TLayout : class, IBlockLayoutItem, new()
    {
        var paths = new List<string>();

        foreach (IPropertyValue blockPropertyValue in property.Values)
        {
            paths.AddRange(GetPathsFromBlockEditorData(GetBlockEditorData(blockPropertyValue.PublishedValue, blockEditorValues)));
            paths.AddRange(GetPathsFromBlockEditorData(GetBlockEditorData(blockPropertyValue.EditedValue, blockEditorValues)));
        }

        return paths;
    }

    private IReadOnlyCollection<string> GetPathsFromBlockEditorData<TValue, TLayout>(BlockEditorData<TValue, TLayout>? blockEditorData)
        where TValue : BlockValue<TLayout>, new()
        where TLayout : class, IBlockLayoutItem, new()
    {
        var paths = new List<string>();

        if (blockEditorData == null)
        {
            return paths;
        }

        IEnumerable<BlockPropertyValue> blockPropertyValues = blockEditorData.BlockValue.ContentData
            .Concat(blockEditorData.BlockValue.SettingsData)
            .SelectMany(x => x.Values);

        foreach (BlockPropertyValue blockPropertyValue in blockPropertyValues)
        {
            if (blockPropertyValue.Value == null)
            {
                continue;
            }

            IPropertyType? propertyType = blockPropertyValue.PropertyType;

            if (propertyType == null)
            {
                continue;
            }

            if (IsUploadFieldPropertyType(propertyType))
            {
                FileUploadValue? originalValue = _fileUploadValueParser.Parse(blockPropertyValue.Value);

                if (string.IsNullOrWhiteSpace(originalValue?.Src))
                {
                    continue;
                }

                paths.Add(_mediaFileManager.FileSystem.GetRelativePath(originalValue.Src));

                continue;
            }

            if (IsBlockListPropertyType(propertyType))
            {
                paths.AddRange(GetPathsFromBlockPropertyValue(blockPropertyValue, _blockListEditorValues));

                continue;
            }

            if (IsBlockGridPropertyType(propertyType))
            {
                paths.AddRange(GetPathsFromBlockPropertyValue(blockPropertyValue, _blockGridEditorValues));

                continue;
            }
        }

        return paths;
    }

    private IReadOnlyCollection<string> GetPathsFromBlockPropertyValue<TValue, TLayout>(BlockPropertyValue blockItemDataValue, BlockEditorValues<TValue, TLayout> blockEditorValues)
        where TValue : BlockValue<TLayout>, new()
        where TLayout : class, IBlockLayoutItem, new()
    {
        BlockEditorData<TValue, TLayout>? blockItemEditorDataValue = GetBlockEditorData(blockItemDataValue.Value, blockEditorValues);

        return GetPathsFromBlockEditorData(blockItemEditorDataValue);
    }

    #endregion

    private static BlockEditorData<TValue, TLayout>? GetBlockEditorData<TValue, TLayout>(object? value, BlockEditorValues<TValue, TLayout> blockListEditorValues)
        where TValue : BlockValue<TLayout>, new()
        where TLayout : class, IBlockLayoutItem, new()
    {
        try
        {
            return blockListEditorValues.DeserializeAndClean(value);
        }
        catch
        {
            // if this occurs it means the data is invalid, shouldn't happen but has happened if we change the data format.
            return null;
        }
    }

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new FileUploadConfigurationEditor(_ioHelper);

    /// <summary>
    ///     Creates the corresponding property value editor.
    /// </summary>
    /// <returns>The corresponding property value editor.</returns>
    protected override IDataValueEditor CreateValueEditor()
        => DataValueEditorFactory.Create<FileUploadPropertyValueEditor>(Attribute!);

    /// <summary>
    ///     Gets a value indicating whether a property is an upload field.
    /// </summary>
    /// <param name="propertyType">The property type.</param>
    /// <returns>
    ///     <c>true</c> if the specified property is an upload field; otherwise, <c>false</c>.
    /// </returns>
    private static bool IsUploadFieldPropertyType(IPropertyType propertyType)
    {
        return propertyType.PropertyEditorAlias == Constants.PropertyEditors.Aliases.UploadField;
    }

    /// <summary>
    ///     Gets a value indicating whether a property is an block list field.
    /// </summary>
    /// <param name="propertyType">The property type.</param>
    /// <returns>
    ///     <c>true</c> if the specified property is an block list field; otherwise, <c>false</c>.
    /// </returns>
    private static bool IsBlockListPropertyType(IPropertyType propertyType)
    {
        return propertyType.PropertyEditorAlias == Constants.PropertyEditors.Aliases.BlockList;
    }

    /// <summary>
    ///     Gets a value indicating whether a property is an block grid field.
    /// </summary>
    /// <param name="propertyType">The property type.</param>
    /// <returns>
    ///     <c>true</c> if the specified property is an block grid field; otherwise, <c>false</c>.
    /// </returns>
    private static bool IsBlockGridPropertyType(IPropertyType propertyType)
    {
        return propertyType.PropertyEditorAlias == Constants.PropertyEditors.Aliases.BlockGrid;
    }
}
