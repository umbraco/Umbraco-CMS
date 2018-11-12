using AutoMapper;
using System.Web.Mvc;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class DefaultTemplateResolver : IValueResolver<IContent, ContentItemDisplay, string>
    {
        private readonly IContentTypeService _contentTypeService;

        public DefaultTemplateResolver(IContentTypeService contentTypeService)
        {
            _contentTypeService = contentTypeService;
        }

        public string Resolve(IContent source, ContentItemDisplay destination, string destMember, ResolutionContext context)
        {
            if (source == null)
                return null;

            var contentType = _contentTypeService.Get(source.ContentTypeId);

            // If no template id was set...
            if (!source.TemplateId.HasValue)
            {
                // ... and no default template is set, return null...
                if (string.IsNullOrWhiteSpace(contentType.DefaultTemplate?.Alias))
                    return null;

                // ... otherwise return the content type default template alias.
                return contentType.DefaultTemplate?.Alias;
            }

            var fileService = DependencyResolver.Current.GetService<IFileService>();
            var template = fileService.GetTemplate(source.TemplateId.Value);

            return template.Alias;
        }
    }
}
