using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using LightInject;
using Umbraco.Core;
using Umbraco.Core.Plugins;

namespace Umbraco.Web
{
    internal static class LightInjectExtensions
    {
        /// <summary>
        /// Registers all IControllers using the PluginManager for scanning and caching found instances for the calling assembly
        /// </summary>
        /// <param name="container"></param>
        /// <param name="pluginManager"></param>
        public static void RegisterMvcControllers(this IServiceRegistry container, PluginManager pluginManager)
        {
            //TODO: We've already scanned for UmbracoApiControllers and SurfaceControllers - should we scan again 
            // for all controllers? Seems like we should just do this once and then filter. That said here we are
            // only scanning our own single assembly. Hrm.

            var types = pluginManager.ResolveTypes<IController>(specificAssemblies: new[] { Assembly.GetCallingAssembly() });
            foreach (var type in types)
            {
                container.Register(type, new PerRequestLifeTime());
            }
        }

        /// <summary>
        /// Registers all IHttpController using the PluginManager for scanning and caching found instances for the calling assembly
        /// </summary>
        /// <param name="container"></param>
        /// <param name="pluginManager"></param>
        public static void RegisterApiControllers(this IServiceRegistry container, PluginManager pluginManager)
        {
            //TODO: We've already scanned for UmbracoApiControllers and SurfaceControllers - should we scan again 
            // for all controllers? Seems like we should just do this once and then filter. That said here we are
            // only scanning our own single assembly. Hrm.

            var types = pluginManager.ResolveTypes<IHttpController>(specificAssemblies: new[] { Assembly.GetCallingAssembly() });
            foreach (var type in types)
            {
                container.Register(type, new PerRequestLifeTime());
            }
        }

    }
}
