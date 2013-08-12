using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using System.Linq;
using Umbraco.Web.WebApi.Binders;
using Umbraco.Web.WebApi.Filters;
using umbraco;
using umbraco.BusinessLogic.Actions;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Editors
{

    /// <remarks>
    /// This controller is decorated with the UmbracoApplicationAuthorizeAttribute which means that any user requesting
    /// access to ALL of the methods on this controller will need access to the media application.
    /// </remarks>
    [PluginController("UmbracoApi")]
    [UmbracoApplicationAuthorizeAttribute(Constants.Applications.Media)]
    public class MediaController : ContentControllerBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MediaController()
            : this(UmbracoContext.Current)
        {            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        public MediaController(UmbracoContext umbracoContext)
            : base(umbracoContext)
        {
        }

        /// <summary>
        /// Gets an empty content item for the 
        /// </summary>
        /// <param name="contentTypeAlias"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public MediaItemDisplay GetEmpty(string contentTypeAlias, int parentId)
        {
            var contentType = Services.ContentTypeService.GetMediaType(contentTypeAlias);
            if (contentType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var emptyContent = new Core.Models.Media("Empty", parentId, contentType);
            return Mapper.Map<IMedia, MediaItemDisplay>(emptyContent);
        }

        /// <summary>
        /// Gets the content json for the content id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [EnsureUserPermissionForMedia("id")]
        public MediaItemDisplay GetById(int id)
        {
            var foundContent = Services.MediaService.GetById(id);
            if (foundContent == null)
            {
                HandleContentNotFound(id);
            }
            return Mapper.Map<IMedia, MediaItemDisplay>(foundContent);
        }

        /// <summary>
        /// Returns the root media objects
        /// </summary>
        public IEnumerable<ContentItemBasic<ContentPropertyBasic, IMedia>> GetRootMedia()
        {
            //TODO: Add permissions check!

            return Services.MediaService.GetRootMedia()
                           .Select(Mapper.Map<IMedia, ContentItemBasic<ContentPropertyBasic, IMedia>>);
        }

        /// <summary>
        /// Returns the child media objects
        /// </summary>
        public IEnumerable<ContentItemBasic<ContentPropertyBasic, IMedia>> GetChildren(int parentId)
        {
            //TODO: Change this to be like content with paged params
            //TODO: filter results based on permissions!

            return Services.MediaService.GetChildren(parentId)
                           .Select(Mapper.Map<IMedia, ContentItemBasic<ContentPropertyBasic, IMedia>>);
        }

        /// <summary>
        /// Saves content
        /// </summary>
        /// <returns></returns>        
        [FileUploadCleanupFilter]
        public MediaItemDisplay PostSave(
            [ModelBinder(typeof(MediaItemBinder))]
                ContentItemSave<IMedia> contentItem)
        {
            //We now need to validate that the user is allowed to be doing what they are doing
            if (CheckPermissions(
                Request.Properties,
                Security.CurrentUser,
                Services.MediaService,
                contentItem.Id,
                contentItem.PersistedContent) == false)
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            //If we've reached here it means:
            // * Our model has been bound
            // * and validated
            // * any file attachments have been saved to their temporary location for us to use
            // * we have a reference to the DTO object and the persisted object

            UpdateName(contentItem);

            MapPropertyValues(contentItem);

            //We need to manually check the validation results here because:
            // * We still need to save the entity even if there are validation value errors
            // * Depending on if the entity is new, and if there are non property validation errors (i.e. the name is null)
            //      then we cannot continue saving, we can only display errors
            // * If there are validation errors and they were attempting to publish, we can only save, NOT publish and display 
            //      a message indicating this
            if (!ModelState.IsValid)
            {
                if (ValidationHelper.ModelHasRequiredForPersistenceErrors(contentItem)
                    && (contentItem.Action == ContentSaveAction.SaveNew))
                {
                    //ok, so the absolute mandatory data is invalid and it's new, we cannot actually continue!
                    // add the modelstate to the outgoing object and throw a 403
                    var forDisplay = Mapper.Map<IMedia, MediaItemDisplay>(contentItem.PersistedContent);
                    forDisplay.Errors = ModelState.ToErrorDictionary();
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Forbidden, forDisplay));
                }
            }

            //save the item
            Services.MediaService.Save(contentItem.PersistedContent, (int)Security.CurrentUser.Id);

            //return the updated model
            var display = Mapper.Map<IMedia, MediaItemDisplay>(contentItem.PersistedContent);
            
            //lasty, if it is not valid, add the modelstate to the outgoing object and throw a 403
            HandleInvalidModelState(display);

            //put the correct msgs in 
            switch (contentItem.Action)
            {
                case ContentSaveAction.Save:
                case ContentSaveAction.SaveNew:
                    display.AddSuccessNotification(ui.Text("speechBubbles", "editMediaSaved"), ui.Text("speechBubbles", "editMediaSavedText"));
                    break;                
            }

            return display;
        }

        /// <summary>
        /// Change the sort order for media
        /// </summary>
        /// <param name="sorted"></param>
        /// <returns></returns>
        [EnsureUserPermissionForContent("sorted.ParentId", 'S')]
        public HttpResponseMessage PostSort(ContentSortOrder sorted)
        {
            if (sorted == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            //if there's nothing to sort just return ok
            if (sorted.IdSortOrder.Length == 0)
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            
            var mediaService = base.ApplicationContext.Services.MediaService;
            var sortedMedia = new List<IMedia>();
            try
            {
                sortedMedia.AddRange(sorted.IdSortOrder.Select(mediaService.GetById));

                // Save Media with new sort order and update content xml in db accordingly
                if (mediaService.Sort(sortedMedia) == false)
                {
                    LogHelper.Warn<MediaController>("Media sorting failed, this was probably caused by an event being cancelled");
                    return Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Media sorting failed, this was probably caused by an event being cancelled");
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MediaController>("Could not update media sort order", ex);
                throw;
            }
        }

        /// <summary>
        /// Performs a permissions check for the user to check if it has access to the node based on 
        /// start node and/or permissions for the node
        /// </summary>
        /// <param name="storage">The storage to add the content item to so it can be reused</param>
        /// <param name="user"></param>
        /// <param name="mediaService"></param>
        /// <param name="nodeId">The content to lookup, if the contentItem is not specified</param>
        /// <param name="media">Specifies the already resolved content item to check against, setting this ignores the nodeId</param>
        /// <returns></returns>
        internal static bool CheckPermissions(IDictionary<string, object> storage, IUser user, IMediaService mediaService, int nodeId, IMedia media = null)
        {
            var contentItem = mediaService.GetById(nodeId);
            if (contentItem == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            //put the content item into storage so it can be retreived 
            // in the controller (saves a lookup)
            storage.Add(typeof(IMedia).ToString(), contentItem);

            var hasPathAccess = user.HasPathAccess(contentItem);

            return hasPathAccess;
        }
    }
}
