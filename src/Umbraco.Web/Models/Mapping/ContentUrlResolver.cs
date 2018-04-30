using System.Linq;
using AutoMapper;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Models.Mapping
{
    internal class ContentUrlResolver : IValueResolver<IContent, ContentItemDisplay, string[]>
    {
        private readonly ILocalizedTextService _textService;
        private readonly IContentService _contentService;
        private readonly ILogger _logger;

        public ContentUrlResolver(ILocalizedTextService textService, IContentService contentService, ILogger logger)
        {
            _textService = textService;
            _contentService = contentService;
            _logger = logger;
        }

        public string[] Resolve(IContent source, ContentItemDisplay destination, string[] destMember, ResolutionContext context)
        {
            var umbracoContext = context.GetUmbracoContext(throwIfMissing: false);

            var urls = umbracoContext == null
                ? new[] {"Cannot generate urls without a current Umbraco Context"}
                : source.GetContentUrls(umbracoContext.UrlProvider, _textService, _contentService, _logger).ToArray();

            return urls;
        }
    }
}
