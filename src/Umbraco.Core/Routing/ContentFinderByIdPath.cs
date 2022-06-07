using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Provides an implementation of <see cref="IContentFinder" /> that handles page identifiers.
/// </summary>
/// <remarks>
///     <para>Handles <c>/1234</c> where <c>1234</c> is the identified of a document.</para>
/// </remarks>
public class ContentFinderByIdPath : IContentFinder
{
    private readonly ILogger<ContentFinderByIdPath> _logger;
    private readonly IRequestAccessor _requestAccessor;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private WebRoutingSettings _webRoutingSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentFinderByIdPath" /> class.
    /// </summary>
    public ContentFinderByIdPath(
        IOptionsMonitor<WebRoutingSettings> webRoutingSettings,
        ILogger<ContentFinderByIdPath> logger,
        IRequestAccessor requestAccessor,
        IUmbracoContextAccessor umbracoContextAccessor)
    {
        _webRoutingSettings = webRoutingSettings.CurrentValue ??
                              throw new ArgumentNullException(nameof(webRoutingSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _requestAccessor = requestAccessor ?? throw new ArgumentNullException(nameof(requestAccessor));
        _umbracoContextAccessor =
            umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));

        webRoutingSettings.OnChange(x => _webRoutingSettings = x);
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

        if (umbracoContext.InPreviewMode == false && _webRoutingSettings.DisableFindContentByIdPath)
        {
            return Task.FromResult(false);
        }

        IPublishedContent? node = null;
        var path = frequest.AbsolutePathDecoded;

        var nodeId = -1;

        // no id if "/"
        if (path != "/")
        {
            var noSlashPath = path.Substring(1);

            if (int.TryParse(noSlashPath, NumberStyles.Integer, CultureInfo.InvariantCulture, out nodeId) == false)
            {
                nodeId = -1;
            }

            if (nodeId > 0)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Id={NodeId}", nodeId);
                }

                node = umbracoContext.Content?.GetById(nodeId);

                if (node != null)
                {
                    var cultureFromQuerystring = _requestAccessor.GetQueryStringValue("culture");

                    // if we have a node, check if we have a culture in the query string
                    if (!string.IsNullOrEmpty(cultureFromQuerystring))
                    {
                        // we're assuming it will match a culture, if an invalid one is passed in, an exception will throw (there is no TryGetCultureInfo method), i think this is ok though
                        frequest.SetCulture(cultureFromQuerystring);
                    }

                    frequest.SetPublishedContent(node);
                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug("Found node with id={PublishedContentId}", node.Id);
                    }
                }
                else
                {
                    nodeId = -1; // trigger message below
                }
            }
        }

        if (nodeId == -1)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Not a node id");
            }
        }

        return Task.FromResult(node != null);
    }
}
