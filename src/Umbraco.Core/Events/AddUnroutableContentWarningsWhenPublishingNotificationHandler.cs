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
    private readonly IPublishedRouter _publishedRouter;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly ILanguageService _languageService;
    private readonly ILocalizedTextService _localizedTextService;
    private readonly IContentService _contentService;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly ILoggerFactory _loggerFactory;
    private readonly UriUtility _uriUtility;
    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly IDocumentNavigationQueryService _navigationQueryService;
    private readonly IPublishedContentStatusFilteringService _publishedContentStatusFilteringService;
    private readonly IEventMessagesFactory _eventMessagesFactory;
    private readonly ContentSettings _contentSettings;

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
    {
        _publishedRouter = publishedRouter;
        _umbracoContextAccessor = umbracoContextAccessor;
        _languageService = languageService;
        _localizedTextService = localizedTextService;
        _contentService = contentService;
        _variationContextAccessor = variationContextAccessor;
        _loggerFactory = loggerFactory;
        _uriUtility = uriUtility;
        _publishedUrlProvider = publishedUrlProvider;
        _navigationQueryService = navigationQueryService;
        _publishedContentStatusFilteringService = publishedContentStatusFilteringService;
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

        foreach (IContent content in notification.PublishedEntities)
        {
            string[]? successfulCultures;
            if (content.ContentType.VariesByCulture() is false)
            {
                // successfulCultures will be null here - change it to a wildcard and utilize this below
                successfulCultures = ["*"];
            }
            else
            {
                successfulCultures = content.PublishedCultures.ToArray();
            }

            if (successfulCultures?.Any() is not true)
            {
                return;
            }

            if (!_umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbracoContext))
            {
                return;
            }

            UrlInfo[] urls = (await content.GetContentUrlsAsync(
                _publishedRouter,
                umbracoContext,
                _languageService,
                _localizedTextService,
                _contentService,
                _variationContextAccessor,
                _loggerFactory.CreateLogger<IContent>(),
                _uriUtility,
                _publishedUrlProvider,
                _navigationQueryService,
                _publishedContentStatusFilteringService)).ToArray();


            EventMessages eventMessages = _eventMessagesFactory.Get();
            foreach (var culture in successfulCultures)
            {
                if (urls.Where(u => u.Culture == culture || culture == "*").All(u => u.Url is null))
                {
                    eventMessages.Add(new EventMessage("Content published", "The document does not have a URL, possibly due to a naming collision with another document. More details can be found under Info.", EventMessageType.Warning));

                    // only add one warning here, even though there might actually be more
                    break;
                }
            }
        }
    }
}
