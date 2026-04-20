namespace Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
///     Represents the context for property rendering, including fallback policies.
/// </summary>
public class PropertyRenderingContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PropertyRenderingContext" /> class.
    /// </summary>
    /// <param name="fallback">The fallback policy to use during property value conversion.</param>
    public PropertyRenderingContext(Fallback fallback) => Fallback = fallback;

    /// <summary>
    ///     Gets the fallback policy for property value resolution.
    /// </summary>
    public Fallback Fallback { get; }
}
