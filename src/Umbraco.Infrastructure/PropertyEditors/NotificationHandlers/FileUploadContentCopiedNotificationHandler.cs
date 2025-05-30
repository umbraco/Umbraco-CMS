using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.PropertyEditors.NotificationHandlers;

/// <summary>
/// Implements a notification handler that processes file uploads when content is copied, making sure the copied contetnt relates to a new instance
/// of the file.
/// </summary>
internal sealed class FileUploadContentCopiedNotificationHandler : FileUploadNotificationHandlerBase, INotificationHandler<ContentCopiedNotification>
{
    private readonly IContentService _contentService;

    private readonly BlockEditorValues<BlockListValue, BlockListLayoutItem> _blockListEditorValues;
    private readonly BlockEditorValues<BlockGridValue, BlockGridLayoutItem> _blockGridEditorValues;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileUploadContentCopiedNotificationHandler"/> class.
    /// </summary>
    public FileUploadContentCopiedNotificationHandler(
        IJsonSerializer jsonSerializer,
        MediaFileManager mediaFileManager,
        IBlockEditorElementTypeCache elementTypeCache,
        ILogger<FileUploadContentCopiedNotificationHandler> logger,
        IContentService contentService)
        : base(jsonSerializer, mediaFileManager, elementTypeCache)
    {
        _blockListEditorValues = new BlockEditorValues<BlockListValue, BlockListLayoutItem>(new BlockListEditorDataConverter(jsonSerializer), elementTypeCache, logger);
        _blockGridEditorValues = new BlockEditorValues<BlockGridValue, BlockGridLayoutItem>(new BlockGridEditorDataConverter(jsonSerializer), elementTypeCache, logger);
        _contentService = contentService;
    }

    /// <inheritdoc/>
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

            if (IsRichTextPropertyType(property.PropertyType))
            {
                isUpdated |= UpdateRichTextProperty(notification, property);

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

        // Copy each of the property values (variants, segments) to the destination.
        foreach (IPropertyValue propertyValue in property.Values)
        {
            var propVal = property.GetValue(propertyValue.Culture, propertyValue.Segment);
            if (propVal == null || propVal is not string sourceUrl || string.IsNullOrWhiteSpace(sourceUrl))
            {
                continue;
            }

            var copyUrl = CopyFile(sourceUrl, notification.Copy, property.PropertyType);

            notification.Copy.SetValue(property.Alias, copyUrl, propertyValue.Culture, propertyValue.Segment);

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

        if (blockEditorData is null)
        {
            return (isUpdated, null);
        }

        IEnumerable<BlockPropertyValue> blockPropertyValues = blockEditorData.BlockValue.ContentData
            .Concat(blockEditorData.BlockValue.SettingsData)
            .SelectMany(x => x.Values);

        isUpdated = UpdateBlockPropertyValues(notification, isUpdated, blockPropertyValues);

        var updatedValue = JsonSerializer.Serialize(blockEditorData.BlockValue);

        return (isUpdated, updatedValue);
    }

    private bool UpdateRichTextProperty(ContentCopiedNotification notification, IProperty property)
    {
        var isUpdated = false;

        foreach (IPropertyValue blockPropertyValue in property.Values)
        {
            var rawBlockPropertyValue = property.GetValue(blockPropertyValue.Culture, blockPropertyValue.Segment);

            RichTextBlockValue? richTextBlockValue = GetRichTextBlockValue(rawBlockPropertyValue);

            (bool hasUpdates, string? updatedValue) = UpdateBlockEditorData(notification, richTextBlockValue);

            if (hasUpdates && string.IsNullOrEmpty(updatedValue) is false)
            {
                RichTextEditorValue? richTextEditorValue = GetRichTextEditorValue(rawBlockPropertyValue);
                if (richTextEditorValue is not null)
                {
                    richTextEditorValue.Blocks = JsonSerializer.Deserialize<RichTextBlockValue>(updatedValue);
                    notification.Copy.SetValue(property.Alias, JsonSerializer.Serialize(richTextEditorValue), blockPropertyValue.Culture, blockPropertyValue.Segment);
                }
            }

            isUpdated |= hasUpdates;
        }

        return isUpdated;
    }

    private (bool, string?) UpdateBlockEditorData(ContentCopiedNotification notification, RichTextBlockValue? richTextBlockValue)
    {
        var isUpdated = false;

        if (richTextBlockValue is null)
        {
            return (isUpdated, null);
        }

        IEnumerable<BlockPropertyValue> blockPropertyValues = richTextBlockValue.ContentData
            .Concat(richTextBlockValue.SettingsData)
            .SelectMany(x => x.Values);

        isUpdated = UpdateBlockPropertyValues(notification, isUpdated, blockPropertyValues);

        var updatedValue = JsonSerializer.Serialize(richTextBlockValue);

        return (isUpdated, updatedValue);
    }

    private bool UpdateBlockPropertyValues(ContentCopiedNotification notification, bool isUpdated, IEnumerable<BlockPropertyValue> blockPropertyValues)
    {
        foreach (BlockPropertyValue blockPropertyValue in blockPropertyValues)
        {
            if (blockPropertyValue.Value is null)
            {
                continue;
            }

            IPropertyType? propertyType = blockPropertyValue.PropertyType;

            if (propertyType is null)
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

        return isUpdated;
    }

    private bool UpdateUploadFieldBlockPropertyValue(BlockPropertyValue blockItemDataValue, ContentCopiedNotification notification, IPropertyType propertyType)
    {
        FileUploadValue? fileUploadValue = FileUploadValueParser.Parse(blockItemDataValue.Value);

        // if original value is empty, we do not need to copy file
        if (string.IsNullOrWhiteSpace(fileUploadValue?.Src))
        {
            return false;
        }

        var copyFileUrl = CopyFile(fileUploadValue.Src, notification.Copy, propertyType);

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

    private string CopyFile(string sourceUrl, IContent destinationContent, IPropertyType propertyType)
    {
        var sourcePath = MediaFileManager.FileSystem.GetRelativePath(sourceUrl);
        var copyPath = MediaFileManager.CopyFile(destinationContent, propertyType, sourcePath);
        return MediaFileManager.FileSystem.GetUrl(copyPath);
    }
}
