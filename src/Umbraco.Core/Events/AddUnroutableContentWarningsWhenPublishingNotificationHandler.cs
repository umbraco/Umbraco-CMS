using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Handles the <see cref="ContentPublishedNotification" /> to add warnings when published content may not be routable.
/// </summary>
public class AddUnroutableContentWarningsWhenPublishingNotificationHandler : INotificationAsyncHandler<ContentPublishedNotification>
{
    private readonly IPublishedUrlInfoProvider _publishedUrlInfoProvider;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly ILocalizedTextService _localizedTextService;
    private readonly IEventMessagesFactory _eventMessagesFactory;
    private readonly ContentSettings _contentSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AddUnroutableContentWarningsWhenPublishingNotificationHandler" /> class.
    /// </summary>
    [Obsolete("Please use the constructor taking an IPublishedUrlInfoProvider. Scheduled for removal in Umbraco 19.")]
    public AddUnroutableContentWarningsWhenPublishingNotificationHandler(
        IPublishedRouter publishedRouter,
        IUmbracoContextAccessor umbracoContextAccessor,
        ILanguageService languageService,
        ILocalizedTextService localizedTextService,
        IContentService contentService,
        IVariationContextAccessor variationContextAccessor,
        ILoggerFactory loggerFactory,
        UriUtility uriUtility,
        IPublishedUrlProvider publishedUrlProvider,
        IDocumentNavigationQueryService navigationQueryService,
        IPublishedContentStatusFilteringService publishedContentStatusFilteringService,
        IEventMessagesFactory eventMessagesFactory,
        IOptions<ContentSettings> contentSettings)
        : this(
            publishedRouter,
            umbracoContextAccessor,
            languageService,
            localizedTextService,
            contentService,
            variationContextAccessor,
            loggerFactory,
            uriUtility,
            publishedUrlProvider,
            navigationQueryService,
            publishedContentStatusFilteringService,
            eventMessagesFactory,
            contentSettings,
            StaticServiceProvider.Instance.GetRequiredService<IPublishedUrlInfoProvider>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AddUnroutableContentWarningsWhenPublishingNotificationHandler" /> class.
    /// </summary>
    /// <param name="publishedRouter">The published router.</param>
    /// <param name="umbracoContextAccessor">The Umbraco context accessor.</param>
    /// <param name="languageService">The language service.</param>
    /// <param name="localizedTextService">The localized text service.</param>
    /// <param name="contentService">The content service.</param>
    /// <param name="variationContextAccessor">The variation context accessor.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="uriUtility">The URI utility.</param>
    /// <param name="publishedUrlProvider">The published URL provider.</param>
    /// <param name="navigationQueryService">The navigation query service.</param>
    /// <param name="publishedContentStatusFilteringService">The published content status filtering service.</param>
    /// <param name="eventMessagesFactory">The event messages factory.</param>
    /// <param name="contentSettings">The content settings.</param>
    /// <param name="publishedUrlInfoProvider">The provider of URL information for published content.</param>
    public AddUnroutableContentWarningsWhenPublishingNotificationHandler(
#pragma warning disable IDE0060 // Remove unused parameter
        IPublishedRouter publishedRouter, // TODO (V19): Remove the unused parameters once the obsolete constructor is removed.
        IUmbracoContextAccessor umbracoContextAccessor,
        ILanguageService languageService,
        ILocalizedTextService localizedTextService,
        IContentService contentService,
        IVariationContextAccessor variationContextAccessor,
        ILoggerFactory loggerFactory,
        UriUtility uriUtility,
        IPublishedUrlProvider publishedUrlProvider,
        IDocumentNavigationQueryService navigationQueryService,
        IPublishedContentStatusFilteringService publishedContentStatusFilteringService,
        IEventMessagesFactory eventMessagesFactory,
        IOptions<ContentSettings> contentSettings,
        IPublishedUrlInfoProvider publishedUrlInfoProvider)
#pragma warning restore IDE0060 // Remove unused parameter
    {
        _publishedUrlInfoProvider = publishedUrlInfoProvider;
        _umbracoContextAccessor = umbracoContextAccessor;
        _localizedTextService = localizedTextService;
        _eventMessagesFactory = eventMessagesFactory;
        _contentSettings = contentSettings.Value;
    }

    /// <inheritdoc />
    public async Task HandleAsync(ContentPublishedNotification notification, CancellationToken cancellationToken)
    {
        if (_contentSettings.ShowUnroutableContentWarnings is false)
        {
            return;
        }

        if (_umbracoContextAccessor.TryGetUmbracoContext(out _) is false)
        {
            return;
        }

        foreach (IContent content in notification.PublishedEntities)
        {
            // Invariant content has a single set of URLs, so a wildcard covers every culture reported.
            string[] publishedCultures = content.ContentType.VariesByCulture()
                ? content.PublishedCultures.ToArray()
                : ["*"];

            if (publishedCultures.Length == 0)
            {
                continue;
            }

            UrlInfo[] urls = (await _publishedUrlInfoProvider.GetAllAsync(content)).ToArray();

            foreach (var culture in publishedCultures)
            {
                UrlInfo[] cultureUrls = urls
                    .Where(u => culture == "*" || u.Culture.InvariantEquals(culture))
                    .ToArray();

                if (cultureUrls.Any(u => u.Url is not null))
                {
                    continue;
                }

                _eventMessagesFactory.Get().Add(CreateWarning(cultureUrls));

                // Only add one warning here, even though there might actually be more.
                break;
            }
        }
    }

    private EventMessage CreateWarning(IEnumerable<UrlInfo> unroutableUrls)
    {
        var reasons = unroutableUrls
            .Select(u => u.Message)
            .WhereNotNull()
            .Where(m => m.Length > 0)
            .Distinct()
            .Select(m => m.EnsureEndsWith('.'))
            .ToArray();

        var message = reasons.Length == 0
            ? _localizedTextService.Localize("content", "unroutableContentWarning")
            : _localizedTextService.Localize("content", "unroutableContentWarningWithReason", [string.Join(" ", reasons)]);

        return new EventMessage(
            _localizedTextService.Localize("content", "unroutableContentWarningHeader"),
            message,
            EventMessageType.Warning);
    }
}
