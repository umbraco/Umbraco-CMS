using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Composing.Composers
{
    internal static class ControllersComposer
    {
        /// <summary>
        /// Registers all IControllers using the TypeLoader for scanning and caching found instances for the calling assembly
        /// </summary>
        public static IContainer ComposeMvcControllers(this IContainer container, TypeLoader typeLoader, Assembly assembly)
        {
            //TODO: We've already scanned for UmbracoApiControllers and SurfaceControllers - should we scan again
            // for all controllers? Seems like we should just do this once and then filter. That said here we are
            // only scanning our own single assembly. Hrm.

            container.RegisterControllers<IController>(typeLoader, assembly);
            return container;
        }

        /// <summary>
        /// Registers all IHttpController using the TypeLoader for scanning and caching found instances for the calling assembly
        /// </summary>
        public static IContainer ComposeApiControllers(this IContainer container, TypeLoader typeLoader, Assembly assembly)
        {
            //TODO: We've already scanned for UmbracoApiControllers and SurfaceControllers - should we scan again
            // for all controllers? Seems like we should just do this once and then filter. That said here we are
            // only scanning our own single assembly. Hrm.

            container.RegisterControllers<IHttpController>(typeLoader, assembly);
            return container;
        }

        private static void RegisterControllers<TController>(this IContainer container, TypeLoader typeLoader, Assembly assembly)
        {
            var types = typeLoader.GetTypes<TController>(specificAssemblies: new[] { assembly });
            foreach (var type in types)
                container.Register(type, Lifetime.Request);
        }
    }
}
