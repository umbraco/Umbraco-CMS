using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Routing
{
    /// <summary>
    /// Provides an implementation of <see cref="IContentFinder"/> that handles page nice URLs and a template.
    /// </summary>
    /// <remarks>
    /// <para>This finder allows for an odd routing pattern similar to altTemplate, probably only use case is if there is an alternative mime type template and it should be routable by something like "/hello/world/json" where the JSON template is to be used for the "world" page</para>
    /// <para>Handles <c>/foo/bar/template</c> where <c>/foo/bar</c> is the nice URL of a document, and <c>template</c> a template alias.</para>
    /// <para>If successful, then the template of the document request is also assigned.</para>
    /// </remarks>
    public partial class ContentFinderByUrlAndTemplate : ContentFinderByUrl
    {
        private readonly ILogger<ContentFinderByUrlAndTemplate> _logger;
        private readonly IFileService _fileService;

        private readonly IContentTypeService _contentTypeService;
        private WebRoutingSettings _webRoutingSettings;

        private static readonly Action<ILogger, string, Exception> s_logNotValidTemplate
            = LoggerMessage.Define<string>(LogLevel.Debug, new EventId(18), "Not a valid template: '{TemplateAlias}'");

        private static readonly Action<ILogger, string, Exception> s_logValidTemplate
            = LoggerMessage.Define<string>(LogLevel.Debug, new EventId(19), "Valid template: '{TemplateAlias}'");

        private static readonly Action<ILogger, string, Exception> s_logNotValidRouteToNode
            = LoggerMessage.Define<string>(LogLevel.Debug, new EventId(20), "Not a valid route to node: '{Route}'");

        private static readonly Action<ILogger, string, int, Exception> s_logNotAllowedTemplate
            = LoggerMessage.Define<string, int>(LogLevel.Warning, new EventId(21), "Alternative template '{TemplateAlias}' is not allowed on node {NodeId}.");

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentFinderByUrlAndTemplate"/> class.
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
        /// Tries to find and assign an Umbraco document to a <c>PublishedRequest</c>.
        /// </summary>
        /// <param name="frequest">The <c>PublishedRequest</c>.</param>
        /// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
        /// <remarks>If successful, also assigns the template.</remarks>
        public override bool TryFindContent(IPublishedRequestBuilder frequest)
        {
            var path = frequest.AbsolutePathDecoded;

            if (frequest.Domain != null)
            {
                path = DomainUtilities.PathRelativeToDomain(frequest.Domain.Uri, path);
            }

            // no template if "/"
            if (path == "/")
            {
                _logger.LogDebug("No template in path '/'");
                return false;
            }

            // look for template in last position
            var pos = path.LastIndexOf('/');
            var templateAlias = path.Substring(pos + 1);
            path = pos == 0 ? "/" : path.Substring(0, pos);

            ITemplate? template = _fileService.GetTemplate(templateAlias);

            if (template == null)
            {
                LogNotValidTemplate(templateAlias);
                return false;
            }

            LogValidTemplate(templateAlias);

            // look for node corresponding to the rest of the route
            var route = frequest.Domain != null ? (frequest.Domain.ContentId + path) : path;
            IPublishedContent? node = FindContent(frequest, route);

            if (node == null)
            {
                LogNotValidRouteToNode(route);
                return false;
            }

            // IsAllowedTemplate deals both with DisableAlternativeTemplates and ValidateAlternativeTemplates settings
            if (!node.IsAllowedTemplate(_contentTypeService, _webRoutingSettings, template.Id))
            {
                LogNotAllowedTemplate(template.Alias, node.Id);
                frequest.SetPublishedContent(null); // clear
                return false;
            }

            // got it
            frequest.SetTemplate(template);
            return true;
        }

        private void LogNotValidTemplate(string templateAlias)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                s_logNotValidTemplate.Invoke(_logger, templateAlias, null);
            }
        }

        private void LogValidTemplate(string templateAlias)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                s_logValidTemplate.Invoke(_logger, templateAlias, null);
            }
        }

        private void LogNotValidRouteToNode(string route)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                s_logNotValidRouteToNode.Invoke(_logger, route, null);
            }
        }

        private void LogNotAllowedTemplate(string templateAlias, int nodeId)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                s_logNotAllowedTemplate.Invoke(_logger, templateAlias, nodeId, null);
            }
        }
    }
}
