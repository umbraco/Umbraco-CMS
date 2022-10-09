using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Strings;

public class UrlSegmentProviderCollection : BuilderCollectionBase<IUrlSegmentProvider>
{
    public UrlSegmentProviderCollection(Func<IEnumerable<IUrlSegmentProvider>> items)
        : base(items)
    {
    }
}
