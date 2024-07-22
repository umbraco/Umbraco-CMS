// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Converts the block json data into objects
/// </summary>
public abstract class BlockEditorDataConverter<TValue, TLayout>
    where TValue : BlockValue<TLayout>, new()
    where TLayout : IBlockLayoutItem
{
    private readonly IJsonSerializer _jsonSerializer;

    [Obsolete("Use the non-obsolete constructor. Will be removed in V15.")]
    protected BlockEditorDataConverter(string propertyEditorAlias)
        : this(propertyEditorAlias, StaticServiceProvider.Instance.GetRequiredService<IJsonSerializer>())
    {
    }

    [Obsolete("Use the non-obsolete constructor. Will be removed in V15.")]
    protected BlockEditorDataConverter(string propertyEditorAlias, IJsonSerializer jsonSerializer)
        : this(jsonSerializer)
    {
    }

    protected BlockEditorDataConverter(IJsonSerializer jsonSerializer)
        => _jsonSerializer = jsonSerializer;

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

    public BlockEditorData<TValue, TLayout> Convert(TValue? value)
    {
        if (value is not null)
        {
            ConvertOriginalBlockFormat(value.ContentData);
            ConvertOriginalBlockFormat(value.SettingsData);
        }

        TLayout[]? layouts = value?.GetLayouts()?.ToArray();
        if (layouts is null)
        {
            return BlockEditorData<TValue, TLayout>.Empty;
        }

        foreach (TLayout layout in layouts.Where(l => l.ContentKey == Guid.Empty))
        {
            layout.ContentKey = (layout.ContentUdi as GuidUdi)?.Guid ?? throw new ArgumentException(nameof(value));
            layout.SettingsKey = (layout.SettingsUdi as GuidUdi)?.Guid;
        }

        IEnumerable<ContentAndSettingsReference> references = GetBlockReferences(layouts);

        return new BlockEditorData<TValue, TLayout>(references, value!);
    }

    // this method is only meant to have any effect when migrating block editor values
    // from the original format to the new, variant enabled format
    private void ConvertOriginalBlockFormat(List<BlockItemData> blockItemDatas)
    {
        foreach (BlockItemData blockItemData in blockItemDatas)
        {
            // only overwrite the Properties collection if none have been added at this point
            if (blockItemData.Properties.Any() is false && blockItemData.RawPropertyValues.Any())
            {
                blockItemData.Properties = blockItemData
                    .RawPropertyValues
                    .Select(item => new BlockPropertyValue { Alias = item.Key, Value = item.Value })
                    .ToList();
            }

            // no matter what, clear the RawPropertyValues collection so it is not saved back to the DB
            blockItemData.RawPropertyValues.Clear();

            // assign the correct Key if only a UDI is set
            if (blockItemData.Key == Guid.Empty && blockItemData.Udi is GuidUdi guidUdi)
            {
                blockItemData.Key = guidUdi.Guid;
            }

            // no matter what, clear the UDI value so it's not saved back to the DB
            blockItemData.Udi = null;
        }
    }
}
