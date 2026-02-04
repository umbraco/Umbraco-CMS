using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Infrastructure.HybridCache.Services;

public interface IMemberCacheService
{
    Task<IPublishedMember?> Get(IMember member);

    // TODO (V18): Remove the default implementation on this method.
    void Rebuild(IReadOnlyCollection<int> contentTypeIds) => throw new NotImplementedException();
}
