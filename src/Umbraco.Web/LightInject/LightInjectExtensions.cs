using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.LightInject;

namespace Umbraco.Web.LightInject
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
            var types = pluginManager.ResolveTypes<IHttpController>(specificAssemblies: new[] { Assembly.GetCallingAssembly() });
            foreach (var type in types)
            {
                container.Register(type, new PerRequestLifeTime());
            }
        }

    }
}
