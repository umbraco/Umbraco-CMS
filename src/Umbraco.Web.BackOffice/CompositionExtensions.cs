using Umbraco.Core.Composing;
using Umbraco.Web.Dashboards;

// ReSharper disable once CheckNamespace
namespace Umbraco.Web
{
    /// <summary>
    /// Provides extension methods to the <see cref="Composition"/> class.
    /// </summary>
    public static class WebCompositionExtensions
    {
        #region Collection Builders

        /// <summary>
        /// Gets the backoffice dashboards collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        public static DashboardCollectionBuilder Dashboards(this Composition composition)
            => composition.WithCollectionBuilder<DashboardCollectionBuilder>();

        #endregion

    }
}
