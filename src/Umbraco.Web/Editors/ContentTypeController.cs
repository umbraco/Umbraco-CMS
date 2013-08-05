using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using AutoMapper;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using System.Linq;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// An API controller used for dealing with content types
    /// </summary>
    [PluginController("UmbracoApi")]
    public class ContentTypeController : UmbracoAuthorizedJsonController
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ContentTypeController()
            : this(UmbracoContext.Current)
        {            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        public ContentTypeController(UmbracoContext umbracoContext)
            : base(umbracoContext)
        {
        }

        /// <summary>
        /// Returns the allowed child content type objects for the content item id passed in
        /// </summary>
        /// <param name="contentId"></param>
        public IEnumerable<ContentTypeBasic> GetAllowedChildren(int contentId)
        {
            if (contentId == Core.Constants.System.Root)
            {
                return Services.ContentTypeService.GetAllContentTypes()
                    .Where(x => x.AllowedAsRoot)
                    .Select(Mapper.Map<IContentType, ContentTypeBasic>);
            }

            var contentItem = Services.ContentService.GetById(contentId);
            if (contentItem == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }


            return contentItem.ContentType.AllowedContentTypes
                .Select(x => Services.ContentTypeService.GetContentType(x.Id.Value))
                .Select(Mapper.Map<IContentType, ContentTypeBasic>);
            
        }
    }
}