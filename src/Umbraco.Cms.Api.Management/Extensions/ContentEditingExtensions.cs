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
}
