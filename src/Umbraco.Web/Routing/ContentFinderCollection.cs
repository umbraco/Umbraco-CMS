using System.Collections.Generic;
using Umbraco.Core.DependencyInjection;

namespace Umbraco.Web.Routing
{
    public class ContentFinderCollection : BuilderCollectionBase<IContentFinder>
    {
        public ContentFinderCollection(IEnumerable<IContentFinder> items)
            : base(items)
        { }
    }
}
