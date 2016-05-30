using LightInject;
using Umbraco.Core;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web
{
    // kill this eventually - it's here during the transition
    // provides static access to singletons
    internal static class Current
    {
        public static IServiceContainer Container { get; set; } // ok to set - don't be stupid

        public static UmbracoContext UmbracoContext => Container.GetInstance<IUmbracoContextAccessor>().UmbracoContext;

        public static IFacade Facade => Container.GetInstance<IFacadeAccessor>().Facade;
    }
}
