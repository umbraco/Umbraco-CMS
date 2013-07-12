using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// An API controller used for dealing with content types
    /// </summary>
    public class MediaTypeApiController : UmbracoAuthorizedJsonController
    {
        private readonly MediaTypeModelMapper _mediaTypeModelMapper;

        /// <summary>
        /// Constructor
        /// </summary>
        public MediaTypeApiController()
            : this(UmbracoContext.Current, new MediaTypeModelMapper(UmbracoContext.Current.Application))
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="mediaModelMapper"></param>
        internal MediaTypeApiController(UmbracoContext umbracoContext, MediaTypeModelMapper mediaModelMapper)
            : base(umbracoContext)
        {
            _mediaTypeModelMapper = mediaModelMapper;
        }

        /// <summary>
        /// Returns the allowed child content type objects for the content item id passed in
        /// </summary>
        /// <param name="contentId"></param>
        public IEnumerable<ContentTypeBasic> GetAllowedChildren(int contentId)
        {
            var contentItem = Services.MediaService.GetById(contentId);
            if (contentItem == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }


            return contentItem.ContentType.AllowedContentTypes
                              .Select(x => Services.ContentTypeService.GetMediaType((int) x.Id.Value))
                              .Select(x => _mediaTypeModelMapper.ToMediaTypeBasic(x));

        }
    }
}