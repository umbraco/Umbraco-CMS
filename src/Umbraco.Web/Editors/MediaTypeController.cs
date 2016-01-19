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
using System.Text;
using Umbraco.Web.WebApi;
using ContentType = System.Net.Mime.ContentType;
using Umbraco.Core.Services;

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

        public int GetCount()
        {
            return Services.ContentTypeService.CountContentTypes();
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

        /// <summary>
        /// Returns the avilable compositions for this content type
        /// </summary>
        /// <param name="contentTypeId"></param>
        /// <param name="filterContentTypes">
        /// This is normally an empty list but if additional content type aliases are passed in, any content types containing those aliases will be filtered out
        /// along with any content types that have matching property types that are included in the filtered content types
        /// </param>
        /// <param name="filterPropertyTypes">
        /// This is normally an empty list but if additional property type aliases are passed in, any content types that have these aliases will be filtered out.
        /// This is required because in the case of creating/modifying a content type because new property types being added to it are not yet persisted so cannot
        /// be looked up via the db, they need to be passed in.
        /// </param>
        /// <returns></returns>
        public HttpResponseMessage GetAvailableCompositeMediaTypes(int contentTypeId,
            [FromUri]string[] filterContentTypes,
            [FromUri]string[] filterPropertyTypes)
        {
            var result = PerformGetAvailableCompositeContentTypes(contentTypeId, UmbracoObjectTypes.MediaType, filterContentTypes, filterPropertyTypes)
                .Select(x => new
                {
                    contentType = x.Item1,
                    allowed = x.Item2
                });
            return Request.CreateResponse(result);            
        }

        public ContentTypeCompositionDisplay GetEmpty(int parentId)
        {
            var ct = new MediaType(parentId);
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

        /// <summary>
        /// Deletes a document type container wth a given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [HttpPost]
        public HttpResponseMessage DeleteContainer(int id)
        {
            Services.ContentTypeService.DeleteMediaTypeContainer(id, Security.CurrentUser.Id);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        public HttpResponseMessage PostCreateContainer(int parentId, string name)
        {
            var result = Services.ContentTypeService.CreateMediaTypeContainer(parentId, name, Security.CurrentUser.Id);

            return result
                ? Request.CreateResponse(HttpStatusCode.OK, result.Result) //return the id 
                : Request.CreateNotificationValidationErrorResponse(result.Exception.Message);
        }

        public ContentTypeCompositionDisplay PostSave(ContentTypeSave contentTypeSave)
        {
            var savedCt = PerformPostSave<IMediaType, ContentTypeCompositionDisplay>(
                contentTypeSave:        contentTypeSave,
                getContentType:         i => Services.ContentTypeService.GetMediaType(i),
                getContentTypeByAlias:  alias => Services.ContentTypeService.GetMediaType(alias),
                saveContentType:        type => Services.ContentTypeService.Save(type));

            var display = Mapper.Map<ContentTypeCompositionDisplay>(savedCt);

            display.AddSuccessNotification(
                            Services.TextService.Localize("speechBubbles/contentTypeSavedHeader"),
                            string.Empty);

            return display;
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
                var contentItem = Services.MediaService.GetById(contentId);
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

        /// <summary>
        /// Move the media type
        /// </summary>
        /// <param name="move"></param>
        /// <returns></returns>
        public HttpResponseMessage PostMove(MoveOrCopy move)
        {
            return PerformMove(
                move, 
                getContentType: i => Services.ContentTypeService.GetMediaType(i), 
                doMove:         (type, i) => Services.ContentTypeService.MoveMediaType(type, i));            
        }
        
    }
}