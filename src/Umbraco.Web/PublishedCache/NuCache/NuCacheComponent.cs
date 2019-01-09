using Umbraco.Core.Components;

namespace Umbraco.Web.PublishedCache.NuCache
{
    public sealed class NuCacheComponent : IComponent
    {
        public NuCacheComponent(IPublishedSnapshotService service)
        {
            // nothing - this just ensures that the service is created at boot time
        }

        public void Initialize()
        { }

        public void Terminate()
        { }
    }
}
