using System.Globalization;
using Examine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Provides an implementation of <see cref="IContentFinder" /> that runs the legacy 404 logic.
/// </summary>
public class ContentFinderByConfigured404 : IContentLastChanceFinder
{
    private readonly IEntityService _entityService;
    private readonly IExamineManager _examineManager;
    private readonly ILogger<ContentFinderByConfigured404> _logger;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private ContentSettings _contentSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentFinderByConfigured404" /> class.
    /// </summary>
    public ContentFinderByConfigured404(
        ILogger<ContentFinderByConfigured404> logger,
        IEntityService entityService,
        IOptionsMonitor<ContentSettings> contentSettings,
        IExamineManager examineManager,
        IVariationContextAccessor variationContextAccessor,
        IUmbracoContextAccessor umbracoContextAccessor)
    {
        _logger = logger;
        _entityService = entityService;
        _contentSettings = contentSettings.CurrentValue;
        _examineManager = examineManager;
        _variationContextAccessor = variationContextAccessor;
        _umbracoContextAccessor = umbracoContextAccessor;

        contentSettings.OnChange(x => _contentSettings = x);
    }

    /// <summary>
    ///     Tries to find and assign an Umbraco document to a <c>PublishedRequest</c>.
    /// </summary>
    /// <param name="frequest">The <c>PublishedRequest</c>.</param>
    /// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
    public Task<bool> TryFindContent(IPublishedRequestBuilder frequest)
    {
        if (!_umbracoContextAccessor.TryGetUmbracoContext(out IUmbracoContext? umbracoContext))
        {
            return Task.FromResult(false);
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Looking for a page to handle 404.");
        }

        int? domainContentId = null;

        // try to find a culture as best as we can
        var errorCulture = CultureInfo.CurrentUICulture.Name;
        if (frequest.Domain != null)
        {
            errorCulture = frequest.Domain.Culture;
            domainContentId = frequest.Domain.ContentId;
        }
        else
        {
            var route = frequest.AbsolutePathDecoded;
            var pos = route.LastIndexOf('/');
            IPublishedContent? node = null;
            while (pos > 1)
            {
                route = route.Substring(0, pos);
                node = umbracoContext.Content?.GetByRoute(route, culture: frequest?.Culture);
                if (node != null)
                {
                    break;
                }

                pos = route.LastIndexOf('/');
            }

            if (node != null)
            {
                Domain? d = DomainUtilities.FindWildcardDomainInPath(
                    umbracoContext.PublishedSnapshot.Domains?.GetAll(true), node.Path, null);
                if (d != null)
                {
                    errorCulture = d.Culture;
                }
            }
        }

        var error404 = NotFoundHandlerHelper.GetCurrentNotFoundPageId(
            _contentSettings.Error404Collection.ToArray(),
            _entityService,
            new PublishedContentQuery(umbracoContext.PublishedSnapshot, _variationContextAccessor, _examineManager),
            errorCulture,
            domainContentId);

        IPublishedContent? content = null;

        if (error404.HasValue)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Got id={ErrorNodeId}.", error404.Value);
            }

            content = umbracoContext.Content?.GetById(error404.Value);
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(content == null
                    ? "Could not find content with that id."
                    : "Found corresponding content.");
            }
        }
        else
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Got nothing.");
            }
        }

        frequest?
            .SetPublishedContent(content)
            .SetIs404();

        return Task.FromResult(content != null);
    }
}
