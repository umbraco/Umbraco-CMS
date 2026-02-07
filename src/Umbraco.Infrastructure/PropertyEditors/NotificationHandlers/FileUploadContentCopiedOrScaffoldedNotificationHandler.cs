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
/// Implements a notification handler that processes file uploads when content is copied or scaffolded from a blueprint, making
/// sure the new content references a new instance of the file.
/// </summary>
public class FileUploadContentCopiedOrScaffoldedNotificationHandler : FileUploadNotificationHandlerBase,
    INotificationHandler<ContentCopiedNotification>,
    INotificationHandler<ContentScaffoldedNotification>,
    INotificationHandler<ContentSavedBlueprintNotification>
{
    private readonly IContentService _contentService;
    private readonly BlockEditorValues<BlockListValue, BlockListLayoutItem> _blockListEditorValues;
    private readonly BlockEditorValues<BlockGridValue, BlockGridLayoutItem> _blockGridEditorValues;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileUploadContentCopiedOrScaffoldedNotificationHandler"/> class.
    /// </summary>
    public FileUploadContentCopiedOrScaffoldedNotificationHandler(
        IJsonSerializer jsonSerializer,
        MediaFileManager mediaFileManager,
        IBlockEditorElementTypeCache elementTypeCache,
        ILogger<FileUploadContentCopiedOrScaffoldedNotificationHandler> logger,
        IContentService contentService)
        : base(jsonSerializer, mediaFileManager, elementTypeCache)
    {
        _blockListEditorValues = new(new BlockListEditorDataConverter(jsonSerializer), elementTypeCache, logger);
        _blockGridEditorValues = new(new BlockGridEditorDataConverter(jsonSerializer), elementTypeCache, logger);
        _contentService = contentService;
    }

    /// <inheritdoc/>
    public void Handle(ContentCopiedNotification notification) => Handle(notification.Original, notification.Copy, (IContent c) => _contentService.Save(c));

    /// <inheritdoc/>
    public void Handle(ContentScaffoldedNotification notification) => Handle(notification.Original, notification.Scaffold);

    /// <inheritdoc/>
    public void Handle(ContentSavedBlueprintNotification notification)
    {
        if (notification.CreatedFromContent is null)
        {
            // If there is no original content, we don't need to copy files.
            return;
        }

        Handle(notification.CreatedFromContent, notification.SavedBlueprint, (IContent c) => _contentService.SaveBlueprint(c, null));
    }

    private void Handle(IContent source, IContent destination, Action<IContent>? postUpdateAction = null)
    {
        var isUpdated = false;

        foreach (IProperty property in source.Properties)
        {
            if (IsUploadFieldPropertyType(property.PropertyType))
            {
                isUpdated |= UpdateUploadFieldProperty(destination, property);

                continue;
            }

            if (IsBlockListPropertyType(property.PropertyType))
            {
                isUpdated |= UpdateBlockProperty(destination, property, _blockListEditorValues);

                continue;
            }

            if (IsBlockGridPropertyType(property.PropertyType))
            {
                isUpdated |= UpdateBlockProperty(destination, property, _blockGridEditorValues);

                continue;
            }

            if (IsRichTextPropertyType(property.PropertyType))
            {
                isUpdated |= UpdateRichTextProperty(destination, property);

                continue;
            }
        }

        // If updated, re-save the destination with the updated value.
        if (isUpdated && postUpdateAction is not null)
        {
            postUpdateAction(destination);
        }
    }

    private bool UpdateUploadFieldProperty(IContent content, IProperty property)
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

            var copyUrl = CopyFile(sourceUrl, content, property.PropertyType);

            content.SetValue(property.Alias, copyUrl, propertyValue.Culture, propertyValue.Segment);

            isUpdated = true;
        }

        return isUpdated;
    }

    private bool UpdateBlockProperty<TValue, TLayout>(IContent content, IProperty property, BlockEditorValues<TValue, TLayout> blockEditorValues)
        where TValue : BlockValue<TLayout>, new()
        where TLayout : class, IBlockLayoutItem, new()
    {
        var isUpdated = false;

        foreach (IPropertyValue blockPropertyValue in property.Values)
        {
            var rawBlockPropertyValue = property.GetValue(blockPropertyValue.Culture, blockPropertyValue.Segment);

            BlockEditorData<TValue, TLayout>? blockEditorData = GetBlockEditorData(rawBlockPropertyValue, blockEditorValues);

            (bool hasUpdates, string? updatedValue) = UpdateBlockEditorData(content, blockEditorData);

            if (hasUpdates)
            {
                content.SetValue(property.Alias, updatedValue, blockPropertyValue.Culture, blockPropertyValue.Segment);
            }

            isUpdated |= hasUpdates;
        }

        return isUpdated;
    }

    private (bool, string?) UpdateBlockEditorData<TValue, TLayout>(IContent content, BlockEditorData<TValue, TLayout>? blockEditorData)
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

        isUpdated = UpdateBlockPropertyValues(content, isUpdated, blockPropertyValues);

        var updatedValue = JsonSerializer.Serialize(blockEditorData.BlockValue);

        return (isUpdated, updatedValue);
    }

    private bool UpdateRichTextProperty(IContent content, IProperty property)
    {
        var isUpdated = false;

        foreach (IPropertyValue blockPropertyValue in property.Values)
        {
            var rawBlockPropertyValue = property.GetValue(blockPropertyValue.Culture, blockPropertyValue.Segment);

            RichTextBlockValue? richTextBlockValue = GetRichTextBlockValue(rawBlockPropertyValue);

            (bool hasUpdates, string? updatedValue) = UpdateBlockEditorData(content, richTextBlockValue);

            if (hasUpdates && string.IsNullOrEmpty(updatedValue) is false)
            {
                RichTextEditorValue? richTextEditorValue = GetRichTextEditorValue(rawBlockPropertyValue);
                if (richTextEditorValue is not null)
                {
                    richTextEditorValue.Blocks = JsonSerializer.Deserialize<RichTextBlockValue>(updatedValue);
                    content.SetValue(property.Alias, JsonSerializer.Serialize(richTextEditorValue), blockPropertyValue.Culture, blockPropertyValue.Segment);
                }
            }

            isUpdated |= hasUpdates;
        }

        return isUpdated;
    }

    private (bool, string?) UpdateBlockEditorData(IContent content, RichTextBlockValue? richTextBlockValue)
    {
        var isUpdated = false;

        if (richTextBlockValue is null)
        {
            return (isUpdated, null);
        }

        IEnumerable<BlockPropertyValue> blockPropertyValues = richTextBlockValue.ContentData
            .Concat(richTextBlockValue.SettingsData)
            .SelectMany(x => x.Values);

        isUpdated = UpdateBlockPropertyValues(content, isUpdated, blockPropertyValues);

        var updatedValue = JsonSerializer.Serialize(richTextBlockValue);

        return (isUpdated, updatedValue);
    }

    private bool UpdateBlockPropertyValues(IContent content, bool isUpdated, IEnumerable<BlockPropertyValue> blockPropertyValues)
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
                isUpdated |= UpdateUploadFieldBlockPropertyValue(blockPropertyValue, content, propertyType);

                continue;
            }

            if (IsBlockListPropertyType(propertyType))
            {
                (bool hasUpdates, string? newValue) = UpdateBlockPropertyValue(blockPropertyValue, content, _blockListEditorValues);

                isUpdated |= hasUpdates;

                blockPropertyValue.Value = newValue;

                continue;
            }

            if (IsBlockGridPropertyType(propertyType))
            {
                (bool hasUpdates, string? newValue) = UpdateBlockPropertyValue(blockPropertyValue, content, _blockGridEditorValues);

                isUpdated |= hasUpdates;

                blockPropertyValue.Value = newValue;

                continue;
            }

            if (IsRichTextPropertyType(propertyType))
            {
                (bool hasUpdates, string? newValue) = UpdateRichTextPropertyValue(blockPropertyValue, content);

                if (hasUpdates && string.IsNullOrEmpty(newValue) is false)
                {
                    RichTextEditorValue? richTextEditorValue = GetRichTextEditorValue(blockPropertyValue.Value);
                    if (richTextEditorValue is not null)
                    {
                        isUpdated |= hasUpdates;

                        richTextEditorValue.Blocks = JsonSerializer.Deserialize<RichTextBlockValue>(newValue);
                        blockPropertyValue.Value = richTextEditorValue;
                    }
                }

                continue;
            }
        }

        return isUpdated;
    }

    private bool UpdateUploadFieldBlockPropertyValue(BlockPropertyValue blockItemDataValue, IContent content, IPropertyType propertyType)
    {
        FileUploadValue? fileUploadValue = FileUploadValueParser.Parse(blockItemDataValue.Value);

        // if original value is empty, we do not need to copy file
        if (string.IsNullOrWhiteSpace(fileUploadValue?.Src))
        {
            return false;
        }

        var copyFileUrl = CopyFile(fileUploadValue.Src, content, propertyType);

        blockItemDataValue.Value = copyFileUrl;

        return true;
    }

    private (bool, string?) UpdateBlockPropertyValue<TValue, TLayout>(BlockPropertyValue blockItemDataValue, IContent content, BlockEditorValues<TValue, TLayout> blockEditorValues)
        where TValue : BlockValue<TLayout>, new()
        where TLayout : class, IBlockLayoutItem, new()
    {
        BlockEditorData<TValue, TLayout>? blockItemEditorDataValue = GetBlockEditorData(blockItemDataValue.Value, blockEditorValues);

        return UpdateBlockEditorData(content, blockItemEditorDataValue);
    }

    private (bool, string?) UpdateRichTextPropertyValue(BlockPropertyValue blockItemDataValue, IContent content)
    {
        RichTextBlockValue? richTextBlockValue = GetRichTextBlockValue(blockItemDataValue.Value);
        return UpdateBlockEditorData(content, richTextBlockValue);
    }

    protected virtual string CopyFile(string sourceUrl, IContent destinationContent, IPropertyType propertyType)
    {
        var sourcePath = MediaFileManager.FileSystem.GetRelativePath(sourceUrl);
        var copyPath = MediaFileManager.CopyFile(destinationContent, propertyType, sourcePath);
        return MediaFileManager.FileSystem.GetUrl(copyPath);
    }
}
