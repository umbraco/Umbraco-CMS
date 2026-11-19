using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Extensions;

namespace Umbraco.Cms.Infrastructure.PropertyEditors.NotificationHandlers;

/// <summary>
/// Provides base class for notification handler that processes file uploads when a content entity is deleted or media
/// operations are carried out, processing the associated files.
/// </summary>
internal sealed class FileUploadContentDeletedNotificationHandler : FileUploadNotificationHandlerBase,
    INotificationHandler<ContentDeletedNotification>,
    INotificationHandler<ContentDeletedBlueprintNotification>,
    INotificationHandler<MediaDeletedNotification>,
    INotificationHandler<MediaMovedToRecycleBinNotification>,
    INotificationHandler<MediaMovedNotification>,
    INotificationHandler<MemberDeletedNotification>
{
    private readonly BlockEditorValues<BlockListValue, BlockListLayoutItem> _blockListEditorValues;
    private readonly BlockEditorValues<BlockGridValue, BlockGridLayoutItem> _blockGridEditorValues;
    private readonly ILogger<FileUploadContentDeletedNotificationHandler> _logger;
    private ContentSettings _contentSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileUploadContentDeletedNotificationHandler"/> class.
    /// </summary>
    public FileUploadContentDeletedNotificationHandler(
        IJsonSerializer jsonSerializer,
        MediaFileManager mediaFileManager,
        IBlockEditorElementTypeCache elementTypeCache,
        ILogger<FileUploadContentDeletedNotificationHandler> logger,
        IOptionsMonitor<ContentSettings> contentSettngs)
        : base(jsonSerializer, mediaFileManager, elementTypeCache)
    {
        _blockListEditorValues = new(new BlockListEditorDataConverter(jsonSerializer), elementTypeCache, logger);
        _blockGridEditorValues = new(new BlockGridEditorDataConverter(jsonSerializer), elementTypeCache, logger);
        _logger = logger;

        _contentSettings = contentSettngs.CurrentValue;
        contentSettngs.OnChange(x => _contentSettings = x);
    }

    /// <inheritdoc/>
    public void Handle(ContentDeletedNotification notification) => DeleteContainedFiles(notification.DeletedEntities);

    /// <inheritdoc/>
    public void Handle(ContentDeletedBlueprintNotification notification) => DeleteContainedFiles(notification.DeletedBlueprints);

    /// <inheritdoc/>
    public void Handle(MediaDeletedNotification notification)
    {
        if (_contentSettings.EnableMediaRecycleBinProtection)
        {
            RecycleBinMediaProtectionHelper.DeleteContainedFilesWithProtection(
                notification.DeletedEntities,
                ContainedFilePaths,
                MediaFileManager);
            return;
        }

        DeleteContainedFiles(notification.DeletedEntities);
    }

    /// <inheritdoc/>
    public void Handle(MediaMovedToRecycleBinNotification notification)
    {
        if (_contentSettings.EnableMediaRecycleBinProtection is false)
        {
            return;
        }

        SuffixContainedFiles(
            notification.MoveInfoCollection
                .Select(x => x.Entity));
    }

    /// <inheritdoc/>
    public void Handle(MediaMovedNotification notification)
    {
        if (_contentSettings.EnableMediaRecycleBinProtection is false)
        {
            return;
        }

        RemoveSuffixFromContainedFiles(
            notification.MoveInfoCollection
                .Where(x => x.OriginalPath.StartsWith($"{Constants.System.RootString},{Constants.System.RecycleBinMediaString}"))
                .Select(x => x.Entity));
    }

    /// <inheritdoc/>
    public void Handle(MemberDeletedNotification notification) => DeleteContainedFiles(notification.DeletedEntities);

    /// <summary>
    /// Deletes all file upload property files contained within a collection of content entities.
    /// </summary>
    /// <param name="deletedEntities">Delete media entities.</param>
    private void DeleteContainedFiles(IEnumerable<IContentBase> deletedEntities)
    {
        IReadOnlyList<string> filePathsToDelete = ContainedFilePaths(deletedEntities);
        MediaFileManager.DeleteMediaFiles(filePathsToDelete);
    }

    /// <summary>
    /// Renames all file upload property files contained within a collection of media entities that have been moved to the recycle bin.
    /// </summary>
    /// <param name="trashedMedia">Media entities that have been moved to the recycle bin.</param>
    private void SuffixContainedFiles(IEnumerable<IMedia> trashedMedia)
    {
        IEnumerable<string> filePathsToRename = ContainedFilePaths(trashedMedia);
        RecycleBinMediaProtectionHelper.SuffixContainedFiles(filePathsToRename, MediaFileManager);
    }

    /// <summary>
    /// Renames all file upload property files contained within a collection of media entities that have been restored from the recycle bin.
    /// </summary>
    /// <param name="restoredMedia">Media entities that have been restored from the recycle bin.</param>
    private void RemoveSuffixFromContainedFiles(IEnumerable<IMedia> restoredMedia)
    {
        IEnumerable<string> filePathsToRename = ContainedFilePaths(restoredMedia);
        RecycleBinMediaProtectionHelper.RemoveSuffixFromContainedFiles(filePathsToRename, MediaFileManager);
    }

    /// <summary>
    /// Gets the paths to all file upload property files contained within a collection of content entities.
    /// </summary>
    private IReadOnlyList<string> ContainedFilePaths(IEnumerable<IContentBase> entities)
    {
        var paths = new List<string>();

        foreach (IContentBase entity in entities)
        {
            foreach (IProperty property in entity.Properties)
            {
                CollectContainedFilePaths(paths, entity.Key, property);
            }
        }

        return paths.Distinct().ToList().AsReadOnly();
    }

    private void CollectContainedFilePaths(List<string> paths, Guid contentKey, IProperty property)
    {
        if (IsUploadFieldPropertyType(property.PropertyType))
        {
            paths.AddRange(GetPathsFromUploadFieldProperty(property, contentKey));

            return;
        }

        if (IsBlockListPropertyType(property.PropertyType))
        {
            paths.AddRange(GetPathsFromBlockProperty(property, contentKey, _blockListEditorValues));

            return;
        }

        if (IsBlockGridPropertyType(property.PropertyType))
        {
            paths.AddRange(GetPathsFromBlockProperty(property, contentKey, _blockGridEditorValues));

            return;
        }

        if (IsRichTextPropertyType(property.PropertyType))
        {
            paths.AddRange(GetPathsFromRichTextProperty(property, contentKey));
        }
    }

    private IEnumerable<string> GetPathsFromUploadFieldProperty(IProperty property, Guid contentKey)
    {
        foreach (IPropertyValue propertyValue in property.Values)
        {
            if (propertyValue.PublishedValue is string publishedUrl && !string.IsNullOrWhiteSpace(publishedUrl))
            {
                var publishedPath = MediaFileManager.FileSystem.GetRelativePath(publishedUrl);
                if (IsOwnedFile(publishedPath, contentKey, property.PropertyType.Key))
                {
                    yield return publishedPath;
                }
            }

            if (propertyValue.EditedValue is string editedUrl && !string.IsNullOrWhiteSpace(editedUrl))
            {
                var editedPath = MediaFileManager.FileSystem.GetRelativePath(editedUrl);
                if (IsOwnedFile(editedPath, contentKey, property.PropertyType.Key))
                {
                    yield return editedPath;
                }
            }
        }
    }

    private bool IsOwnedFile(string relativePath, Guid contentKey, Guid propertyTypeKey)
    {
        if (MediaFileManager.IsFileOwnedBy(relativePath, contentKey, propertyTypeKey))
        {
            return true;
        }

        // The stored path does not resolve to one this content and property type could own, so it references
        // another item's file. Exclude it from the delete/rename operation - the value may have been tampered
        // with to target a file the acting user is not authorized to affect.
        _logger.LogWarning(
            "Skipping media file operation for path '{Path}' on content {ContentKey}: the path does not belong to property type {PropertyTypeKey}.",
            relativePath,
            contentKey,
            propertyTypeKey);
        return false;
    }

    private IReadOnlyCollection<string> GetPathsFromBlockProperty<TValue, TLayout>(IProperty property, Guid contentKey, BlockEditorValues<TValue, TLayout> blockEditorValues)
        where TValue : BlockValue<TLayout>, new()
        where TLayout : class, IBlockLayoutItem, new()
    {
        var paths = new List<string>();

        foreach (IPropertyValue blockPropertyValue in property.Values)
        {
            paths.AddRange(GetPathsFromBlockValue(GetBlockEditorData(blockPropertyValue.PublishedValue, blockEditorValues)?.BlockValue, contentKey));
            paths.AddRange(GetPathsFromBlockValue(GetBlockEditorData(blockPropertyValue.EditedValue, blockEditorValues)?.BlockValue, contentKey));
        }

        return paths;
    }

    private IReadOnlyCollection<string> GetPathsFromBlockValue(BlockValue? blockValue, Guid contentKey)
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
                FileUploadValue? originalValue = FileUploadValueParser.Parse(blockPropertyValue.Value);

                if (string.IsNullOrWhiteSpace(originalValue?.Src))
                {
                    continue;
                }

                // The file for a block-nested upload is stored under the owning content key and the block's
                // property type key (see BlockValuePropertyValueEditorBase.FromEditor), so verify ownership
                // against those before allowing a destructive operation on the path.
                var uploadPath = MediaFileManager.FileSystem.GetRelativePath(originalValue.Src);
                if (IsOwnedFile(uploadPath, contentKey, propertyType.Key))
                {
                    paths.Add(uploadPath);
                }

                continue;
            }

            if (IsBlockListPropertyType(propertyType))
            {
                paths.AddRange(GetPathsFromBlockPropertyValue(blockPropertyValue, contentKey, _blockListEditorValues));

                continue;
            }

            if (IsBlockGridPropertyType(propertyType))
            {
                paths.AddRange(GetPathsFromBlockPropertyValue(blockPropertyValue, contentKey, _blockGridEditorValues));

                continue;
            }

            if (IsRichTextPropertyType(propertyType))
            {
                paths.AddRange(GetPathsFromRichTextPropertyValue(blockPropertyValue, contentKey));

                continue;
            }
        }

        return paths;
    }

    private IReadOnlyCollection<string> GetPathsFromBlockPropertyValue<TValue, TLayout>(BlockPropertyValue blockItemDataValue, Guid contentKey, BlockEditorValues<TValue, TLayout> blockEditorValues)
        where TValue : BlockValue<TLayout>, new()
        where TLayout : class, IBlockLayoutItem, new()
    {
        BlockEditorData<TValue, TLayout>? blockItemEditorDataValue = GetBlockEditorData(blockItemDataValue.Value, blockEditorValues);

        return GetPathsFromBlockValue(blockItemEditorDataValue?.BlockValue, contentKey);
    }

    private IReadOnlyCollection<string> GetPathsFromRichTextProperty(IProperty property, Guid contentKey)
    {
        var paths = new List<string>();

        IPropertyValue? propertyValue = property.Values.FirstOrDefault();
        if (propertyValue is null)
        {
            return paths;
        }

        paths.AddRange(GetPathsFromBlockValue(GetRichTextBlockValue(propertyValue.PublishedValue), contentKey));
        paths.AddRange(GetPathsFromBlockValue(GetRichTextBlockValue(propertyValue.EditedValue), contentKey));

        return paths;
    }

    private IReadOnlyCollection<string> GetPathsFromRichTextPropertyValue(BlockPropertyValue blockItemDataValue, Guid contentKey)
    {
        RichTextEditorValue? richTextEditorValue = GetRichTextEditorValue(blockItemDataValue.Value);

        // Ensure the property type is populated on all blocks.
        richTextEditorValue?.EnsurePropertyTypePopulatedOnBlocks(ElementTypeCache);

        return GetPathsFromBlockValue(richTextEditorValue?.Blocks, contentKey);
    }
}
