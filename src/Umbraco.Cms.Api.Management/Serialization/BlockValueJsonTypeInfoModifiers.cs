// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Runtime.CompilerServices;
using System.Text.Json.Serialization.Metadata;
using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Api.Management.Serialization;

/// <summary>
/// <see cref="JsonTypeInfo"/> modifiers that order <see cref="BlockItemData.Values"/> (by culture, segment,
/// alias) and <see cref="BlockValue.Expose"/> (by culture, segment, content key) deterministically on write,
/// restoring the original order once the write completes.
/// </summary>
/// <remarks>
/// Serialization callbacks run for every occurrence of the type, so nested blocks are ordered at any depth.
/// If the write fails, the original order is not restored.
/// </remarks>
internal static class BlockValueJsonTypeInfoModifiers
{
    private static readonly ConditionalWeakTable<BlockItemData, IList<BlockPropertyValue>> OriginalValues = new();
    private static readonly ConditionalWeakTable<IList<BlockItemVariation>, IList<BlockItemVariation>> OriginalExpose = new();

    public static void Apply(JsonTypeInfo typeInfo)
    {
        if (typeInfo.Type == typeof(BlockItemData))
        {
            typeInfo.AddSerializationCallbacks(SortValues, RestoreValues);
        }
        else if (typeInfo.Type == typeof(IList<BlockItemVariation>))
        {
            typeInfo.AddSerializationCallbacks(SortExpose, RestoreExpose);
        }
    }

    private static void SortValues(object obj)
    {
        var blockItemData = (BlockItemData)obj;
        OriginalValues.AddOrUpdate(blockItemData, blockItemData.Values);
        blockItemData.Values = blockItemData.Values
            .OrderBy(value => value.Culture, StringComparer.Ordinal)
            .ThenBy(value => value.Segment, StringComparer.Ordinal)
            .ThenBy(value => value.Alias, StringComparer.Ordinal)
            .ToList();
    }

    private static void RestoreValues(object obj)
    {
        var blockItemData = (BlockItemData)obj;
        if (OriginalValues.TryGetValue(blockItemData, out IList<BlockPropertyValue>? original))
        {
            blockItemData.Values = original;
            OriginalValues.Remove(blockItemData);
        }
    }

    private static void SortExpose(object obj)
    {
        // Unlike BlockItemData.Values, Expose is handed to us as the list itself - there is no containing
        // property to reassign - so the only way to influence write order is to reorder it in place.
        var expose = (IList<BlockItemVariation>)obj;
        OriginalExpose.AddOrUpdate(expose, expose.ToList());

        IList<BlockItemVariation> sorted = expose
            .OrderBy(variation => variation.Culture, StringComparer.Ordinal)
            .ThenBy(variation => variation.Segment, StringComparer.Ordinal)
            .ThenBy(variation => variation.ContentKey)
            .ToList();

        for (var i = 0; i < expose.Count; i++)
        {
            expose[i] = sorted[i];
        }
    }

    private static void RestoreExpose(object obj)
    {
        var expose = (IList<BlockItemVariation>)obj;
        if (OriginalExpose.TryGetValue(expose, out IList<BlockItemVariation>? original))
        {
            for (var i = 0; i < expose.Count; i++)
            {
                expose[i] = original[i];
            }

            OriginalExpose.Remove(expose);
        }
    }
}
