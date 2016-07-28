using System;
using System.Collections.Generic;
using LightInject;
using Umbraco.Core.Strings;

namespace Umbraco.Core.DependencyInjection
{
    // this class is here to support the transition from singletons and resolvers to injection,
    // by providing a static access to singleton services - it is initialized once with a service
    // container, in CoreBootManager.
    public static class Current
    {
        internal static IServiceContainer CurrentContainer { get; set; } // ok to set - don't be stupid

        public static IServiceContainer Container
        {
            get
            {
                if (CurrentContainer == null) throw new Exception("oops:container");
                return CurrentContainer;
            }
        }

        public static IEnumerable<IUrlSegmentProvider> UrlSegmentProviders
            => Container.GetInstance<UrlSegmentProviderCollection>();
    }
}
