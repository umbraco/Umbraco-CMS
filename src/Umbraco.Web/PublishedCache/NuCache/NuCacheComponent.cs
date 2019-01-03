using Umbraco.Core.Components;

namespace Umbraco.Web.PublishedCache.NuCache
{
    public class NuCacheComponent : IComponent
    {
        public void Initialize(IPublishedSnapshotService service)
        {
            // nothing - this just ensures that the service is created at boot time
        }
    }
}
