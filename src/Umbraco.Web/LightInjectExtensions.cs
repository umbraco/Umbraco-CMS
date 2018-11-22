using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using LightInject;
using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Web
{
    internal static class LightInjectExtensions
    {
        /// <summary>
        /// Registers all IControllers using the TypeLoader for scanning and caching found instances for the calling assembly
        /// </summary>
        /// <param name="container"></param>
        /// <param name="typeLoader"></param>
        /// <param name="assembly"></param>
        public static void RegisterMvcControllers(this IServiceRegistry container, TypeLoader typeLoader, Assembly assembly)
        {
            //TODO: We've already scanned for UmbracoApiControllers and SurfaceControllers - should we scan again
            // for all controllers? Seems like we should just do this once and then filter. That said here we are
            // only scanning our own single assembly. Hrm.

            container.RegisterControllers<IController>(typeLoader, assembly);
        }

        /// <summary>
        /// Registers all IHttpController using the TypeLoader for scanning and caching found instances for the calling assembly
        /// </summary>
        /// <param name="container"></param>
        /// <param name="typeLoader"></param>
        /// <param name="assembly"></param>
        public static void RegisterApiControllers(this IServiceRegistry container, TypeLoader typeLoader, Assembly assembly)
        {
            //TODO: We've already scanned for UmbracoApiControllers and SurfaceControllers - should we scan again
            // for all controllers? Seems like we should just do this once and then filter. That said here we are
            // only scanning our own single assembly. Hrm.

            container.RegisterControllers<IHttpController>(typeLoader, assembly);
        }

        private static void RegisterControllers<TController>(this IServiceRegistry container, TypeLoader typeLoader, Assembly assembly)
        {
            var types = typeLoader.GetTypes<TController>(specificAssemblies: new[] { assembly });
            foreach (var type in types)
                container.Register(type, new PerRequestLifeTime());
        }
    }
}
