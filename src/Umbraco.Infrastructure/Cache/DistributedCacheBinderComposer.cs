using Umbraco.Core;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// Installs listeners on service events in order to refresh our caches.
    /// </summary>
    [ComposeBefore(typeof(ICoreComposer))] // runs before every other IUmbracoCoreComponent!
    public sealed class DistributedCacheBinderComposer : ComponentComposer<DistributedCacheBinderComponent>, ICoreComposer
    {
        public override void Compose(IUmbracoBuilder builder)
        {
            base.Compose(builder);

            builder.Services.AddUnique<IDistributedCacheBinder, DistributedCacheBinder>();
        }
    }
}
