using Umbraco.Core.Cache;
using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Manifest;
using Umbraco.Core.PackageActions;
using Umbraco.Core.Persistence.Mappers;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Strings;
using Umbraco.Web.Media.EmbedProviders;
using Umbraco.Web.Search;

namespace Umbraco.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Provides extension methods to the <see cref="IUmbracoBuilder"/> class.
    /// </summary>
    public static partial class UmbracoBuilderExtensions
    {
        /// <summary>
        /// Gets the cache refreshers collection builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public static CacheRefresherCollectionBuilder CacheRefreshers(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<CacheRefresherCollectionBuilder>();

        /// <summary>
        /// Gets the mappers collection builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public static MapperCollectionBuilder Mappers(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<MapperCollectionBuilder>();

        /// <summary>
        /// Gets the package actions collection builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        internal static PackageActionCollectionBuilder PackageActions(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<PackageActionCollectionBuilder>();

        /// <summary>
        /// Gets the data editor collection builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public static DataEditorCollectionBuilder DataEditors(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<DataEditorCollectionBuilder>();

        /// <summary>
        /// Gets the data value reference factory collection builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public static DataValueReferenceFactoryCollectionBuilder DataValueReferenceFactories(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<DataValueReferenceFactoryCollectionBuilder>();

        /// <summary>
        /// Gets the property value converters collection builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public static PropertyValueConverterCollectionBuilder PropertyValueConverters(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<PropertyValueConverterCollectionBuilder>();

        /// <summary>
        /// Gets the url segment providers collection builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public static UrlSegmentProviderCollectionBuilder UrlSegmentProviders(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<UrlSegmentProviderCollectionBuilder>();

        /// <summary>
        /// Gets the validators collection builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        internal static ManifestValueValidatorCollectionBuilder ManifestValueValidators(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<ManifestValueValidatorCollectionBuilder>();

        /// <summary>
        /// Gets the manifest filter collection builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public static ManifestFilterCollectionBuilder ManifestFilters(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<ManifestFilterCollectionBuilder>();

        /// <summary>
        /// Gets the backoffice OEmbed Providers collection builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public static EmbedProvidersCollectionBuilder OEmbedProviders(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<EmbedProvidersCollectionBuilder>();

        /// <summary>
        /// Gets the back office searchable tree collection builder
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static SearchableTreeCollectionBuilder SearchableTrees(this IUmbracoBuilder builder)
            => builder.WithCollectionBuilder<SearchableTreeCollectionBuilder>();
    }
}
