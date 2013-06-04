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
    public class ContentTypeApiController : UmbracoAuthorizedApiController
    {
        private readonly ContentTypeModelMapper _contentTypeModelMapper;

        /// <summary>
        /// Constructor
        /// </summary>
        public ContentTypeApiController()
            : this(UmbracoContext.Current, new ContentTypeModelMapper(UmbracoContext.Current.Application))
        {            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="contentModelMapper"></param>
        internal ContentTypeApiController(UmbracoContext umbracoContext, ContentTypeModelMapper contentModelMapper)
            : base(umbracoContext)
        {
            _contentTypeModelMapper = contentModelMapper;
        }

        /// <summary>
        /// Returns the allowed child content type objects for the content item id passed in
        /// </summary>
        /// <param name="contentId"></param>
        public IEnumerable<object> GetAllowedChildrenForContent(int contentId)
        {
            var contentItem = Services.ContentService.GetById(contentId);
            if (contentItem == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return Services.ContentTypeService.GetContentTypeChildren(contentItem.ContentTypeId)
                           .Select(x => _contentTypeModelMapper.ToContentItemBasic(x));
        }
    }
}