using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Routing;

public class ContentFinderCollection : BuilderCollectionBase<IContentFinder>
{
    public ContentFinderCollection(Func<IEnumerable<IContentFinder>> items)
        : base(items)
    {
    }
}
