using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Snippets;

public class PartialViewSnippetCollection : BuilderCollectionBase<PartialViewSnippet>
{
    public PartialViewSnippetCollection(Func<IEnumerable<PartialViewSnippet>> items)
        : base(items)
    {
    }
}
