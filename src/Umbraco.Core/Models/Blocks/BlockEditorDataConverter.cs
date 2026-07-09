// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics.CodeAnalysis;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Converts the block JSON data into objects.
/// </summary>
/// <typeparam name="TValue">The type of the block value.</typeparam>
/// <typeparam name="TLayout">The type of the layout item.</typeparam>
public abstract class BlockEditorDataConverter<TValue, TLayout>
    where TValue : BlockValue<TLayout>, new()
    where TLayout : IBlockLayoutItem
{
    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlockEditorDataConverter{TValue, TLayout}" /> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    protected BlockEditorDataConverter(IJsonSerializer jsonSerializer)
        => _jsonSerializer = jsonSerializer;

    /// <summary>
    ///     Tries to deserialize the specified JSON into block editor data.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="blockEditorData">When this method returns, contains the block editor data if deserialization succeeded, or null if it failed.</param>
    /// <returns>
    ///     <c>true</c> if deserialization succeeded; otherwise, <c>false</c>.
    /// </returns>
    public bool TryDeserialize(string json, [MaybeNullWhen(false)] out BlockEditorData<TValue, TLayout> blockEditorData)
    {
        try
        {
            TValue? value = _jsonSerializer.Deserialize<TValue>(json);
            blockEditorData = Convert(value);
            return true;
        }
        catch (Exception)
        {
            blockEditorData = default;
            return false;
        }
    }

    /// <summary>
    ///     Deserializes the specified JSON into block editor data.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized block editor data.</returns>
    public BlockEditorData<TValue, TLayout> Deserialize(string json)
    {
        TValue? value = _jsonSerializer.Deserialize<TValue>(json);
        return Convert(value);
    }

    /// <summary>
    ///     Return the collection of <see cref="IBlockReference" /> from the block editor's Layout
    /// </summary>
    /// <param name="layout"></param>
    /// <returns></returns>
    protected abstract IEnumerable<ContentAndSettingsReference> GetBlockReferences(IEnumerable<TLayout> layout);

    /// <summary>
    ///     Converts the specified block value into block editor data.
    /// </summary>
    /// <param name="value">The block value to convert.</param>
    /// <returns>The converted block editor data.</returns>
    public BlockEditorData<TValue, TLayout> Convert(TValue? value)
    {
        if (value is not null)
        {
            var converted = ConvertOriginalBlockFormat(value.ContentData);
            if (converted)
            {
                ConvertOriginalBlockFormat(value.SettingsData);
                AmendExpose(value);
            }
        }

        TLayout[]? layouts = value?.GetLayouts()?.ToArray();
        if (layouts is null)
        {
            return BlockEditorData<TValue, TLayout>.Empty;
        }

        IEnumerable<ContentAndSettingsReference> references = GetBlockReferences(layouts);

        return new BlockEditorData<TValue, TLayout>(references, value!);
    }

    // this method is only meant to have any effect when migrating block editor values
    // from the original format to the new, variant enabled format
    private static void AmendExpose(TValue value)
        => value.Expose = value.ContentData.ConvertAll(cd => new BlockItemVariation(cd.Key, null, null));

    // this method is only meant to have any effect when migrating block editor values
    // from the original format to the new, variant enabled format
    private static bool ConvertOriginalBlockFormat(List<BlockItemData> blockItemDatas)
    {
        var converted = false;
        foreach (BlockItemData blockItemData in blockItemDatas)
        {
            // only overwrite the Properties collection if none have been added at this point
            if (blockItemData.Values.Any() is false && blockItemData.RawPropertyValues.Any())
            {
                blockItemData.Values = blockItemData
                    .RawPropertyValues
                    .Select(item => new BlockPropertyValue { Alias = item.Key, Value = item.Value })
                    .ToList();
                converted = true;
            }

            // no matter what, clear the RawPropertyValues collection so it is not saved back to the DB
            blockItemData.RawPropertyValues.Clear();

            // assign the correct Key if only a UDI is set
            if (blockItemData.Key == Guid.Empty && blockItemData.Udi is GuidUdi guidUdi)
            {
                blockItemData.Key = guidUdi.Guid;
                converted = true;
            }

            // no matter what, clear the UDI value so it's not saved back to the DB
            blockItemData.Udi = null;
        }

        return converted;
    }
}
