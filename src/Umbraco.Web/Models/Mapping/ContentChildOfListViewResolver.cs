using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class ContentChildOfListViewResolver : IValueResolver<IContent, ContentItemDisplay, bool>
    {
        private readonly IContentService _contentService;
        private readonly IContentTypeService _contentTypeService;

        public ContentChildOfListViewResolver(IContentService contentService, IContentTypeService contentTypeService)
        {
            _contentService = contentService;
            _contentTypeService = contentTypeService;
        }

        public bool Resolve(IContent source, ContentItemDisplay destination, bool destMember, ResolutionContext context)
        {
            // map the IsChildOfListView (this is actually if it is a descendant of a list view!)
            var parent = _contentService.GetParent(source);
            return parent != null && (parent.ContentType.IsContainer || _contentTypeService.HasContainerInPath(parent.Path));
        }
    }
}