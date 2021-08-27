using Umbraco.Cms.Infrastructure;

namespace Umbraco.Cms.Core
{
    public interface IPublishedContentQueryAccessor
    {
        bool TryGetValue(out IPublishedContentQuery publishedContentQuery);
    }
}
