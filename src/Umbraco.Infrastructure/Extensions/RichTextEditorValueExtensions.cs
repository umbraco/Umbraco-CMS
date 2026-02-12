using System.Diagnostics.CodeAnalysis;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Infrastructure.Extensions;

/// <summary>
/// Defines extensions on <see cref="RichTextEditorValue"/>.
/// </summary>
internal static class RichTextEditorValueExtensions
{
    /// <summary>
    /// Ensures that the property type property is populated on all blocks.
    /// </summary>
    /// <param name="richTextEditorValue">The <see cref="RichTextEditorValue"/> providing the blocks.</param>
    /// <param name="elementTypeCache">Cache for element types.</param>
    public static void EnsurePropertyTypePopulatedOnBlocks(this RichTextEditorValue richTextEditorValue, IBlockEditorElementTypeCache elementTypeCache)
    {
        Guid[] elementTypeKeys = (richTextEditorValue.Blocks?.ContentData ?? [])
            .Select(x => x.ContentTypeKey)
            .Union((richTextEditorValue.Blocks?.SettingsData ?? [])
                .Select(x => x.ContentTypeKey))
            .Distinct()
            .ToArray();

        IEnumerable<IContentType> elementTypes = elementTypeCache.GetMany(elementTypeKeys);

        foreach (BlockItemData dataItem in (richTextEditorValue.Blocks?.ContentData ?? [])
            .Union(richTextEditorValue.Blocks?.SettingsData ?? []))
        {
            foreach (BlockPropertyValue item in dataItem.Values)
            {
                if (TryResolvePropertyType(elementTypes, dataItem.ContentTypeKey, item.Alias, out IPropertyType? resolvedPropertyType))
                {
                    item.PropertyType = resolvedPropertyType;
                }
            }
        }
    }

    private static bool TryResolvePropertyType(IEnumerable<IContentType> elementTypes, Guid contentTypeKey, string propertyTypeAlias, [NotNullWhen(true)] out IPropertyType? propertyType)
    {
        IContentType? elementType = elementTypes.FirstOrDefault(x => x.Key == contentTypeKey);
        if (elementType is null)
        {
            propertyType = null;
            return false;
        }

        propertyType = elementType.PropertyTypes.FirstOrDefault(pt => pt.Alias == propertyTypeAlias);
        if (propertyType is not null)
        {
            return true;
        }

        propertyType = elementType.CompositionPropertyTypes.FirstOrDefault(x => x.Alias == propertyTypeAlias);
        return propertyType is not null;
    }
}
