using Umbraco.Core.Composing;
using Umbraco.Core.HealthCheck;
using Umbraco.Core.Manifest;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Actions;
using Umbraco.Web.ContentApps;
using Umbraco.Web.Dashboards;
using Umbraco.Web.Editors;
using Umbraco.Web.Routing;
using Umbraco.Web.Sections;
using Umbraco.Web.Tour;

namespace Umbraco.Core
{
    public static partial class CompositionExtensions
    {

        #region Collection Builders

        /// <summary>
        /// Gets the actions collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <returns></returns>
        public static ActionCollectionBuilder Actions(this Composition composition)
            => composition.WithCollectionBuilder<ActionCollectionBuilder>();

        /// <summary>
        /// Gets the content apps collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <returns></returns>
        public static ContentAppFactoryCollectionBuilder ContentApps(this Composition composition)
            => composition.WithCollectionBuilder<ContentAppFactoryCollectionBuilder>();

        /// <summary>
        /// Gets the content finders collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <returns></returns>
        public static ContentFinderCollectionBuilder ContentFinders(this Composition composition)
            => composition.WithCollectionBuilder<ContentFinderCollectionBuilder>();

        /// <summary>
        /// Gets the editor validators collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <returns></returns>
        public static EditorValidatorCollectionBuilder EditorValidators(this Composition composition)
            => composition.WithCollectionBuilder<EditorValidatorCollectionBuilder>();

        /// <summary>
        /// Gets the health checks collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        public static HealthCheckCollectionBuilder HealthChecks(this Composition composition)
            => composition.WithCollectionBuilder<HealthCheckCollectionBuilder>();

        /// <summary>
        /// Gets the TourFilters collection builder.
        /// </summary>
        public static TourFilterCollectionBuilder TourFilters(this Composition composition)
            => composition.WithCollectionBuilder<TourFilterCollectionBuilder>();

        /// <summary>
        /// Gets the url providers collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        public static UrlProviderCollectionBuilder UrlProviders(this Composition composition)
            => composition.WithCollectionBuilder<UrlProviderCollectionBuilder>();

        /// <summary>
        /// Gets the media url providers collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        public static MediaUrlProviderCollectionBuilder MediaUrlProviders(this Composition composition)
            => composition.WithCollectionBuilder<MediaUrlProviderCollectionBuilder>();

        /// <summary>
        /// Gets the backoffice sections/applications collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        public static SectionCollectionBuilder Sections(this Composition composition)
            => composition.WithCollectionBuilder<SectionCollectionBuilder>();

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
            => composition.WithCollectionBuilder<DashboardCollectionBuilder>()
                .Add<ContentDashboard>()
                .Add<ExamineDashboard>()
                .Add<FormsDashboard>()
                .Add<HealthCheckDashboard>()
                .Add<ManifestDashboard>()
                .Add<MediaDashboard>()
                .Add<MembersDashboard>()
                .Add<ProfilerDashboard>()
                .Add<PublishedStatusDashboard>()
                .Add<RedirectUrlDashboard>()
                .Add<SettingsDashboard>();


        /// <summary>
        /// Gets the content finders collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <returns></returns>
        public static MediaUrlGeneratorCollectionBuilder MediaUrlGenerators(this Composition composition)
            => composition.WithCollectionBuilder<MediaUrlGeneratorCollectionBuilder>();

        #endregion
    }
}
