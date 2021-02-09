using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Routing
{
    public class ContentFinderCollection : BuilderCollectionBase<IContentFinder>
    {
        public ContentFinderCollection(IEnumerable<IContentFinder> items)
            : base(items)
        { }
    }
}
