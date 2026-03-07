using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Extensions;

namespace Umbraco.Cms.Infrastructure.PropertyEditors.NotificationHandlers;

/// <summary>
/// Provides a base class for all notification handlers relating to file uploads in property editors.
/// </summary>
public abstract class FileUploadNotificationHandlerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FileUploadNotificationHandlerBase"/> class.
    /// </summary>
    protected FileUploadNotificationHandlerBase(
        IJsonSerializer jsonSerializer,
        MediaFileManager mediaFileManager,
        IBlockEditorElementTypeCache elementTypeCache)
    {
        JsonSerializer = jsonSerializer;
        MediaFileManager = mediaFileManager;
        ElementTypeCache = elementTypeCache;
        FileUploadValueParser = new FileUploadValueParser(jsonSerializer);
    }

    /// <summary>
    /// Gets the <see cref="IJsonSerializer" /> used for serializing and deserializing values.
    /// </summary>
    protected IJsonSerializer JsonSerializer { get; }

    /// <summary>
    /// Gets the <see cref="MediaFileManager" /> used for managing media files.
    /// </summary>
    protected MediaFileManager MediaFileManager { get; }

    /// <summary>
    /// Gets the <see cref="IBlockEditorElementTypeCache"/> used for caching block editor element types.
    /// </summary>
    protected IBlockEditorElementTypeCache ElementTypeCache { get; }

    /// <summary>
    /// Gets the <see cref="FileUploadValueParser" /> used for parsing file upload values.
    /// </summary>
    protected FileUploadValueParser FileUploadValueParser { get; }

    /// <summary>
    ///     Gets a value indicating whether a property is an upload field.
    /// </summary>
    /// <param name="propertyType">The property type.</param>
    /// <returns>
    ///     <c>true</c> if the specified property is an upload field; otherwise, <c>false</c>.
    /// </returns>
    protected virtual bool IsUploadFieldPropertyType(IPropertyType propertyType)
        => propertyType.PropertyEditorAlias == Constants.PropertyEditors.Aliases.UploadField;

    /// <summary>
    ///     Gets a value indicating whether a property is an block list field.
    /// </summary>
    /// <param name="propertyType">The property type.</param>
    /// <returns>
    ///     <c>true</c> if the specified property is an block list field; otherwise, <c>false</c>.
    /// </returns>
    protected static bool IsBlockListPropertyType(IPropertyType propertyType)
        => propertyType.PropertyEditorAlias == Constants.PropertyEditors.Aliases.BlockList;

    /// <summary>
    ///     Gets a value indicating whether a property is an block grid field.
    /// </summary>
    /// <param name="propertyType">The property type.</param>
    /// <returns>
    ///     <c>true</c> if the specified property is an block grid field; otherwise, <c>false</c>.
    /// </returns>
    protected static bool IsBlockGridPropertyType(IPropertyType propertyType)
        => propertyType.PropertyEditorAlias == Constants.PropertyEditors.Aliases.BlockGrid;

    /// <summary>
    ///     Gets a value indicating whether a property is an rich text field (supporting blocks).
    /// </summary>
    /// <param name="propertyType">The property type.</param>
    /// <returns>
    ///     <c>true</c> if the specified property is an rich text field; otherwise, <c>false</c>.
    /// </returns>
    protected static bool IsRichTextPropertyType(IPropertyType propertyType)
        => propertyType.PropertyEditorAlias == Constants.PropertyEditors.Aliases.RichText ||
           propertyType.PropertyEditorAlias == "Umbraco.TinyMCE";

    /// <summary>
    /// Deserializes the block editor data value.
    /// </summary>
    protected static BlockEditorData<TValue, TLayout>? GetBlockEditorData<TValue, TLayout>(object? value, BlockEditorValues<TValue, TLayout> blockListEditorValues)
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

    /// <summary>
    /// Deserializes the rich text editor value.
    /// </summary>
    protected RichTextEditorValue? GetRichTextEditorValue(object? value)
    {
        if (value is null)
        {
            return null;
        }

        JsonSerializer.TryDeserialize(value, out RichTextEditorValue? richTextEditorValue);
        return richTextEditorValue;
    }

    /// <summary>
    /// Deserializes the rich text block value.
    /// </summary>
    protected RichTextBlockValue? GetRichTextBlockValue(object? value)
    {
        RichTextEditorValue? richTextEditorValue = GetRichTextEditorValue(value);
        if (richTextEditorValue?.Blocks is null)
        {
            return null;
        }

        // Ensure the property type is populated on all blocks.
        richTextEditorValue.EnsurePropertyTypePopulatedOnBlocks(ElementTypeCache);

        return richTextEditorValue.Blocks;
    }
}
