// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

/// <summary>
/// Helpers for resolving the effective variance of a property holding a block editor value.
/// </summary>
internal static class BlockPropertyVariance
{
    /// <summary>
    /// Resolves the culture of the stored property value a block value was loaded from.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A block property only stores its cultures separately when it actually varies by culture, which is its own
    /// variance narrowed by the variance of the content type it belongs to - <see cref="IPublishedPropertyType.Variations"/>
    /// carries the property's own flag, unintersected. When the property does not vary, every culture shares a single
    /// stored value and there is no owning culture.
    /// </para>
    /// </remarks>
    /// <param name="variationContextAccessor">Accessor for the current variation context.</param>
    /// <param name="owner">The content or element that owns the block property.</param>
    /// <param name="propertyType">The property type holding the block value.</param>
    /// <returns>The culture the stored property value belongs to, or <c>null</c> when the property does not vary by culture.</returns>
    public static string? OwningPropertyCulture(
        IVariationContextAccessor variationContextAccessor,
        IPublishedElement owner,
        IPublishedPropertyType propertyType)
        => (owner.ContentType.Variations & propertyType.Variations).VariesByCulture()
            ? variationContextAccessor.VariationContext?.Culture
            : null;
}
