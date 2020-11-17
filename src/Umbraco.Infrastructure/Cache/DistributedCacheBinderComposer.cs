﻿using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Cache
{
    /// <summary>
    /// Installs listeners on service events in order to refresh our caches.
    /// </summary>
    [ComposeBefore(typeof(ICoreComposer))] // runs before every other IUmbracoCoreComponent!
    public sealed class DistributedCacheBinderComposer : ComponentComposer<DistributedCacheBinderComponent>, ICoreComposer
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            composition.Services.AddUnique<IDistributedCacheBinder, DistributedCacheBinder>();
        }
    }
}
