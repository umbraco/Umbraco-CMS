using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Umbraco.Core.Components;
using Umbraco.Core.Composing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Composing.Composers
{
    internal static class ControllersComposer
    {
        /// <summary>
        /// Registers Umbraco controllers.
        /// </summary>
        public static Composition ComposeUmbracoControllers(this Composition composition, Assembly umbracoWebAssembly)
        {
            // notes
            //
            // We scan and auto-registers:
            // - every IController and IHttpController that *we* have in Umbraco.Web
            // - PluginController, RenderMvcController and UmbracoApiController in every assembly
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
            var umbracoWebControllers = composition.TypeLoader.GetTypes<IController>(specificAssemblies: new[] { umbracoWebAssembly });
            //foreach (var controller in umbracoWebControllers.Where(x => !typeof(PluginController).IsAssignableFrom(x)))
            //    Current.Logger.Debug(typeof(LightInjectExtensions), "IController NOT PluginController: " + controller.FullName);
            composition.RegisterControllers(umbracoWebControllers);

            // scan and register every PluginController in everything (PluginController is IDiscoverable and IController)
            var nonUmbracoWebPluginController = composition.TypeLoader.GetTypes<PluginController>().Where(x => x.Assembly != umbracoWebAssembly);
            composition.RegisterControllers(nonUmbracoWebPluginController);

            // can and register every IRenderMvcController in everything (IRenderMvcController is IDiscoverable)
            var renderMvcControllers = composition.TypeLoader.GetTypes<IRenderMvcController>().Where(x => x.Assembly != umbracoWebAssembly);
            composition.RegisterControllers(renderMvcControllers);

            // scan and register every IHttpController in Umbraco.Web
            var umbracoWebHttpControllers = composition.TypeLoader.GetTypes<IHttpController>(specificAssemblies: new[] { umbracoWebAssembly });
            //foreach (var controller in umbracoWebControllers.Where(x => !typeof(UmbracoApiController).IsAssignableFrom(x)))
            //    Current.Logger.Debug(typeof(LightInjectExtensions), "IHttpController NOT UmbracoApiController: " + controller.FullName);
            composition.RegisterControllers(umbracoWebHttpControllers);

            // scan and register every UmbracoApiController in everything (UmbracoApiController is IDiscoverable and IHttpController)
            var nonUmbracoWebApiControllers = composition.TypeLoader.GetTypes<UmbracoApiController>().Where(x => x.Assembly != umbracoWebAssembly);
            composition.RegisterControllers(nonUmbracoWebApiControllers);

            return composition;
        }

        private static void RegisterControllers(this Composition composition, IEnumerable<Type> controllerTypes)
        {
            foreach (var controllerType in controllerTypes)
                composition.Register(controllerType, Lifetime.Transient);
        }
    }
}
