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
[Obsolete("Use ContentFinderByKeyPath instead. This will be removed in Umbraco 15.")]
public class ContentFinderByIdPath : ContentFinderByIdentifierPathBase, IContentFinder
{
    private readonly ILogger<ContentFinderByIdPath> _logger;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private WebRoutingSettings _webRoutingSettings;

    protected override string FailureLogMessageTemplate => "Not a node id";

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentFinderByIdPath" /> class.
    /// </summary>
    public ContentFinderByIdPath(
        IOptionsMonitor<WebRoutingSettings> webRoutingSettings,
        ILogger<ContentFinderByIdPath> logger,
        IRequestAccessor requestAccessor,
        IUmbracoContextAccessor umbracoContextAccessor)
        : base(requestAccessor, logger)
    {
        _webRoutingSettings = webRoutingSettings.CurrentValue ??
                              throw new ArgumentNullException(nameof(webRoutingSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

        if (umbracoContext.InPreviewMode == false && _webRoutingSettings.DisableFindContentByIdentifierPath)
        {
            return Task.FromResult(false);
        }

        var path = frequest.AbsolutePathDecoded;

        // no id if "/"
        if (path == "/")
        {
            return LogAndReturnFailure();
        }

        var noSlashPath = path.Substring(1);

        if (int.TryParse(noSlashPath, NumberStyles.Integer, CultureInfo.InvariantCulture, out var nodeId) == false)
        {
            return LogAndReturnFailure();
        }

        // NodeId cannot be negative or 0
        if (nodeId < 1)
        {
            return LogAndReturnFailure();
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Id={NodeId}", nodeId);
        }

        IPublishedContent? node = umbracoContext.Content?.GetById(nodeId);

        if (node is null)
        {
            return LogAndReturnFailure();
        }

        ResolveAndSetCultureOnRequest(frequest);
        ResolveAndSetSegmentOnRequest(frequest);

        frequest.SetPublishedContent(node);
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Found node with id={PublishedContentId}", node.Id);
        }

        return Task.FromResult(true);
    }
}
