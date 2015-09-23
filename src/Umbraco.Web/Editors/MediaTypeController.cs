using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;
using System.Web.Http;
using System.Net;
using Umbraco.Core.PropertyEditors;
using System;
using System.Net.Http;
using ContentType = System.Net.Mime.ContentType;

namespace Umbraco.Web.Editors
{
    //TODO:  We'll need to be careful about the security on this controller, when we start implementing 
    // methods to modify content types we'll need to enforce security on the individual methods, we
    // cannot put security on the whole controller because things like GetAllowedChildren are required for content editing.

    /// <summary>
    /// An API controller used for dealing with content types
    /// </summary>
    [PluginController("UmbracoApi")]
    [UmbracoTreeAuthorize(Constants.Trees.MediaTypes)]
    [EnableOverrideAuthorization]
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


        public ContentTypeCompositionDisplay GetById(int id)
        {
            var ct = Services.ContentTypeService.GetMediaType(id);
            if (ct == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var dto = Mapper.Map<IMediaType, ContentTypeCompositionDisplay>(ct);
            return dto;
        }

        /// <summary>
        /// Deletes a document type wth a given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [HttpPost]
        public HttpResponseMessage DeleteById(int id)
        {
            var foundType = Services.ContentTypeService.GetMediaType(id);
            if (foundType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            Services.ContentTypeService.Delete(foundType, Security.CurrentUser.Id);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        public ContentTypeCompositionDisplay GetEmpty()
        {
            var ct = new MediaType(-1);
            ct.Icon = "icon-picture";

            var dto = Mapper.Map<IMediaType, ContentTypeCompositionDisplay>(ct);
            return dto;
        }


        /// <summary>
        /// Returns all member types
        /// </summary>
        public IEnumerable<ContentTypeBasic> GetAll()
        {   
            
            return Services.ContentTypeService.GetAllMediaTypes()
                               .Select(Mapper.Map<IMediaType, ContentTypeBasic>);
        }

        public ContentTypeCompositionDisplay PostSave(ContentTypeCompositionDisplay contentType)
        {

            var ctService = ApplicationContext.Services.ContentTypeService;

            //TODO: warn on content type alias conflicts
            //TODO: warn on property alias conflicts

            //TODO: Validate the submitted model

            //filter out empty properties
            contentType.Groups = contentType.Groups.Where(x => x.Name.IsNullOrWhiteSpace() == false).ToList();
            foreach (var group in contentType.Groups)
            {
                group.Properties = group.Properties.Where(x => x.Alias.IsNullOrWhiteSpace() == false).ToList();
            }

            var ctId = Convert.ToInt32(contentType.Id);

            if (ctId > 0)
            {
                //its an update to an existing
                IMediaType found = ctService.GetMediaType(ctId);
                if (found == null)
                    throw new HttpResponseException(HttpStatusCode.NotFound);

                Mapper.Map(contentType, found);
                ctService.Save(found);

                //map the saved item back to the content type (it should now get id etc set)
                Mapper.Map(found, contentType);
                return contentType;
            }
            else
            {
                //ensure alias is set
                if (string.IsNullOrEmpty(contentType.Alias))
                    contentType.Alias = contentType.Name.ToSafeAlias();

                contentType.Id = null;

                //save as new
                IMediaType newCt = new MediaType(-1);
                Mapper.Map(contentType, newCt);

                ctService.Save(newCt);

                //map the saved item back to the content type (it should now get id etc set)
                Mapper.Map(newCt, contentType);
                return contentType;
            }

        }


        /// <summary>
        /// Returns the allowed child content type objects for the content item id passed in
        /// </summary>
        /// <param name="contentId"></param>
        [UmbracoTreeAuthorize(Constants.Trees.MediaTypes, Constants.Trees.Media)]
        public IEnumerable<ContentTypeBasic> GetAllowedChildren(int contentId)
        {
            if (contentId == Constants.System.RecycleBinContent)
                return Enumerable.Empty<ContentTypeBasic>();

            IEnumerable<IMediaType> types;
            if (contentId == Constants.System.Root)
            {
                types = Services.ContentTypeService.GetAllMediaTypes().ToList();

                //if no allowed root types are set, just return everything
                if (types.Any(x => x.AllowedAsRoot))
                    types = types.Where(x => x.AllowedAsRoot);
            }
            else
            {
                var contentItem = Services.ContentService.GetById(contentId);
                if (contentItem == null)
                {
                    return Enumerable.Empty<ContentTypeBasic>();
                }

                var ids = contentItem.ContentType.AllowedContentTypes.Select(x => x.Id.Value).ToArray();

                if (ids.Any() == false) return Enumerable.Empty<ContentTypeBasic>();

                types = Services.ContentTypeService.GetAllMediaTypes(ids).ToList();
            }

            var basics = types.Select(Mapper.Map<IMediaType, ContentTypeBasic>).ToList();

            foreach (var basic in basics)
            {
                basic.Name = TranslateItem(basic.Name);
                basic.Description = TranslateItem(basic.Description);
            }

            return basics;
        }
    }
}