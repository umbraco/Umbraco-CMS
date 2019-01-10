using Umbraco.Core.Components;

namespace Umbraco.Web.Cache
{
    public class DistributedCacheBinderComponent : IComponent
    {
        private readonly IDistributedCacheBinder _binder;

        public DistributedCacheBinderComponent(IDistributedCacheBinder distributedCacheBinder)
        {
            _binder = distributedCacheBinder;
        }

        public void Initialize()
        {
            _binder.BindEvents();
        }

        public void Terminate()
        { }
    }
}
