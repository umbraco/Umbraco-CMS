using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.WebApi;
using System.Linq;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// An API controller used for dealing with content types
    /// </summary>
    public class ContentTypeController : UmbracoAuthorizedApiController
    {
        private readonly ContentTypeModelMapper _contentTypeModelMapper;

        /// <summary>
        /// Constructor
        /// </summary>
        public ContentTypeController()
            : this(UmbracoContext.Current, new ContentTypeModelMapper(UmbracoContext.Current.Application))
        {            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="contentModelMapper"></param>
        internal ContentTypeController(UmbracoContext umbracoContext, ContentTypeModelMapper contentModelMapper)
            : base(umbracoContext)
        {
            _contentTypeModelMapper = contentModelMapper;
        }

        /// <summary>
        /// Returns the allowed child content type objects for the content item id passed in
        /// </summary>
        /// <param name="contentId"></param>
        public IEnumerable<object> GetAllowedChildren(int contentId)
        {
            var contentItem = Services.ContentService.GetById(contentId);
            if (contentItem == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }


            return contentItem.ContentType.AllowedContentTypes
                .Select(x => Services.ContentTypeService.GetContentType(x.Id.Value))
                .Select(x => _contentTypeModelMapper.ToContentTypeBasic(x));
            
        }
    }

    /// <summary>
    /// An API controller used for dealing with content types
    /// </summary>
    public class MediaTypeApiController : UmbracoAuthorizedApiController
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
        public IEnumerable<object> GetAllowedChildren(int contentId)
        {
            var contentItem = Services.MediaService.GetById(contentId);
            if (contentItem == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }


            return contentItem.ContentType.AllowedContentTypes
                .Select(x => Services.ContentTypeService.GetMediaType(x.Id.Value))
                .Select(x => _mediaTypeModelMapper.ToMediaTypeBasic(x));

        }
    }
}