using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Extensions;

namespace Umbraco.Cms.Infrastructure.PropertyEditors.NotificationHandlers;

/// <summary>
/// Provides base class for notification handler that processes file uploads when a content entity is deleted, removing associated files.
/// </summary>
internal abstract class FileUploadEntityDeletedNotificationHandlerBase : FileUploadNotificationHandlerBase
{
    private readonly BlockEditorValues<BlockListValue, BlockListLayoutItem> _blockListEditorValues;
    private readonly BlockEditorValues<BlockGridValue, BlockGridLayoutItem> _blockGridEditorValues;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileUploadEntityDeletedNotificationHandlerBase"/> class.
    /// </summary>
    protected FileUploadEntityDeletedNotificationHandlerBase(
        IJsonSerializer jsonSerializer,
        MediaFileManager mediaFileManager,
        IBlockEditorElementTypeCache elementTypeCache,
        ILogger logger)
        : base(jsonSerializer, mediaFileManager, elementTypeCache)
    {
        _blockListEditorValues = new BlockEditorValues<BlockListValue, BlockListLayoutItem>(new BlockListEditorDataConverter(jsonSerializer), elementTypeCache, logger);
        _blockGridEditorValues = new BlockEditorValues<BlockGridValue, BlockGridLayoutItem>(new BlockGridEditorDataConverter(jsonSerializer), elementTypeCache, logger);
    }

    /// <summary>
    /// Deletes all file upload property files contained within a collection of content entities.
    /// </summary>
    /// <param name="deletedEntities"></param>
    protected void DeleteContainedFiles(IEnumerable<IContentBase> deletedEntities)
    {
        IReadOnlyList<string> filePathsToDelete = ContainedFilePaths(deletedEntities);
        MediaFileManager.DeleteMediaFiles(filePathsToDelete);
    }

    /// <summary>
    /// Gets the paths to all file upload property files contained within a collection of content entities.
    /// </summary>
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
                yield return MediaFileManager.FileSystem.GetRelativePath(publishedUrl);
            }

            if (propertyValue.EditedValue != null && propertyValue.EditedValue is string editedUrl && !string.IsNullOrWhiteSpace(editedUrl))
            {
                yield return MediaFileManager.FileSystem.GetRelativePath(editedUrl);
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
                FileUploadValue? originalValue = FileUploadValueParser.Parse(blockPropertyValue.Value);

                if (string.IsNullOrWhiteSpace(originalValue?.Src))
                {
                    continue;
                }

                paths.Add(MediaFileManager.FileSystem.GetRelativePath(originalValue.Src));

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

            if (IsRichTextPropertyType(propertyType))
            {
                paths.AddRange(GetPathsFromRichTextPropertyValue(blockPropertyValue));

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

    private IReadOnlyCollection<string> GetPathsFromRichTextPropertyValue(BlockPropertyValue blockItemDataValue)
    {
        RichTextEditorValue? richTextEditorValue = GetRichTextEditorValue(blockItemDataValue.Value);

        // Ensure the property type is populated on all blocks.
        richTextEditorValue?.EnsurePropertyTypePopulatedOnBlocks(ElementTypeCache);

        return GetPathsFromBlockValue(richTextEditorValue?.Blocks);
    }
}
