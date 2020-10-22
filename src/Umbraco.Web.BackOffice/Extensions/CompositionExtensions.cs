using Umbraco.Core.Composing;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Web.Common.Controllers;
using Umbraco.Web.Common.Install;
using Umbraco.Web.Trees;

// the namespace here is intentional -  although defined in Umbraco.Web assembly,
// this class should be visible when using Umbraco.Core.Components, alongside
// Umbraco.Core's own CompositionExtensions class

// ReSharper disable once CheckNamespace
namespace Umbraco.Extensions
{
    /// <summary>
    /// Provides extension methods to the <see cref="Composition"/> class.
    /// </summary>
    public static class WebCompositionExtensions
    {
        #region Collection Builders

        /// <summary>
        /// Gets the back office tree collection builder
        /// </summary>
        /// <param name="composition"></param>
        /// <returns></returns>
        public static TreeCollectionBuilder Trees(this Composition composition)
            => composition.WithCollectionBuilder<TreeCollectionBuilder>();

        #endregion


        /// <summary>
        /// Registers Umbraco backoffice controllers.
        /// </summary>
        public static Composition ComposeUmbracoBackOfficeControllers(this Composition composition)
        {
            composition.RegisterControllers(new []
            {
                typeof(BackOfficeController),
                typeof(PreviewController),
                typeof(AuthenticationController),
                typeof(InstallController),
                typeof(InstallApiController),
            });

            var umbracoAuthorizedApiControllers = composition.TypeLoader.GetTypes<UmbracoApiController>();
            composition.RegisterControllers(umbracoAuthorizedApiControllers);

            return composition;
        }
    }
}
