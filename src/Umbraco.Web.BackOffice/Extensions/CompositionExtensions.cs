using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Composing;
using Umbraco.Web.BackOffice.Trees;

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
        /// <param name="builder"></param>
        /// <returns></returns>
        public static TreeCollectionBuilder Trees(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<TreeCollectionBuilder>();

        #endregion
    }
}
