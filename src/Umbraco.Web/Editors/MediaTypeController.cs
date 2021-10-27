using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;
using System.Web.Http;
using System.Net;
using System.Net.Http;
using Umbraco.Web.WebApi;
using Umbraco.Core.Services;
using System;
using System.Web.Http.Controllers;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Web.Composing;
using IMediaType = Umbraco.Core.Models.IMediaType;

namespace Umbraco.Web.Editors
{
    // TODO:  We'll need to be careful about the security on this controller, when we start implementing
    // methods to modify content types we'll need to enforce security on the individual methods, we
    // cannot put security on the whole controller because things like GetAllowedChildren are required for content editing.

    /// <summary>
    /// An API controller used for dealing with content types
    /// </summary>
    [PluginController("UmbracoApi")]
    [UmbracoTreeAuthorize(Constants.Trees.MediaTypes)]
    [EnableOverrideAuthorization]
    [MediaTypeControllerControllerConfiguration]
    public class MediaTypeController : ContentTypeControllerBase<IMediaType>
    {
        public MediaTypeController(ICultureDictionaryFactory cultureDictionaryFactory, IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState, UmbracoHelper umbracoHelper)
            : base(cultureDictionaryFactory, globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoHelper)
        {
        }

        /// <summary>
        /// Configures this controller with a custom action selector
        /// </summary>
        private class MediaTypeControllerControllerConfigurationAttribute : Attribute, IControllerConfiguration
        {
            public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
            {
                controllerSettings.Services.Replace(typeof(IHttpActionSelector), new ParameterSwapControllerActionSelector(
                    new ParameterSwapControllerActionSelector.ParameterSwapInfo("GetById", "id", typeof(int), typeof(Guid), typeof(Udi)),
                    new ParameterSwapControllerActionSelector.ParameterSwapInfo("GetAllowedChildren", "contentId", typeof(int), typeof(Guid), typeof(Udi))));
            }
        }

        public int GetCount()
        {
            return Services.ContentTypeService.Count();
        }

        /// <summary>
        /// Gets the media type a given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [UmbracoTreeAuthorize(Constants.Trees.MediaTypes, Constants.Trees.Media)]
        public MediaTypeDisplay GetById(int id)
        {
            var mediaType = Services.MediaTypeService.Get(id);
            if (mediaType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var dto = Mapper.Map<IMediaType, MediaTypeDisplay>(mediaType);
            return dto;
        }

        /// <summary>
        /// Gets the media type a given guid
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [UmbracoTreeAuthorize(Constants.Trees.MediaTypes, Constants.Trees.Media)]
        public MediaTypeDisplay GetById(Guid id)
        {
            var mediaType = Services.MediaTypeService.Get(id);
            if (mediaType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var dto = Mapper.Map<IMediaType, MediaTypeDisplay>(mediaType);
            return dto;
        }

        /// <summary>
        /// Gets the media type a given udi
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [UmbracoTreeAuthorize(Constants.Trees.MediaTypes, Constants.Trees.Media)]
        public MediaTypeDisplay GetById(Udi id)
        {
            var guidUdi = id as GuidUdi;
            if (guidUdi == null)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var mediaType = Services.MediaTypeService.Get(guidUdi.Guid);
            if (mediaType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var dto = Mapper.Map<IMediaType, MediaTypeDisplay>(mediaType);
            return dto;
        }

        /// <summary>
        /// Deletes a media type with a given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [HttpPost]
        public HttpResponseMessage DeleteById(int id)
        {
            var foundType = Services.MediaTypeService.Get(id);
            if (foundType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            Services.MediaTypeService.Delete(foundType, Security.CurrentUser.Id);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// Returns the available compositions for this content type
        /// This has been wrapped in a dto instead of simple parameters to support having multiple parameters in post request body
        /// </summary>
        /// <param name="filter.contentTypeId"></param>
        /// <param name="filter.ContentTypes">
        /// This is normally an empty list but if additional content type aliases are passed in, any content types containing those aliases will be filtered out
        /// along with any content types that have matching property types that are included in the filtered content types
        /// </param>
        /// <param name="filter.PropertyTypes">
        /// This is normally an empty list but if additional property type aliases are passed in, any content types that have these aliases will be filtered out.
        /// This is required because in the case of creating/modifying a content type because new property types being added to it are not yet persisted so cannot
        /// be looked up via the db, they need to be passed in.
        /// </param>
        /// <param name="filter">
        /// Filter applied when resolving compositions</param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage GetAvailableCompositeMediaTypes(GetAvailableCompositionsFilter filter)
        {
            var result = PerformGetAvailableCompositeContentTypes(filter.ContentTypeId, UmbracoObjectTypes.MediaType, filter.FilterContentTypes, filter.FilterPropertyTypes, filter.IsElement)
                .Select(x => new
                {
                    contentType = x.Item1,
                    allowed = x.Item2
                });
            return Request.CreateResponse(result);
        }
        /// <summary>
        /// Returns where a particular composition has been used
        /// This has been wrapped in a dto instead of simple parameters to support having multiple parameters in post request body
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage GetWhereCompositionIsUsedInContentTypes(GetAvailableCompositionsFilter filter)
        {
            var result = PerformGetWhereCompositionIsUsedInContentTypes(filter.ContentTypeId, UmbracoObjectTypes.MediaType)
                .Select(x => new
                {
                    contentType = x
                });
            return Request.CreateResponse(result);
        }
        public MediaTypeDisplay GetEmpty(int parentId)
        {
            IMediaType mt;
            if (parentId != Constants.System.Root)
            {
                var parent = Services.MediaTypeService.Get(parentId);
                mt = parent != null ? new MediaType(parent, string.Empty) : new MediaType(parentId);
            }
            else
                mt = new MediaType(parentId);

            mt.Icon = Constants.Icons.MediaImage;

            var dto = Mapper.Map<IMediaType, MediaTypeDisplay>(mt);
            return dto;
        }


        /// <summary>
        /// Returns all media types
        /// </summary>
        public IEnumerable<ContentTypeBasic> GetAll()
        {
            return Services.MediaTypeService.GetAll()
                               .Select(Mapper.Map<IMediaType, ContentTypeBasic>);
        }

        /// <summary>
        /// Deletes a media type container with a given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [HttpPost]
        public HttpResponseMessage DeleteContainer(int id)
        {
            Services.MediaTypeService.DeleteContainer(id, Security.CurrentUser.Id);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        public HttpResponseMessage PostCreateContainer(int parentId, string name)
        {
            var result = Services.MediaTypeService.CreateContainer(parentId, name, Security.CurrentUser.Id);

            return result
                ? Request.CreateResponse(HttpStatusCode.OK, result.Result) //return the id
                : Request.CreateNotificationValidationErrorResponse(result.Exception.Message);
        }

        public HttpResponseMessage PostRenameContainer(int id, string name)
        {

            var result = Services.MediaTypeService.RenameContainer(id, name, Security.CurrentUser.Id);

            return result
                ? Request.CreateResponse(HttpStatusCode.OK, result.Result) //return the id
                : Request.CreateNotificationValidationErrorResponse(result.Exception.Message);
        }

        public MediaTypeDisplay PostSave(MediaTypeSave contentTypeSave)
        {
            var savedCt = PerformPostSave<MediaTypeDisplay, MediaTypeSave, PropertyTypeBasic>(
                contentTypeSave:        contentTypeSave,
                getContentType:         i => Services.MediaTypeService.Get(i),
                saveContentType:        type => Services.MediaTypeService.Save(type));

            var display = Mapper.Map<MediaTypeDisplay>(savedCt);

            display.AddSuccessNotification(
                            Services.TextService.Localize("speechBubbles", "mediaTypeSavedHeader"),
                            string.Empty);

            return display;
        }


        #region GetAllowedChildren
        /// <summary>
        /// Returns the allowed child content type objects for the content item id passed in - based on an INT id
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
                types = Services.MediaTypeService.GetAll().ToList();

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

                var contentType = Services.MediaTypeService.Get(contentItem.ContentTypeId);
                var ids = contentType.AllowedContentTypes.OrderBy(c => c.SortOrder).Select(x => x.Id.Value).ToArray();

                if (ids.Any() == false) return Enumerable.Empty<ContentTypeBasic>();

                types = Services.MediaTypeService.GetAll(ids).OrderBy(c => ids.IndexOf(c.Id)).ToList();
            }

            var basics = types.Select(Mapper.Map<IMediaType, ContentTypeBasic>).ToList();

            foreach (var basic in basics)
            {
                basic.Name = TranslateItem(basic.Name);
                basic.Description = TranslateItem(basic.Description);
            }

            return basics.OrderBy(c => contentId == Constants.System.Root ? c.Name : string.Empty);
        }

        /// <summary>
        /// Returns the allowed child content type objects for the content item id passed in - based on a GUID id
        /// </summary>
        /// <param name="contentId"></param>
        [UmbracoTreeAuthorize(Constants.Trees.MediaTypes, Constants.Trees.Media)]
        public IEnumerable<ContentTypeBasic> GetAllowedChildren(Guid contentId)
        {
            var entity = Current.Services.EntityService.Get(contentId);
            if (entity != null)
            {
                return GetAllowedChildren(entity.Id);
            }

            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Returns the allowed child content type objects for the content item id passed in - based on a UDI id
        /// </summary>
        /// <param name="contentId"></param>
        [UmbracoTreeAuthorize(Constants.Trees.MediaTypes, Constants.Trees.Media)]
        public IEnumerable<ContentTypeBasic> GetAllowedChildren(Udi contentId)
        {
            var guidUdi = contentId as GuidUdi;
            if (guidUdi != null)
            {
                var entity = Current.Services.EntityService.Get(guidUdi.Guid);
                if (entity != null)
                {
                    return GetAllowedChildren(entity.Id);
                }
            }

            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        #endregion

        /// <summary>
        /// Move the media type
        /// </summary>
        /// <param name="move"></param>
        /// <returns></returns>
        public HttpResponseMessage PostMove(MoveOrCopy move)
        {
            return PerformMove(
                move,
                getContentType: i => Services.MediaTypeService.Get(i),
                doMove: (type, i) => Services.MediaTypeService.Move(type, i));
        }

        /// <summary>
        /// Copy the media type
        /// </summary>
        /// <param name="copy"></param>
        /// <returns></returns>
        public HttpResponseMessage PostCopy(MoveOrCopy copy)
        {
            return PerformCopy(
                copy,
                getContentType: i => Services.MediaTypeService.Get(i),
                doCopy: (type, i) => Services.MediaTypeService.Copy(type, i));
        }


    }
}
