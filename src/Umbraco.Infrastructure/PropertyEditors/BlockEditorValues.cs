// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Used to deserialize json values and clean up any values based on the existence of element types and layout structure.
/// </summary>
public class BlockEditorValues<TValue, TLayout>
    where TValue : BlockValue<TLayout>, new()
    where TLayout : class, IBlockLayoutItem, new()
{
    private readonly BlockEditorDataConverter<TValue, TLayout> _dataConverter;
    private readonly IBlockEditorElementTypeCache _elementTypeCache;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Core.PropertyEditors.BlockEditorValues{TValue, TLayout}"/> class.
    /// </summary>
    /// <param name="dataConverter">The <see cref="BlockEditorDataConverter{TValue, TLayout}"/> used to convert block editor data.</param>
    /// <param name="elementTypeCache">The <see cref="IBlockEditorElementTypeCache"/> used to cache block editor element types.</param>
    /// <param name="logger">The <see cref="ILogger"/> instance used for logging operations within the block editor values.</param>
    public BlockEditorValues(BlockEditorDataConverter<TValue, TLayout> dataConverter, IBlockEditorElementTypeCache elementTypeCache, ILogger logger)
    {
        _dataConverter = dataConverter;
        _elementTypeCache = elementTypeCache;
        _logger = logger;
    }

    /// <summary>
    /// Deserializes the specified property value into a <see cref="BlockEditorData{TValue, TLayout}"/> instance and cleans the resulting data.
    /// </summary>
    /// <param name="propertyValue">The property value to deserialize and clean. If null or whitespace, the method returns <c>null</c>.</param>
    /// <returns>The cleaned <see cref="BlockEditorData{TValue, TLayout}"/> instance, or <c>null</c> if the input is null or whitespace.</returns>
    public BlockEditorData<TValue, TLayout>? DeserializeAndClean(object? propertyValue)
    {
        var propertyValueAsString = propertyValue?.ToString();
        if (string.IsNullOrWhiteSpace(propertyValueAsString))
        {
            return null;
        }

        BlockEditorData<TValue, TLayout> blockEditorData = _dataConverter.Deserialize(propertyValueAsString);
        return Clean(blockEditorData);
    }

    /// <summary>
    /// Converts the specified <paramref name="blockValue"/> to a <see cref="BlockEditorData{TValue, TLayout}"/>, then cleans and returns the result.
    /// </summary>
    /// <param name="blockValue">The value to convert and clean.</param>
    /// <returns>The cleaned <see cref="BlockEditorData{TValue, TLayout}"/>, or <c>null</c> if conversion fails.</returns>
    public BlockEditorData<TValue, TLayout>? ConvertAndClean(TValue blockValue)
    {
        BlockEditorData<TValue, TLayout> blockEditorData = _dataConverter.Convert(blockValue);
        return Clean(blockEditorData);
    }

    private BlockEditorData<TValue, TLayout>? Clean(BlockEditorData<TValue, TLayout> blockEditorData)
    {
        if (blockEditorData.BlockValue.ContentData.Count == 0)
        {
            // if there's no content ensure there's no settings too
            blockEditorData.BlockValue.SettingsData.Clear();
            return null;
        }

        var contentTypePropertyTypes = new Dictionary<string, Dictionary<string, IPropertyType>>();

        // filter out any content that isn't referenced in the layout references
        IEnumerable<Guid> contentTypeKeys = blockEditorData.BlockValue.ContentData.Select(x => x.ContentTypeKey)
            .Union(blockEditorData.BlockValue.SettingsData.Select(x => x.ContentTypeKey)).Distinct();
        IDictionary<Guid, IContentType> contentTypesDictionary = _elementTypeCache.GetMany(contentTypeKeys).ToDictionary(x=>x.Key);

        foreach (BlockItemData block in blockEditorData.BlockValue.ContentData.Where(x =>
                     blockEditorData.References.Any(r => r.ContentKey == x.Key)))
        {
            ResolveBlockItemData(block, contentTypePropertyTypes, contentTypesDictionary);
        }

        // filter out any settings that isn't referenced in the layout references
        foreach (BlockItemData block in blockEditorData.BlockValue.SettingsData.Where(x =>
                     blockEditorData.References.Any(r =>
                         r.SettingsKey.HasValue && r.SettingsKey.Value == x.Key)))
        {
            ResolveBlockItemData(block, contentTypePropertyTypes, contentTypesDictionary);
        }

        // remove blocks that couldn't be resolved
        blockEditorData.BlockValue.ContentData.RemoveAll(x => x.ContentTypeAlias.IsNullOrWhiteSpace());
        blockEditorData.BlockValue.SettingsData.RemoveAll(x => x.ContentTypeAlias.IsNullOrWhiteSpace());

        return blockEditorData;
    }

    private bool ResolveBlockItemData(BlockItemData block, Dictionary<string, Dictionary<string, IPropertyType>> contentTypePropertyTypes, IDictionary<Guid, IContentType> contentTypesDictionary)
    {
        if (contentTypesDictionary.TryGetValue(block.ContentTypeKey, out IContentType? contentType) is false)
        {
            return false;
        }

        // get the prop types for this content type but keep a dictionary of found ones so we don't have to keep re-looking and re-creating
        // objects on each iteration.
        if (!contentTypePropertyTypes.TryGetValue(contentType.Alias, out Dictionary<string, IPropertyType>? propertyTypes))
        {
            propertyTypes = contentTypePropertyTypes[contentType.Alias] = contentType.CompositionPropertyTypes.ToDictionary(x => x.Alias, x => x);
        }

        // resolve the actual property types for all block properties
        foreach (BlockPropertyValue property in block.Values)
        {
            if (!propertyTypes.TryGetValue(property.Alias, out IPropertyType? propertyType))
            {
                _logger.LogWarning(
                    "The property {PropertyAlias} for block {BlockKey} was removed because the property type was not found on {ContentTypeAlias}",
                    property.Alias,
                    block.Key,
                    contentType.Alias);
                continue;
            }

            property.PropertyType = propertyType;
        }

        // remove all block properties that did not resolve a property type
        block.Values.RemoveAll(blockProperty => blockProperty.PropertyType is null);

        block.ContentTypeAlias = contentType.Alias;

        return true;
    }
}
