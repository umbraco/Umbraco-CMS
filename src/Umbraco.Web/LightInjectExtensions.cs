using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using LightInject;
using Umbraco.Core.Composing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Umbraco.Web
{
    internal static class LightInjectExtensions
    {
        /// <summary>
        /// Registers Umbraco controllers.
        /// </summary>
        public static void RegisterUmbracoControllers(this IServiceRegistry container, TypeLoader typeLoader, Assembly umbracoWebAssembly)
        {
            // notes
            //
            // We scan and auto-registers:
            // - every IController and IHttpController that *we* have in Umbraco.Web
            // - PluginController and UmbracoApiController in every assembly
            //
            // We do NOT scan:
            // - any IController or IHttpController (anything not PluginController nor UmbracoApiController), outside of Umbraco.Web
            // which means that users HAVE to explicitly register their own non-Umbraco controllers
            //
            // This is because we try to achieve a balance between "simple" and "fast. Scanning for PluginController or
            // UmbracoApiController is fast-ish because they both are IDiscoverable. Scanning for IController or IHttpController
            // is a full, non-cached scan = expensive, we do it only for 1 assembly.
            //
            // TODO
            // find a way to scan for IController *and* IHttpController in one single pass
            // or, actually register them manually so don't require a full scan for these
            // 5 are IController but not PluginController
            //  Umbraco.Web.Mvc.RenderMvcController
            //  Umbraco.Web.Install.Controllers.InstallController
            //  Umbraco.Web.Macros.PartialViewMacroController
            //  Umbraco.Web.Editors.PreviewController
            //  Umbraco.Web.Editors.BackOfficeController
            // 9 are IHttpController but not UmbracoApiController
            //  Umbraco.Web.Controllers.UmbProfileController
            //  Umbraco.Web.Controllers.UmbLoginStatusController
            //  Umbraco.Web.Controllers.UmbRegisterController
            //  Umbraco.Web.Controllers.UmbLoginController
            //  Umbraco.Web.Mvc.RenderMvcController
            //  Umbraco.Web.Install.Controllers.InstallController
            //  Umbraco.Web.Macros.PartialViewMacroController
            //  Umbraco.Web.Editors.PreviewController
            //  Umbraco.Web.Editors.BackOfficeController

            // scan and register every IController in Umbraco.Web
            var umbracoWebControllers = typeLoader.GetTypes<IController>(specificAssemblies: new[] { umbracoWebAssembly });
            //foreach (var controller in umbracoWebControllers.Where(x => !typeof(PluginController).IsAssignableFrom(x)))
            //    Current.Logger.Debug(typeof(LightInjectExtensions), "IController NOT PluginController: " + controller.FullName);
            container.RegisterControllers(umbracoWebControllers);

            // scan and register every PluginController in everything (PluginController is IDiscoverable and IController)
            var nonUmbracoWebPluginController = typeLoader.GetTypes<PluginController>().Where(x => x.Assembly != umbracoWebAssembly);
            container.RegisterControllers(nonUmbracoWebPluginController);

            // scan and register every IHttpController in Umbraco.Web
            var umbracoWebHttpControllers = typeLoader.GetTypes<IHttpController>(specificAssemblies: new[] { umbracoWebAssembly });
            //foreach (var controller in umbracoWebControllers.Where(x => !typeof(UmbracoApiController).IsAssignableFrom(x)))
            //    Current.Logger.Debug(typeof(LightInjectExtensions), "IHttpController NOT UmbracoApiController: " + controller.FullName);
            container.RegisterControllers(umbracoWebHttpControllers);

            // scan and register every UmbracoApiController in everything (UmbracoApiController is IDiscoverable and IHttpController)
            var nonUmbracoWebApiControllers = typeLoader.GetTypes<UmbracoApiController>().Where(x => x.Assembly != umbracoWebAssembly);
            container.RegisterControllers(nonUmbracoWebApiControllers);
        }

        private static void RegisterControllers(this IServiceRegistry container, IEnumerable<Type> controllerTypes)
        {
            foreach (var controllerType in controllerTypes)
                container.Register(controllerType, new PerRequestLifeTime());
        }
    }
}
