using Umbraco.Core.Components;

namespace Umbraco.Web.Cache
{
    public class DistributedCacheBinderComponent : IComponent
    {
        public DistributedCacheBinderComponent(IDistributedCacheBinder distributedCacheBinder)
        {
            distributedCacheBinder.BindEvents();
        }
    }
}
