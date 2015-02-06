using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using AutoMapper;
using Newtonsoft.Json;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Editors
{

    //TODO:  We'll need to be careful about the security on this controller, when we start implementing 
    // methods to modify content types we'll need to enforce security on the individual methods, we
    // cannot put security on the whole controller because things like GetAllowedChildren are required for content editing.

    /// <summary>
    /// An API controller used for dealing with media types
    /// </summary>
    [PluginController("UmbracoApi")]
    public class MediaTypeController : ContentTypeControllerBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MediaTypeController()
            : this(UmbracoContext.Current)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        public MediaTypeController(UmbracoContext umbracoContext)
            : base(umbracoContext)
        {
        }

        /// <summary>
        /// Returns the allowed child content type objects for the content item id passed in
        /// </summary>
        /// <param name="contentId"></param>
        public IEnumerable<ContentTypeBasic> GetAllowedChildren(int contentId)
        {
            if (contentId == Core.Constants.System.RecycleBinMedia)
                return Enumerable.Empty<ContentTypeBasic>();

            if (contentId == Core.Constants.System.Root)
            {
                return Services.ContentTypeService.GetAllMediaTypes()
                    .Where(x => x.AllowedAsRoot)
                    .Select(Mapper.Map<IMediaType, ContentTypeBasic>);
            }

            var contentItem = Services.MediaService.GetById(contentId);
            if (contentItem == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var ids = contentItem.ContentType.AllowedContentTypes.Select(x => x.Id.Value).ToArray();
            if (ids.Any() == false) return Enumerable.Empty<ContentTypeBasic>();

            return Services.ContentTypeService.GetAllMediaTypes(ids)
                .Select(Mapper.Map<IMediaType, ContentTypeBasic>);
        }
    }
}