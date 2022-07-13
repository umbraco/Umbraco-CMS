using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Provides an implementation of <see cref="IContentFinder" /> that handles page nice URLs and a template.
/// </summary>
/// <remarks>
///     <para>
///         This finder allows for an odd routing pattern similar to altTemplate, probably only use case is if there is
///         an alternative mime type template and it should be routable by something like "/hello/world/json" where the
///         JSON template is to be used for the "world" page
///     </para>
///     <para>
///         Handles <c>/foo/bar/template</c> where <c>/foo/bar</c> is the nice URL of a document, and <c>template</c> a
///         template alias.
///     </para>
///     <para>If successful, then the template of the document request is also assigned.</para>
/// </remarks>
public class ContentFinderByUrlAndTemplate : ContentFinderByUrl
{
    private readonly IContentTypeService _contentTypeService;
    private readonly IFileService _fileService;
    private readonly ILogger<ContentFinderByUrlAndTemplate> _logger;
    private WebRoutingSettings _webRoutingSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentFinderByUrlAndTemplate" /> class.
    /// </summary>
    public ContentFinderByUrlAndTemplate(
        ILogger<ContentFinderByUrlAndTemplate> logger,
        IFileService fileService,
        IContentTypeService contentTypeService,
        IUmbracoContextAccessor umbracoContextAccessor,
        IOptionsMonitor<WebRoutingSettings> webRoutingSettings)
        : base(logger, umbracoContextAccessor)
    {
        _logger = logger;
        _fileService = fileService;
        _contentTypeService = contentTypeService;
        _webRoutingSettings = webRoutingSettings.CurrentValue;
        webRoutingSettings.OnChange(x => _webRoutingSettings = x);
    }

    /// <summary>
    ///     Tries to find and assign an Umbraco document to a <c>PublishedRequest</c>.
    /// </summary>
    /// <param name="frequest">The <c>PublishedRequest</c>.</param>
    /// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
    /// <remarks>If successful, also assigns the template.</remarks>
    public override Task<bool> TryFindContent(IPublishedRequestBuilder frequest)
    {
        var path = frequest.AbsolutePathDecoded;

        if (frequest.Domain != null)
        {
            path = DomainUtilities.PathRelativeToDomain(frequest.Domain.Uri, path);
        }

        // no template if "/"
        if (path == "/")
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("No template in path '/'");
            }

            return Task.FromResult(false);
        }

        // look for template in last position
        var pos = path.LastIndexOf('/');
        var templateAlias = path.Substring(pos + 1);
        path = pos == 0 ? "/" : path.Substring(0, pos);;

        ITemplate? template = _fileService.GetTemplate(templateAlias);

        if (template == null)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Not a valid template: '{TemplateAlias}'", templateAlias);
            }

            return Task.FromResult(false);
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Valid template: '{TemplateAlias}'", templateAlias);
        }

        // look for node corresponding to the rest of the route
        var route = frequest.Domain != null ? frequest.Domain.ContentId + path : path;
        IPublishedContent? node = FindContent(frequest, route);

        if (node == null)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Not a valid route to node: '{Route}'", route);
            }

            return Task.FromResult(false);
        }

        // IsAllowedTemplate deals both with DisableAlternativeTemplates and ValidateAlternativeTemplates settings
        if (!node.IsAllowedTemplate(_contentTypeService, _webRoutingSettings, template.Id))
        {
            _logger.LogWarning(
                "Alternative template '{TemplateAlias}' is not allowed on node {NodeId}.", template.Alias, node.Id);
            frequest.SetPublishedContent(null); // clear
            return Task.FromResult(false);
        }

        // got it
        frequest.SetTemplate(template);
        return Task.FromResult(true);
    }
}
