using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Provides an implementation of <see cref="IContentFinder" /> that handles page key identifiers.
/// </summary>
/// <remarks>
///     <para>Handles <c>/e7b65017-c6b3-4c11-b7c7-7ea1d0404c9a</c> where <c>e7b65017-c6b3-4c11-b7c7-7ea1d0404c9a</c> is the key of a document.</para>
/// </remarks>
public class ContentFinderByKeyPath : ContentFinderByIdentifierPathBase, IContentFinder
{
    private readonly ILogger<ContentFinderByKeyPath> _logger;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private WebRoutingSettings _webRoutingSettings;

    protected override string FailureLogMessageTemplate => "Not a node key";

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentFinderByKeyPath" /> class.
    /// </summary>
    public ContentFinderByKeyPath(
        IOptionsMonitor<WebRoutingSettings> webRoutingSettings,
        ILogger<ContentFinderByKeyPath> logger,
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

        if (Guid.TryParse(noSlashPath, out var nodeKey) == false)
        {
            return LogAndReturnFailure();
        }

        // We shouldn't be persisting empty Guids
        if (nodeKey.Equals(Guid.Empty))
        {
            return LogAndReturnFailure();
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Key={NodeKey}", nodeKey);
        }

        IPublishedContent? node = umbracoContext.Content?.GetById(nodeKey);

        if (node is null)
        {
            return LogAndReturnFailure();
        }

        ResolveAndSetCultureOnRequest(frequest);
        ResolveAndSetSegmentOnRequest(frequest);

        frequest.SetPublishedContent(node);
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Found node with key={PublishedContentKey}", node.Key);
        }

        return Task.FromResult(true);
    }
}
