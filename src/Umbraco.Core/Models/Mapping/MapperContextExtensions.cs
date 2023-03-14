using Umbraco.Cms.Core.Mapping;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods for the <see cref="MapperContext" /> class.
/// </summary>
public static class MapperContextExtensions
{
    private const string CultureKey = "Map.Culture";
    private const string SegmentKey = "Map.Segment";
    private const string IncludedPropertiesKey = "Map.IncludedProperties";

    /// <summary>
    ///     Gets the context culture.
    /// </summary>
    public static string? GetCulture(this MapperContext context) =>
        context.HasItems && context.Items.TryGetValue(CultureKey, out var obj) && obj is string s ? s : null;

    /// <summary>
    ///     Gets the context segment.
    /// </summary>
    public static string? GetSegment(this MapperContext context) =>
        context.HasItems && context.Items.TryGetValue(SegmentKey, out var obj) && obj is string s ? s : null;

    /// <summary>
    ///     Sets a context culture.
    /// </summary>
    public static void SetCulture(this MapperContext context, string? culture) => context.Items[CultureKey] = culture;

    /// <summary>
    ///     Sets a context segment.
    /// </summary>
    public static void SetSegment(this MapperContext context, string? segment) => context.Items[SegmentKey] = segment;

    /// <summary>
    ///     Get included properties.
    /// </summary>
    public static string[]? GetIncludedProperties(this MapperContext context) => context.HasItems &&
                                                                                 context.Items.TryGetValue(IncludedPropertiesKey, out var obj) && obj is string[] s
            ? s
            : null;

    /// <summary>
    ///     Sets included properties.
    /// </summary>
    public static void SetIncludedProperties(this MapperContext context, string[] properties) =>
        context.Items[IncludedPropertiesKey] = properties;
}
