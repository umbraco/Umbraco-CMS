using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Extensions;

internal static class ContentEditingExtensions
{
    /// <summary>
    /// Determines whether the specified model does not vary by culture.
    /// </summary>
    /// <param name="model">The model to check for culture variance.</param>
    /// <returns>True if the model does not vary by culture; otherwise, false.</returns>
    public static bool DoesNotVaryByCulture(this IHasCultureAndSegment model)
        => model.VariesByCulture() == false;

    /// <summary>
    /// Determines whether the specified model does not vary by segment.
    /// </summary>
    /// <param name="model">The model to check for segment variation.</param>
    /// <returns>True if the model does not vary by segment; otherwise, false.</returns>
    public static bool DoesNotVaryBySegment(this IHasCultureAndSegment model)
        => model.VariesBySegment() == false;

    /// <summary>
    /// Determines whether the specified model varies by culture.
    /// </summary>
    /// <param name="model">The model to check for culture variation.</param>
    /// <returns>True if the model varies by culture; otherwise, false.</returns>
    public static bool VariesByCulture(this IHasCultureAndSegment model)
        => model.Culture is not null;

    /// <summary>
    /// Determines whether the specified model varies by segment.
    /// </summary>
    /// <param name="model">The model to check for segment variation.</param>
    /// <returns>True if the model varies by segment; otherwise, false.</returns>
    public static bool VariesBySegment(this IHasCultureAndSegment model)
        => model.Segment is not null;

    /// <summary>
    /// Orders variants by culture, then by segment, using ordinal comparison with invariant (<c>null</c>) first.
    /// </summary>
    /// <remarks>
    /// This is the order in which variants are returned by the Management API. Use it wherever variants are
    /// exposed, so index-based references into one representation also hold for the others.
    /// </remarks>
    /// <param name="variants">The variants to order.</param>
    /// <returns>The variants in their canonical order.</returns>
    public static IOrderedEnumerable<TVariant> OrderByCultureAndSegment<TVariant>(this IEnumerable<TVariant> variants)
        where TVariant : IHasCultureAndSegment
        => variants
            .OrderBy(variant => variant.Culture, StringComparer.Ordinal)
            .ThenBy(variant => variant.Segment, StringComparer.Ordinal);

    /// <summary>
    /// Orders property values by culture, then by segment, then by property alias, using ordinal comparison with
    /// invariant (<c>null</c>) first.
    /// </summary>
    /// <remarks>
    /// This is the order in which property values are returned by the Management API. Use it wherever property
    /// values are exposed, so index-based references into one representation also hold for the others.
    /// </remarks>
    /// <param name="values">The property values to order.</param>
    /// <returns>The property values in their canonical order.</returns>
    public static IOrderedEnumerable<TValue> OrderByCultureSegmentAndAlias<TValue>(this IEnumerable<TValue> values)
        where TValue : ValueModelBase
        => values
            .OrderByCultureAndSegment()
            .ThenBy(value => value.Alias, StringComparer.Ordinal);
}
