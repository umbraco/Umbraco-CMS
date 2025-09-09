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
                IContentType? elementType = elementTypes.FirstOrDefault(x => x.Key == dataItem.ContentTypeKey);
                if (elementType is null)
                {
                    continue;
                }

                IPropertyType? resovledProperty = elementType?.PropertyTypes.FirstOrDefault(pt => pt.Alias == item.Alias);
                if (resovledProperty is not null)
                {
                    item.PropertyType = resovledProperty;
                    continue;
                }

                resovledProperty = elementType?.CompositionPropertyTypes.FirstOrDefault(x => x.Alias == item.Alias);
                if (resovledProperty is not null)
                {
                    item.PropertyType = resovledProperty;
                    continue;
                }
            }
        }
    }
}
