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
    where TLayout : class, IBlockLayoutItem, new()
{
    private readonly string _propertyEditorAlias;
    private readonly IJsonSerializer _jsonSerializer;

    [Obsolete("Use the constructor that takes IJsonSerializer. Will be removed in V15.")]
    protected BlockEditorDataConverter(string propertyEditorAlias)
        : this(propertyEditorAlias, StaticServiceProvider.Instance.GetRequiredService<IJsonSerializer>())
    {
    }

    protected BlockEditorDataConverter(string propertyEditorAlias, IJsonSerializer jsonSerializer)
    {
        _propertyEditorAlias = propertyEditorAlias;
        _jsonSerializer = jsonSerializer;
    }

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
            blockEditorData = null;
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

    private BlockEditorData<TValue, TLayout> Convert(TValue? value)
    {
        if (value?.Layout == null)
        {
            return BlockEditorData<TValue, TLayout>.Empty;
        }

        IEnumerable<ContentAndSettingsReference> references =
            value.Layout.TryGetValue(_propertyEditorAlias, out IEnumerable<TLayout>? layout)
                ? GetBlockReferences(layout)
                : Enumerable.Empty<ContentAndSettingsReference>();

        return new BlockEditorData<TValue, TLayout>(_propertyEditorAlias, references, value);
    }
}
