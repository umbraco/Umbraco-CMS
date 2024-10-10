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
        if (value?.GetLayouts() is not IEnumerable<TLayout> layouts)
        {
            return BlockEditorData<TValue, TLayout>.Empty;
        }

        IEnumerable<ContentAndSettingsReference> references = GetBlockReferences(layouts);

        return new BlockEditorData<TValue, TLayout>(references, value);
    }
}
