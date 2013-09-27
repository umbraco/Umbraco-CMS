using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Dynamics;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Services;
using Umbraco.Web.Models;
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

            var emptyContent = new Core.Models.Media("", parentId, contentType);
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
        /// Return media for the specified ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [FilterAllowedOutgoingMedia(typeof(IEnumerable<MediaItemDisplay>))]
        public IEnumerable<MediaItemDisplay> GetByIds([FromUri]int[] ids)
        {
            var foundMedia = ((MediaService)Services.MediaService).GetByIds(ids);
            return foundMedia.Select(Mapper.Map<IMedia, MediaItemDisplay>);
        }


        /// <summary>
        /// Returns the root media objects
        /// </summary>
        [FilterAllowedOutgoingMedia(typeof(IEnumerable<ContentItemBasic<ContentPropertyBasic, IMedia>>))]
        public IEnumerable<ContentItemBasic<ContentPropertyBasic, IMedia>> GetRootMedia()
        {
            //TODO: Add permissions check!

            return Services.MediaService.GetRootMedia()
                           .Select(Mapper.Map<IMedia, ContentItemBasic<ContentPropertyBasic, IMedia>>);
        }

        /// <summary>
        /// Returns the child media objects
        /// </summary>
        [FilterAllowedOutgoingMedia(typeof(IEnumerable<ContentItemBasic<ContentPropertyBasic, IMedia>>), "Items")]
        public PagedResult<ContentItemBasic<ContentPropertyBasic, IMedia>> GetChildren(int id,
            int pageNumber = 0,
            int pageSize = 0,
            string orderBy = "SortOrder",
            Direction orderDirection = Direction.Ascending,
            string filter = "")
        {
            //TODO: Not sure how to handle 'filter' just yet! - SD: have implemented this for EntityService so I just need to get it working here,
            // will be a post filter though.

            //TODO: This will be horribly inefficient for paging! This is because our datasource/repository 
            // doesn't support paging at the SQL level... and it'll be pretty interesting to try to make that work.

            var children = Services.MediaService.GetChildren(id).ToArray();
            var totalChildren = children.Length;

            var result = children
                .Select(Mapper.Map<IMedia, ContentItemBasic<ContentPropertyBasic, IMedia>>)
                .AsQueryable();

            var orderedResult = orderDirection == Direction.Ascending
                ? result.OrderBy(orderBy)
                : result.OrderByDescending(orderBy);

            var pagedResult = new PagedResult<ContentItemBasic<ContentPropertyBasic, IMedia>>(
               totalChildren,
               pageNumber,
               pageSize);

            if (pageNumber > 0 && pageSize > 0)
            {
                pagedResult.Items = orderedResult
                    .Skip(pagedResult.SkipSize)
                    .Take(pageSize);
            }
            else
            {
                pagedResult.Items = orderedResult;
            }

            return pagedResult;

        }

        /// <summary>
        /// Moves an item to the recycle bin, if it is already there then it will permanently delete it
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns> 
        [EnsureUserPermissionForMedia("id")]
        public HttpResponseMessage DeleteById(int id)
        {
            var foundMedia = Services.MediaService.GetById(id);
            if (foundMedia == null)
            {
                return HandleContentNotFound(id, false);
            }

            //if the current item is in the recycle bin
            if (foundMedia.IsInRecycleBin() == false)
            {
                Services.MediaService.MoveToRecycleBin(foundMedia, (int)Security.CurrentUser.Id);
            }
            else
            {
                Services.MediaService.Delete(foundMedia, (int)Security.CurrentUser.Id);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// Saves content
        /// </summary>
        /// <returns></returns>        
        [FileUploadCleanupFilter]
        [MediaPostValidate]
        public MediaItemDisplay PostSave(
            [ModelBinder(typeof(MediaItemBinder))]
                MediaItemSave contentItem)
        {
            //If we've reached here it means:
            // * Our model has been bound
            // * and validated
            // * any file attachments have been saved to their temporary location for us to use
            // * we have a reference to the DTO object and the persisted object
            // * Permissions are valid

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
                    // add the modelstate to the outgoing object and throw validation response
                    var forDisplay = Mapper.Map<IMedia, MediaItemDisplay>(contentItem.PersistedContent);
                    forDisplay.Errors = ModelState.ToErrorDictionary();
                    throw new HttpResponseException(Request.CreateValidationErrorResponse(forDisplay));
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
        [EnsureUserPermissionForMedia("sorted.ParentId")]
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
                    return Request.CreateValidationErrorResponse("Media sorting failed, this was probably caused by an event being cancelled");
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MediaController>("Could not update media sort order", ex);
                throw;
            }
        }

        [EnsureUserPermissionForMedia("folder.ParentId")]
        public MediaItemDisplay PostAddFolder(EntityBasic folder)
        {
            var mediaService = ApplicationContext.Services.MediaService;
            var f = mediaService.CreateMedia(folder.Name, folder.ParentId, Constants.Conventions.MediaTypes.Folder);
            mediaService.Save(f);

            return Mapper.Map<IMedia, MediaItemDisplay>(f);
        }

        /// <summary>
        /// Used to submit a media file
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// We cannot validate this request with attributes (nicely) due to the nature of the multi-part for data.
        /// 
        /// TOOD: Validate this request properly!
        /// </remarks>
        public async Task<HttpResponseMessage> PostAddFile()
        {
            if (Request.Content.IsMimeMultipartContent() == false)
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var root = IOHelper.MapPath("~/App_Data/TEMP/FileUploads");
            //ensure it exists
            Directory.CreateDirectory(root);
            var provider = new MultipartFormDataStreamProvider(root);

            var result = await Request.Content.ReadAsMultipartAsync(provider);
            
            //must have a file
            if (result.FileData.Count == 0)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            //get the string json from the request
            int parentId;
            if (int.TryParse(result.FormData["currentFolder"], out parentId) == false)
            {
                throw new HttpResponseException(
                    new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        ReasonPhrase = "The request was not formatted correctly, the currentFolder is not an integer"
                    });
            }

            //get the files
            foreach (var file in result.FileData)
            {
                var fileName = file.Headers.ContentDisposition.FileName.Trim(new[] { '\"' });

                var mediaService = ApplicationContext.Services.MediaService;
                var f = mediaService.CreateMedia(fileName, parentId, Constants.Conventions.MediaTypes.Image);

                using (var fs = System.IO.File.OpenRead(file.LocalFileName))
                {
                    f.SetValue(Constants.Conventions.Media.File, fileName, fs);
                }

                mediaService.Save(f);

                //now we can remove the temp file
                System.IO.File.Delete(file.LocalFileName);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
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
            if (media == null && nodeId != Constants.System.Root)
            {
                media = mediaService.GetById(nodeId);
                //put the content item into storage so it can be retreived 
                // in the controller (saves a lookup)
                storage[typeof(IMedia).ToString()] = media;
            }

            if (media == null && nodeId != Constants.System.Root)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var hasPathAccess = (nodeId == Constants.System.Root)
                                    ? UserExtensions.HasPathAccess("-1", user.StartMediaId, Constants.System.RecycleBinMedia)
                                    : user.HasPathAccess(media);

            return hasPathAccess;
        }
    }
}
