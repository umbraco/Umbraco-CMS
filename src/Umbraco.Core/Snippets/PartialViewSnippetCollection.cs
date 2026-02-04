using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Snippets;

/// <summary>
/// Represents a collection of <see cref="PartialViewSnippet"/> instances.
/// </summary>
/// <remarks>
/// This collection contains all registered partial view snippets that can be used
/// when creating new partial views in the Umbraco backoffice.
/// </remarks>
public class PartialViewSnippetCollection : BuilderCollectionBase<PartialViewSnippet>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PartialViewSnippetCollection"/> class.
    /// </summary>
    /// <param name="items">A factory function that provides the partial view snippets.</param>
    public PartialViewSnippetCollection(Func<IEnumerable<PartialViewSnippet>> items)
        : base(items)
    {
    }
}
