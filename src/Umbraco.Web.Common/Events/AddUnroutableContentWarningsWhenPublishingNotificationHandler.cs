using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Events;

public class AddUnroutableContentWarningsWhenPublishingNotificationHandler : INotificationAsyncHandler<ContentPublishedNotification>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPublishedRouter _publishedRouter;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly ILanguageService _languageService;
    private readonly ILocalizedTextService _localizedTextService;
    private readonly IContentService _contentService;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly ILoggerFactory _loggerFactory;
    private readonly UriUtility _uriUtility;
    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly IPublishedContentCache _publishedContentCache;
    private readonly IDocumentNavigationQueryService _navigationQueryService;
    private readonly IEventMessagesFactory _eventMessagesFactory;
    private readonly ContentSettings _contentSettings;

    public AddUnroutableContentWarningsWhenPublishingNotificationHandler(
        IHttpContextAccessor httpContextAccessor,
        IPublishedRouter publishedRouter,
        IUmbracoContextAccessor umbracoContextAccessor,
        ILanguageService languageService,
        ILocalizedTextService localizedTextService,
        IContentService contentService,
        IVariationContextAccessor variationContextAccessor,
        ILoggerFactory loggerFactory,
        UriUtility uriUtility,
        IPublishedUrlProvider publishedUrlProvider,
        IPublishedContentCache publishedContentCache,
        IDocumentNavigationQueryService navigationQueryService,
        IEventMessagesFactory eventMessagesFactory,
        IOptions<ContentSettings> contentSettings)
    {
        _httpContextAccessor = httpContextAccessor;
        _publishedRouter = publishedRouter;
        _umbracoContextAccessor = umbracoContextAccessor;
        _languageService = languageService;
        _localizedTextService = localizedTextService;
        _contentService = contentService;
        _variationContextAccessor = variationContextAccessor;
        _loggerFactory = loggerFactory;
        _uriUtility = uriUtility;
        _publishedUrlProvider = publishedUrlProvider;
        _publishedContentCache = publishedContentCache;
        _navigationQueryService = navigationQueryService;
        _eventMessagesFactory = eventMessagesFactory;
        _contentSettings = contentSettings.Value;
    }

    public async Task HandleAsync(ContentPublishedNotification notification, CancellationToken cancellationToken)
    {
        if (_contentSettings.ShowUnroutableContentWarnings is false)
        {
            return;
        }

        // If we don't have an HTTP context, early return. We could be in a background job (e.g. scheduled publish).
        // Without an HTTP context we'll get an exception from the following code, and in any case there's no value
        // in generating visual warnings for the editor as they won't see them.
        if (_httpContextAccessor.HttpContext is null)
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
                _publishedContentCache,
                _navigationQueryService)).ToArray();


            EventMessages eventMessages = _eventMessagesFactory.Get();
            foreach (var culture in successfulCultures)
            {
                if (urls.Where(u => u.Culture == culture || culture == "*").All(u => u.IsUrl is false))
                {
                    eventMessages.Add(new EventMessage("Content published", "The document does not have a URL, possibly due to a naming collision with another document. More details can be found under Info.", EventMessageType.Warning));

                    // only add one warning here, even though there might actually be more
                    break;
                }
            }
        }
    }
}
