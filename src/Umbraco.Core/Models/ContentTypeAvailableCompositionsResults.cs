namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Used when determining available compositions for a given content type
/// </summary>
public class ContentTypeAvailableCompositionsResults
{
    public ContentTypeAvailableCompositionsResults()
    {
        Ancestors = Enumerable.Empty<IContentTypeComposition>();
        Results = Enumerable.Empty<ContentTypeAvailableCompositionsResult>();
    }

    public ContentTypeAvailableCompositionsResults(
        IEnumerable<IContentTypeComposition> ancestors,
        IEnumerable<ContentTypeAvailableCompositionsResult> results)
    {
        Ancestors = ancestors;
        Results = results;
    }

    public IEnumerable<IContentTypeComposition> Ancestors { get; }

    public IEnumerable<ContentTypeAvailableCompositionsResult> Results { get; }
}
