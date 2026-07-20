namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Used when determining available compositions for a given content type
/// </summary>
public class ContentTypeAvailableCompositionsResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentTypeAvailableCompositionsResult" /> class.
    /// </summary>
    /// <param name="composition">The composition content type.</param>
    /// <param name="allowed">Whether the composition is allowed.</param>
    public ContentTypeAvailableCompositionsResult(IContentTypeComposition composition, bool allowed)
    {
        Composition = composition;
        Allowed = allowed;
    }

    /// <summary>
    ///     Gets the composition content type.
    /// </summary>
    public IContentTypeComposition Composition { get; }

    /// <summary>
    ///     Gets a value indicating whether the composition is allowed.
    /// </summary>
    public bool Allowed { get; }
}
