using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Umbraco.Core.Components;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Composing.Composers
{
    internal static class ControllersComposer
    {
        /// <summary>
        /// Registers all IControllers using the TypeLoader for scanning and caching found instances for the calling assembly
        /// </summary>
        public static Composition ComposeMvcControllers(this Composition composition, Assembly assembly)
        {
            //TODO: We've already scanned for UmbracoApiControllers and SurfaceControllers - should we scan again
            // for all controllers? Seems like we should just do this once and then filter. That said here we are
            // only scanning our own single assembly. Hrm.

            composition.RegisterControllers<IController>(assembly);
            return composition;
        }

        /// <summary>
        /// Registers all IHttpController using the TypeLoader for scanning and caching found instances for the calling assembly
        /// </summary>
        public static Composition ComposeApiControllers(this Composition composition, Assembly assembly)
        {
            //TODO: We've already scanned for UmbracoApiControllers and SurfaceControllers - should we scan again
            // for all controllers? Seems like we should just do this once and then filter. That said here we are
            // only scanning our own single assembly. Hrm.

            composition.RegisterControllers<IHttpController>(assembly);
            return composition;
        }

        private static void RegisterControllers<TController>(this Composition composition, Assembly assembly)
        {
            var types = composition.TypeLoader.GetTypes<TController>(specificAssemblies: new[] { assembly });
            foreach (var type in types)
                composition.Register(type, Lifetime.Request);
        }
    }
}
