namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Indicates how values can vary.
/// </summary>
/// <remarks>
///     <para>Values can vary by nothing, or culture, or segment, or both.</para>
///     <para>
///         Varying by culture implies that each culture version of a document can
///         be available or not, and published or not, individually. Varying by segment
///         is a property-level thing.
///     </para>
/// </remarks>
[Flags]
public enum ContentVariation : byte
{
    /// <summary>
    ///     Values do not vary.
    /// </summary>
    Nothing = 0,

    /// <summary>
    ///     Values vary by culture.
    /// </summary>
    Culture = 1,

    /// <summary>
    ///     Values vary by segment.
    /// </summary>
    Segment = 2,

    /// <summary>
    ///     Values vary by culture and segment.
    /// </summary>
    CultureAndSegment = Culture | Segment,
}
