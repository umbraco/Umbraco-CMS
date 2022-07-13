using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.ContentApps;
using Umbraco.Cms.Core.Dashboards;
using Umbraco.Cms.Core.Editors;
using Umbraco.Cms.Core.HealthChecks;
using Umbraco.Cms.Core.HealthChecks.NotificationMethods;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Media.EmbedProviders;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.Validators;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Sections;
using Umbraco.Cms.Core.Snippets;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Tour;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Core.WebAssets;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IUmbracoBuilder"/>
/// </summary>
public static partial class UmbracoBuilderExtensions
{
    /// <summary>
    /// Adds all core collection builders
    /// </summary>
    internal static void AddAllCoreCollectionBuilders(this IUmbracoBuilder builder)
    {
        builder.CacheRefreshers().Add(() => builder.TypeLoader.GetCacheRefreshers());
        builder.DataEditors().Add(() => builder.TypeLoader.GetDataEditors());
        builder.Actions().Add(() => builder .TypeLoader.GetActions());

        // register known content apps
        builder.ContentApps()
            .Append<ListViewContentAppFactory>()
            .Append<ContentEditorContentAppFactory>()
            .Append<ContentInfoContentAppFactory>()
            .Append<ContentTypeDesignContentAppFactory>()
            .Append<ContentTypeListViewContentAppFactory>()
            .Append<ContentTypePermissionsContentAppFactory>()
            .Append<ContentTypeTemplatesContentAppFactory>()
            .Append<MemberEditorContentAppFactory>()
            .Append<DictionaryContentAppFactory>();

        // all built-in finders in the correct order,
        // devs can then modify this list on application startup
        builder.ContentFinders()
            .Append<ContentFinderByPageIdQuery>()
            .Append<ContentFinderByUrl>()
            .Append<ContentFinderByIdPath>()
            /*.Append<ContentFinderByUrlAndTemplate>() // disabled, this is an odd finder */
            .Append<ContentFinderByUrlAlias>()
            .Append<ContentFinderByRedirectUrl>();
        builder.EditorValidators().Add(() => builder.TypeLoader.GetTypes<IEditorValidator>());
        builder.HealthChecks().Add(() => builder.TypeLoader.GetTypes<HealthCheck>());
        builder.HealthCheckNotificationMethods().Add(() => builder.TypeLoader.GetTypes<IHealthCheckNotificationMethod>());
        builder.TourFilters();
        builder.UrlProviders()
            .Append<AliasUrlProvider>()
            .Append<DefaultUrlProvider>();
        builder.MediaUrlProviders()
            .Append<DefaultMediaUrlProvider>();
        // register back office sections in the order we want them rendered
        builder.Sections()
            .Append<ContentSection>()
            .Append<MediaSection>()
            .Append<SettingsSection>()
            .Append<PackagesSection>()
            .Append<UsersSection>()
            .Append<MembersSection>()
            .Append<FormsSection>()
            .Append<TranslationSection>();
        builder.Components();
        // register core CMS dashboards and 3rd party types - will be ordered by weight attribute & merged with package.manifest dashboards
        builder.Dashboards()
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
            .Add<SettingsDashboard>()
            .Add(builder.TypeLoader.GetTypes<IDashboard>());
        builder.PartialViewSnippets();
        builder.PartialViewMacroSnippets();
        builder.DataValueReferenceFactories();
        builder.PropertyValueConverters().Append(builder.TypeLoader.GetTypes<IPropertyValueConverter>());
        builder.UrlSegmentProviders().Append<DefaultUrlSegmentProvider>();
        builder.ManifestValueValidators()
            .Add<RequiredValidator>()
            .Add<RegexValidator>()
            .Add<DelimitedValueValidator>()
            .Add<EmailValidator>()
            .Add<IntegerValidator>()
            .Add<DecimalValidator>();
        builder.ManifestFilters();
        builder.MediaUrlGenerators();
        // register OEmbed providers - no type scanning - all explicit opt-in of adding types, IEmbedProvider is not IDiscoverable
        builder.EmbedProviders()
            .Append<YouTube>()
            .Append<Twitter>()
            .Append<Vimeo>()
            .Append<DailyMotion>()
            .Append<Flickr>()
            .Append<Slideshare>()
            .Append<Kickstarter>()
            .Append<GettyImages>()
            .Append<Ted>()
            .Append<Soundcloud>()
            .Append<Issuu>()
            .Append<Hulu>()
            .Append<Giphy>()
            .Append<LottieFiles>();
        builder.SearchableTrees().Add(() => builder.TypeLoader.GetTypes<ISearchableTree>());
        builder.BackOfficeAssets();
    }

    /// <summary>
    /// Gets the actions collection builder.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public static ActionCollectionBuilder Actions(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<ActionCollectionBuilder>();

    /// <summary>
    /// Gets the content apps collection builder.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public static ContentAppFactoryCollectionBuilder ContentApps(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<ContentAppFactoryCollectionBuilder>();

    /// <summary>
    /// Gets the content finders collection builder.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public static ContentFinderCollectionBuilder ContentFinders(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<ContentFinderCollectionBuilder>();

    /// <summary>
    /// Gets the editor validators collection builder.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public static EditorValidatorCollectionBuilder EditorValidators(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<EditorValidatorCollectionBuilder>();

    /// <summary>
    /// Gets the health checks collection builder.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public static HealthCheckCollectionBuilder HealthChecks(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<HealthCheckCollectionBuilder>();

    public static HealthCheckNotificationMethodCollectionBuilder HealthCheckNotificationMethods(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<HealthCheckNotificationMethodCollectionBuilder>();

    /// <summary>
    /// Gets the TourFilters collection builder.
    /// </summary>
    public static TourFilterCollectionBuilder TourFilters(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<TourFilterCollectionBuilder>();

    /// <summary>
    /// Gets the URL providers collection builder.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public static UrlProviderCollectionBuilder UrlProviders(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<UrlProviderCollectionBuilder>();

    /// <summary>
    /// Gets the media url providers collection builder.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public static MediaUrlProviderCollectionBuilder MediaUrlProviders(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<MediaUrlProviderCollectionBuilder>();

    /// <summary>
    /// Gets the backoffice sections/applications collection builder.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public static SectionCollectionBuilder Sections(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<SectionCollectionBuilder>();

    /// <summary>
    /// Gets the components collection builder.
    /// </summary>
    public static ComponentCollectionBuilder Components(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<ComponentCollectionBuilder>();

    /// <summary>
    /// Gets the backoffice dashboards collection builder.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public static DashboardCollectionBuilder Dashboards(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<DashboardCollectionBuilder>();

    /// <summary>
    public static PartialViewSnippetCollectionBuilder? PartialViewSnippets(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<PartialViewSnippetCollectionBuilder>();

    public static PartialViewMacroSnippetCollectionBuilder? PartialViewMacroSnippets(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<PartialViewMacroSnippetCollectionBuilder>();

    /// Gets the cache refreshers collection builder.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public static CacheRefresherCollectionBuilder CacheRefreshers(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<CacheRefresherCollectionBuilder>();

    /// <summary>
    /// Gets the map definitions collection builder.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public static MapDefinitionCollectionBuilder MapDefinitions(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<MapDefinitionCollectionBuilder>();

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
    /// Gets the content finders collection builder.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public static MediaUrlGeneratorCollectionBuilder MediaUrlGenerators(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<MediaUrlGeneratorCollectionBuilder>();

    /// <summary>
    /// Gets the backoffice OEmbed Providers collection builder.
    /// </summary>
    /// <param name="builder">The builder.</param>
    [Obsolete("Use EmbedProviders() instead")]
    public static EmbedProvidersCollectionBuilder OEmbedProviders(this IUmbracoBuilder builder)
        => EmbedProviders(builder);

    /// <summary>
    /// Gets the backoffice Embed Providers collection builder.
    /// </summary>
    /// <param name="builder">The builder.</param>
    public static EmbedProvidersCollectionBuilder EmbedProviders(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<EmbedProvidersCollectionBuilder>();

    /// <summary>
    /// Gets the back office searchable tree collection builder
    /// </summary>
    public static SearchableTreeCollectionBuilder SearchableTrees(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<SearchableTreeCollectionBuilder>();

    /// <summary>
    /// Gets the back office custom assets collection builder
    /// </summary>
    public static CustomBackOfficeAssetsCollectionBuilder BackOfficeAssets(this IUmbracoBuilder builder)
        => builder.WithCollectionBuilder<CustomBackOfficeAssetsCollectionBuilder>();
}
