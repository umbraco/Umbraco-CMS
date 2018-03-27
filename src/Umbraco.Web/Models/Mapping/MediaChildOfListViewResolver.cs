using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class MediaChildOfListViewResolver : IValueResolver<IMedia, MediaItemDisplay, bool>
    {
        private readonly IMediaService _mediaService;
        private readonly IMediaTypeService _mediaTypeService;

        public MediaChildOfListViewResolver(IMediaService mediaService, IMediaTypeService mediaTypeService)
        {
            _mediaService = mediaService;
            _mediaTypeService = mediaTypeService;
        }

        public bool Resolve(IMedia source, MediaItemDisplay destination, bool destMember, ResolutionContext context)
        {
            // map the IsChildOfListView (this is actually if it is a descendant of a list view!)
            var parent = _mediaService.GetParent(source);
            return parent != null && (parent.ContentType.IsContainer || _mediaTypeService.HasContainerInPath(parent.Path));
        }
    }
}
