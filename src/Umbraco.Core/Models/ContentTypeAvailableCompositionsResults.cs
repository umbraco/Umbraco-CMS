namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Used when determining available compositions for a given content type
/// </summary>
public class ContentTypeAvailableCompositionsResults
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentTypeAvailableCompositionsResults" /> class with default empty values.
    /// </summary>
    public ContentTypeAvailableCompositionsResults()
    {
        Ancestors = Enumerable.Empty<IContentTypeComposition>();
        Results = Enumerable.Empty<ContentTypeAvailableCompositionsResult>();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentTypeAvailableCompositionsResults" /> class.
    /// </summary>
    /// <param name="ancestors">The ancestor content types.</param>
    /// <param name="results">The composition availability results.</param>
    public ContentTypeAvailableCompositionsResults(
        IEnumerable<IContentTypeComposition> ancestors,
        IEnumerable<ContentTypeAvailableCompositionsResult> results)
    {
        Ancestors = ancestors;
        Results = results;
    }

    /// <summary>
    ///     Gets the ancestor content types.
    /// </summary>
    public IEnumerable<IContentTypeComposition> Ancestors { get; }

    /// <summary>
    ///     Gets the composition availability results.
    /// </summary>
    public IEnumerable<ContentTypeAvailableCompositionsResult> Results { get; }
}
