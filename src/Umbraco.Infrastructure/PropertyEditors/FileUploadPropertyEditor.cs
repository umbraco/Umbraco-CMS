// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Extensions;
using Umbraco.Cms.Infrastructure.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

// TODO (V17): Consider moving the notification handlers that are part of this class to their own class, so we adhere
// better to the single responsibility principle.

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
    private readonly FileUploadValueParser _fileUploadValueParser;
    private readonly IBlockEditorElementTypeCache _elementTypeCache;

    private readonly BlockEditorValues<BlockListValue, BlockListLayoutItem> _blockListEditorValues;
    private readonly BlockEditorValues<BlockGridValue, BlockGridLayoutItem> _blockGridEditorValues;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileUploadPropertyEditor"/> class.
    /// </summary>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 17.")]
    public FileUploadPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        MediaFileManager mediaFileManager,
        IOptionsMonitor<ContentSettings> contentSettings,
        UploadAutoFillProperties uploadAutoFillProperties,
        IContentService contentService,
        IIOHelper ioHelper)
        : this(
              dataValueEditorFactory,
              mediaFileManager,
              contentSettings,
              uploadAutoFillProperties,
              contentService,
              ioHelper,
              StaticServiceProvider.Instance.GetRequiredService<IBlockEditorElementTypeCache>(),
              StaticServiceProvider.Instance.GetRequiredService<IJsonSerializer>(),
              StaticServiceProvider.Instance.GetRequiredService<ILogger<FileUploadPropertyEditor>>(),
              StaticServiceProvider.Instance.GetRequiredService<IBlockEditorElementTypeCache>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileUploadPropertyEditor"/> class.
    /// </summary>
    public FileUploadPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        MediaFileManager mediaFileManager,
        IOptionsMonitor<ContentSettings> contentSettings,
        UploadAutoFillProperties uploadAutoFillProperties,
        IContentService contentService,
        IIOHelper ioHelper,
        IBlockEditorElementTypeCache blockEditorElementTypeCache,
        IJsonSerializer jsonSerializer,
        ILogger<FileUploadPropertyEditor> logger,
        IBlockEditorElementTypeCache elementTypeCache)
        : base(dataValueEditorFactory)
    {
        _mediaFileManager = mediaFileManager ?? throw new ArgumentNullException(nameof(mediaFileManager));
        _contentSettings = contentSettings;
        _uploadAutoFillProperties = uploadAutoFillProperties;
        _contentService = contentService;
        _ioHelper = ioHelper;
        _jsonSerializer = jsonSerializer;
        _elementTypeCache = elementTypeCache;

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
                // TODO: Handle rich text properties with blocks.

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
        FileUploadValue? fileUploadValue = _fileUploadValueParser.Parse(blockItemDataValue.Value);

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
        var sourcePath = _mediaFileManager.FileSystem.GetRelativePath(sourceUrl);
        var copyPath = _mediaFileManager.CopyFile(destinationContent, propertyType, sourcePath);
        return _mediaFileManager.FileSystem.GetUrl(copyPath);
    }

    #endregion

    #region Handle Saving Notifications

    /// <inheritdoc/>
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
                    _uploadAutoFillProperties.Populate(
                        model,
                        autoFillConfig,
                        _mediaFileManager.FileSystem.GetRelativePath(svalue),
                        pvalue.Culture,
                        pvalue.Segment);
                }
            }
        }
    }

    #endregion

    #region Handle Deleted Notifications

    /// <inheritdoc/>
    public void Handle(ContentDeletedNotification notification) => DeleteContainedFiles(notification.DeletedEntities);

    /// <inheritdoc/>
    public void Handle(MediaDeletedNotification notification) => DeleteContainedFiles(notification.DeletedEntities);

    /// <inheritdoc/>
    public void Handle(MemberDeletedNotification notification) => DeleteContainedFiles(notification.DeletedEntities);

    private void DeleteContainedFiles(IEnumerable<IContentBase> deletedEntities)
    {
        IReadOnlyList<string> filePathsToDelete = ContainedFilePaths(deletedEntities);
        _mediaFileManager.DeleteMediaFiles(filePathsToDelete);
    }

    /// <summary>
    ///     The paths to all file upload property files contained within a collection of content entities.
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

            if (IsRichTextPropertyType(property.PropertyType))
            {
                paths.AddRange(GetPathsFromRichTextProperty(property));

                continue;
            }
        }

        return paths.Distinct().ToList().AsReadOnly();
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
            paths.AddRange(GetPathsFromBlockValue(GetBlockEditorData(blockPropertyValue.PublishedValue, blockEditorValues)?.BlockValue));
            paths.AddRange(GetPathsFromBlockValue(GetBlockEditorData(blockPropertyValue.EditedValue, blockEditorValues)?.BlockValue));
        }

        return paths;
    }

    private IReadOnlyCollection<string> GetPathsFromBlockValue(BlockValue? blockValue)
    {
        var paths = new List<string>();

        if (blockValue is null)
        {
            return paths;
        }

        IEnumerable<BlockPropertyValue> blockPropertyValues = blockValue.ContentData
            .Concat(blockValue.SettingsData)
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

        return GetPathsFromBlockValue(blockItemEditorDataValue?.BlockValue);
    }

    private IReadOnlyCollection<string> GetPathsFromRichTextProperty(IProperty property)
    {
        var paths = new List<string>();

        IPropertyValue? propertyValue = property.Values.FirstOrDefault();
        if (propertyValue is null)
        {
            return paths;
        }

        paths.AddRange(GetPathsFromBlockValue(GetRichTextBlockValue(propertyValue.PublishedValue)));
        paths.AddRange(GetPathsFromBlockValue(GetRichTextBlockValue(propertyValue.EditedValue)));

        return paths;
    }

    private RichTextBlockValue? GetRichTextBlockValue(object? value)
    {
        if (value is null)
        {
            return null;
        }

        _jsonSerializer.TryDeserialize(value, out RichTextEditorValue? richTextEditorValue);
        if (richTextEditorValue?.Blocks is null)
        {
            return null;
        }

        // Ensure the property type is populated on all blocks.
        richTextEditorValue.EnsurePropertyTypePopulatedOnBlocks(_elementTypeCache);

        return richTextEditorValue.Blocks;
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
            // If this occurs it means the data is invalid. Shouldn't happen but could if we change the data format.
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
        => propertyType.PropertyEditorAlias == Constants.PropertyEditors.Aliases.UploadField;

    /// <summary>
    ///     Gets a value indicating whether a property is an block list field.
    /// </summary>
    /// <param name="propertyType">The property type.</param>
    /// <returns>
    ///     <c>true</c> if the specified property is an block list field; otherwise, <c>false</c>.
    /// </returns>
    private static bool IsBlockListPropertyType(IPropertyType propertyType)
        => propertyType.PropertyEditorAlias == Constants.PropertyEditors.Aliases.BlockList;

    /// <summary>
    ///     Gets a value indicating whether a property is an block grid field.
    /// </summary>
    /// <param name="propertyType">The property type.</param>
    /// <returns>
    ///     <c>true</c> if the specified property is an block grid field; otherwise, <c>false</c>.
    /// </returns>
    private static bool IsBlockGridPropertyType(IPropertyType propertyType)
        => propertyType.PropertyEditorAlias == Constants.PropertyEditors.Aliases.BlockGrid;

    /// <summary>
    ///     Gets a value indicating whether a property is an rich text field (supporting blocks).
    /// </summary>
    /// <param name="propertyType">The property type.</param>
    /// <returns>
    ///     <c>true</c> if the specified property is an rich text field; otherwise, <c>false</c>.
    /// </returns>
    private static bool IsRichTextPropertyType(IPropertyType propertyType)
        => propertyType.PropertyEditorAlias == Constants.PropertyEditors.Aliases.RichText ||
           propertyType.PropertyEditorAlias == "Umbraco.TinyMCE";
}
