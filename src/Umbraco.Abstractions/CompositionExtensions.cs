using Umbraco.Core.Composing;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Dashboards;

namespace Umbraco.Core
{
    public static partial class CompositionExtensions
    {

        #region Collection Builders

        /// <summary>
        /// Gets the components collection builder.
        /// </summary>
        public static ComponentCollectionBuilder Components(this Composition composition)
            => composition.WithCollectionBuilder<ComponentCollectionBuilder>();


        /// <summary>
        /// Gets the backoffice dashboards collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        public static DashboardCollectionBuilder Dashboards(this Composition composition)
            => composition.WithCollectionBuilder<DashboardCollectionBuilder>();


        /// <summary>
        /// Gets the content finders collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <returns></returns>
        public static DataEditorWithMediaPathCollectionBuilder DataEditorsWithMediaPath(this Composition composition)
            => composition.WithCollectionBuilder<DataEditorWithMediaPathCollectionBuilder>();

        #endregion
    }
}
