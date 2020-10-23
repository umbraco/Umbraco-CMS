using System.Linq;
using Umbraco.Core.Composing;
using Umbraco.Web.Common.Controllers;
using Umbraco.Web.Mvc;
using Umbraco.Web.Website.Controllers;

namespace Umbraco.Extensions
{
    public static class CompositionExtensions
    {
        /// <summary>
        /// Registers Umbraco website controllers.
        /// </summary>
        public static Composition ComposeWebsiteUmbracoControllers(this Composition composition)
        {
            composition.RegisterControllers(new []
            {
                // typeof(UmbProfileController), //TODO introduce when migrated
                // typeof(UmbLoginStatusController),//TODO introduce when migrated
                // typeof(UmbRegisterController),//TODO introduce when migrated
                // typeof(UmbLoginController),//TODO introduce when migrated
                typeof(RenderMvcController),

            });

            var umbracoWebAssembly = typeof(SurfaceController).Assembly;

            // scan and register every PluginController in everything (PluginController is IDiscoverable and IController)
            var nonUmbracoWebPluginController = composition.TypeLoader.GetTypes<PluginController>().Where(x => x.Assembly != umbracoWebAssembly);
            composition.RegisterControllers(nonUmbracoWebPluginController);

            // can and register every IRenderMvcController in everything (IRenderMvcController is IDiscoverable)
            var renderMvcControllers = composition.TypeLoader.GetTypes<IRenderMvcController>().Where(x => x.Assembly != umbracoWebAssembly);
            composition.RegisterControllers(renderMvcControllers);

            return composition;
        }
    }
}
