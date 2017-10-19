using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Http.Controllers;
using Examine;
using Umbraco.Web.WebApi.Binders;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;
using Umbraco.Core.Configuration;
using Umbraco.Web.UI;
using Notification = Umbraco.Web.Models.ContentEditing.Notification;
using Umbraco.Core.Persistence;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Web.Editors
{
    /// <remarks>
    /// This controller is decorated with the UmbracoApplicationAuthorizeAttribute which means that any user requesting
    /// access to ALL of the methods on this controller will need access to the media application.
    /// </remarks>
    [PluginController("UmbracoApi")]
    [UmbracoApplicationAuthorize(Constants.Applications.Media)]
    [MediaControllerControllerConfiguration]
    public class MediaController : ContentControllerBase
    {
        /// <summary>
        /// Configures this controller with a custom action selector
        /// </summary>
        private class MediaControllerControllerConfigurationAttribute : Attribute, IControllerConfiguration
        {
            public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
            {
                controllerSettings.Services.Replace(typeof(IHttpActionSelector), new ParameterSwapControllerActionSelector(
                    new ParameterSwapControllerActionSelector.ParameterSwapInfo("GetById", "id", typeof(int), typeof(Guid), typeof(Udi)),
                    new ParameterSwapControllerActionSelector.ParameterSwapInfo("GetChildren", "id", typeof(int), typeof(Guid), typeof(Udi), typeof(string))));
            }
        }

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
        [OutgoingEditorModelEvent]
        public MediaItemDisplay GetEmpty(string contentTypeAlias, int parentId)
        {
            var contentType = Services.ContentTypeService.GetMediaType(contentTypeAlias);
            if (contentType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var emptyContent = Services.MediaService.CreateMedia("", parentId, contentType.Alias, UmbracoUser.Id);
            var mapped = Mapper.Map<IMedia, MediaItemDisplay>(emptyContent);

            //remove this tab if it exists: umbContainerView
            var containerTab = mapped.Tabs.FirstOrDefault(x => x.Alias == Constants.Conventions.PropertyGroups.ListViewGroupName);
            mapped.Tabs = mapped.Tabs.Except(new[] { containerTab });
            return mapped;
        }

        /// <summary>
        /// Returns an item to be used to display the recycle bin for media
        /// </summary>
        /// <returns></returns>
        public ContentItemDisplay GetRecycleBin()
        {
            var display = new ContentItemDisplay
            {
                Id = Constants.System.RecycleBinMedia,
                Alias = "recycleBin",
                ParentId = -1,
                Name = Services.TextService.Localize("general/recycleBin"),
                ContentTypeAlias = "recycleBin",
                CreateDate = DateTime.Now,
                IsContainer = true,
                Path = "-1," + Constants.System.RecycleBinMedia
            };

            TabsAndPropertiesResolver.AddListView(display, "media", Services.DataTypeService, Services.TextService);

            return display;
        }

        /// <summary>
        /// Gets the media item by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OutgoingEditorModelEvent]
        [EnsureUserPermissionForMedia("id")]
        public MediaItemDisplay GetById(int id)
        {
            var foundContent = GetObjectFromRequest(() => Services.MediaService.GetById(id));

            if (foundContent == null)
            {
                HandleContentNotFound(id);
                //HandleContentNotFound will throw an exception
                return null;
            }
            return Mapper.Map<IMedia, MediaItemDisplay>(foundContent);
        }

        /// <summary>
        /// Gets the media item by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OutgoingEditorModelEvent]
        [EnsureUserPermissionForMedia("id")]
        public MediaItemDisplay GetById(Guid id)
        {
            var foundContent = GetObjectFromRequest(() => Services.MediaService.GetById(id));

            if (foundContent == null)
            {
                HandleContentNotFound(id);
                //HandleContentNotFound will throw an exception
                return null;
            }
            return Mapper.Map<IMedia, MediaItemDisplay>(foundContent);
        }

        /// <summary>
        /// Gets the media item by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OutgoingEditorModelEvent]
        [EnsureUserPermissionForMedia("id")]
        public MediaItemDisplay GetById(Udi id)
        {
            var guidUdi = id as GuidUdi;
            if (guidUdi != null)
            {
                return GetById(guidUdi.Guid);
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Return media for the specified ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [FilterAllowedOutgoingMedia(typeof(IEnumerable<MediaItemDisplay>))]
        public IEnumerable<MediaItemDisplay> GetByIds([FromUri]int[] ids)
        {
            var foundMedia = Services.MediaService.GetByIds(ids);
            return foundMedia.Select(Mapper.Map<IMedia, MediaItemDisplay>);
        }

        /// <summary>
        /// Returns media items known to be of a "Folder" type
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Obsolete("This is no longer used and shouldn't be because it performs poorly when there are a lot of media items")]
        [FilterAllowedOutgoingMedia(typeof(IEnumerable<ContentItemBasic<ContentPropertyBasic, IMedia>>))]
        public IEnumerable<ContentItemBasic<ContentPropertyBasic, IMedia>> GetChildFolders(int id = -1)
        {
            //we are only allowing a max of 500 to be returned here, if more is required it needs to be paged
            var result = GetChildFolders(id, 1, 500);
            return result.Items;
        }

        /// <summary>
        /// Returns a paged result of media items known to be of a "Folder" type
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PagedResult<ContentItemBasic<ContentPropertyBasic, IMedia>> GetChildFolders(int id, int pageNumber, int pageSize)
        {
            //Suggested convention for folder mediatypes - we can make this more or less complicated as long as we document it...
            //if you create a media type, which has an alias that ends with ...Folder then its a folder: ex: "secureFolder", "bannerFolder", "Folder"
            var folderTypes = Services.ContentTypeService
                .GetAllMediaTypes()
                .Where(x => x.Alias.EndsWith("Folder"))
                .Select(x => x.Id)
                .ToArray();

            if (folderTypes.Length == 0)
            {
                return new PagedResult<ContentItemBasic<ContentPropertyBasic, IMedia>>(0, pageNumber, pageSize);
            }

            long total;
            var children = Services.MediaService.GetPagedChildren(id, pageNumber - 1, pageSize, out total, "Name", Direction.Ascending, true, null, folderTypes.ToArray());

            return new PagedResult<ContentItemBasic<ContentPropertyBasic, IMedia>>(total, pageNumber, pageSize)
            {
                Items = children.Select(Mapper.Map<IMedia, ContentItemBasic<ContentPropertyBasic, IMedia>>)
            };
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

        #region GetChildren

        private int[] _userStartNodes;
        protected int[] UserStartNodes
        {
            get { return _userStartNodes ?? (_userStartNodes = Security.CurrentUser.CalculateMediaStartNodeIds(Services.EntityService)); }
        }

        /// <summary>
        /// Returns the child media objects - using the entity INT id
        /// </summary>
        [FilterAllowedOutgoingMedia(typeof(IEnumerable<ContentItemBasic<ContentPropertyBasic, IMedia>>), "Items")]
        public PagedResult<ContentItemBasic<ContentPropertyBasic, IMedia>> GetChildren(int id,
            int pageNumber = 0,
            int pageSize = 0,
            string orderBy = "SortOrder",
            Direction orderDirection = Direction.Ascending,
            bool orderBySystemField = true,
            string filter = "")
        {
            //if a request is made for the root node data but the user's start node is not the default, then
            // we need to return their start nodes
            if (id == Constants.System.Root && UserStartNodes.Length > 0 && UserStartNodes.Contains(Constants.System.Root) == false)
            {
                if (pageNumber > 0)
                    return new PagedResult<ContentItemBasic<ContentPropertyBasic, IMedia>>(0, 0, 0);
                var nodes = Services.MediaService.GetByIds(UserStartNodes).ToArray();
                if (nodes.Length == 0)
                    return new PagedResult<ContentItemBasic<ContentPropertyBasic, IMedia>>(0, 0, 0);
                if (pageSize < nodes.Length) pageSize = nodes.Length; // bah
                var pr = new PagedResult<ContentItemBasic<ContentPropertyBasic, IMedia>>(nodes.Length, pageNumber, pageSize)
                {
                    Items = nodes.Select(Mapper.Map<IMedia, ContentItemBasic<ContentPropertyBasic, IMedia>>)
                };
                return pr;
            }

            // else proceed as usual

            long totalChildren;
            IMedia[] children;
            if (pageNumber > 0 && pageSize > 0)
            {
                children = Services.MediaService
                 .GetPagedChildren(id, (pageNumber - 1), pageSize, out totalChildren
                 , orderBy, orderDirection, orderBySystemField, filter).ToArray();
            }
            else
            {
                children = Services.MediaService.GetChildren(id).ToArray();
                totalChildren = children.Length;
            }

            if (totalChildren == 0)
            {
                return new PagedResult<ContentItemBasic<ContentPropertyBasic, IMedia>>(0, 0, 0);
            }

            var pagedResult = new PagedResult<ContentItemBasic<ContentPropertyBasic, IMedia>>(totalChildren, pageNumber, pageSize);
            pagedResult.Items = children
                .Select(Mapper.Map<IMedia, ContentItemBasic<ContentPropertyBasic, IMedia>>);

            return pagedResult;
        }

        /// <summary>
        /// Returns the child media objects - using the entity GUID id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="orderBy"></param>
        /// <param name="orderDirection"></param>
        /// <param name="orderBySystemField"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [FilterAllowedOutgoingMedia(typeof(IEnumerable<ContentItemBasic<ContentPropertyBasic, IMedia>>), "Items")]
        public PagedResult<ContentItemBasic<ContentPropertyBasic, IMedia>> GetChildren(Guid id,
           int pageNumber = 0,
           int pageSize = 0,
           string orderBy = "SortOrder",
           Direction orderDirection = Direction.Ascending,
           bool orderBySystemField = true,
           string filter = "")
        {
            var entity = Services.EntityService.GetByKey(id);
            if (entity != null)
            {
                return GetChildren(entity.Id, pageNumber, pageSize, orderBy, orderDirection, orderBySystemField, filter);
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Returns the child media objects - using the entity UDI id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="orderBy"></param>
        /// <param name="orderDirection"></param>
        /// <param name="orderBySystemField"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [FilterAllowedOutgoingMedia(typeof(IEnumerable<ContentItemBasic<ContentPropertyBasic, IMedia>>), "Items")]
        public PagedResult<ContentItemBasic<ContentPropertyBasic, IMedia>> GetChildren(Udi id,
           int pageNumber = 0,
           int pageSize = 0,
           string orderBy = "SortOrder",
           Direction orderDirection = Direction.Ascending,
           bool orderBySystemField = true,
           string filter = "")
        {
            var guidUdi = id as GuidUdi;
            if (guidUdi != null)
            {
                var entity = Services.EntityService.GetByKey(guidUdi.Guid);
                if (entity != null)
                {
                    return GetChildren(entity.Id, pageNumber, pageSize, orderBy, orderDirection, orderBySystemField, filter);
                }
            }

            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        [Obsolete("Do not use this method, use either the overload with INT or GUID instead, this will be removed in future versions")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [UmbracoTreeAuthorize(Constants.Trees.MediaTypes, Constants.Trees.Media)]
        public PagedResult<ContentItemBasic<ContentPropertyBasic, IMedia>> GetChildren(string id,
           int pageNumber = 0,
           int pageSize = 0,
           string orderBy = "SortOrder",
           Direction orderDirection = Direction.Ascending,
           bool orderBySystemField = true,
           string filter = "")
        {
            foreach (var type in new[] { typeof(int), typeof(Guid) })
            {
                var parsed = id.TryConvertTo(type);
                if (parsed)
                {
                    //oooh magic! will auto select the right overload
                    return GetChildren((dynamic)parsed.Result);
                }
            }

            throw new HttpResponseException(HttpStatusCode.NotFound);
        }
        #endregion

        /// <summary>
        /// Moves an item to the recycle bin, if it is already there then it will permanently delete it
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [EnsureUserPermissionForMedia("id")]
        [HttpPost]
        public HttpResponseMessage DeleteById(int id)
        {
            var foundMedia = GetObjectFromRequest(() => Services.MediaService.GetById(id));

            if (foundMedia == null)
            {
                return HandleContentNotFound(id, false);
            }

            //if the current item is in the recycle bin
            if (foundMedia.IsInRecycleBin() == false)
            {
                var moveResult = Services.MediaService.WithResult().MoveToRecycleBin(foundMedia, (int)Security.CurrentUser.Id);
                if (moveResult == false)
                {
                    //returning an object of INotificationModel will ensure that any pending
                    // notification messages are added to the response.
                    return Request.CreateValidationErrorResponse(new SimpleNotificationModel());
                }
            }
            else
            {
                var deleteResult = Services.MediaService.WithResult().Delete(foundMedia, (int)Security.CurrentUser.Id);
                if (deleteResult == false)
                {
                    //returning an object of INotificationModel will ensure that any pending
                    // notification messages are added to the response.
                    return Request.CreateValidationErrorResponse(new SimpleNotificationModel());
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// Change the sort order for media
        /// </summary>
        /// <param name="move"></param>
        /// <returns></returns>
        [EnsureUserPermissionForMedia("move.Id")]
        public HttpResponseMessage PostMove(MoveOrCopy move)
        {
            var toMove = ValidateMoveOrCopy(move);

            Services.MediaService.Move(toMove, move.ParentId);

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(toMove.Path, Encoding.UTF8, "application/json");
            return response;
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

            MapPropertyValues(contentItem);

            //We need to manually check the validation results here because:
            // * We still need to save the entity even if there are validation value errors
            // * Depending on if the entity is new, and if there are non property validation errors (i.e. the name is null)
            //      then we cannot continue saving, we can only display errors
            // * If there are validation errors and they were attempting to publish, we can only save, NOT publish and display
            //      a message indicating this
            if (ModelState.IsValid == false)
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
            var saveStatus = Services.MediaService.WithResult().Save(contentItem.PersistedContent, (int)Security.CurrentUser.Id);

            //return the updated model
            var display = Mapper.Map<IMedia, MediaItemDisplay>(contentItem.PersistedContent);

            //lasty, if it is not valid, add the modelstate to the outgoing object and throw a 403
            HandleInvalidModelState(display);

            //put the correct msgs in
            switch (contentItem.Action)
            {
                case ContentSaveAction.Save:
                case ContentSaveAction.SaveNew:
                    if (saveStatus.Success)
                    {
                        display.AddSuccessNotification(
                            Services.TextService.Localize("speechBubbles/editMediaSaved"),
                            Services.TextService.Localize("speechBubbles/editMediaSavedText"));
                    }
                    else
                    {
                        AddCancelMessage(display);

                        //If the item is new and the operation was cancelled, we need to return a different
                        // status code so the UI can handle it since it won't be able to redirect since there
                        // is no Id to redirect to!
                        if (saveStatus.Result.StatusType == OperationStatusType.FailedCancelledByEvent && IsCreatingAction(contentItem.Action))
                        {
                            throw new HttpResponseException(Request.CreateValidationErrorResponse(display));
                        }
                    }

                    break;
            }

            return display;
        }

        /// <summary>
        /// Maps the property values to the persisted entity
        /// </summary>
        /// <param name="contentItem"></param>
        protected override void MapPropertyValues<TPersisted>(ContentBaseItemSave<TPersisted> contentItem)
        {
            UpdateName(contentItem);

            //use the base method to map the rest of the properties
            base.MapPropertyValues(contentItem);
        }

        /// <summary>
        /// Empties the recycle bin
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [HttpPost]
        public HttpResponseMessage EmptyRecycleBin()
        {
            Services.MediaService.EmptyRecycleBin();

            return Request.CreateNotificationSuccessResponse(Services.TextService.Localize("defaultdialogs/recycleBinIsEmpty"));
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
        
        public MediaItemDisplay PostAddFolder(PostedFolder folder)
        {
            var intParentId = GetParentIdAsInt(folder.ParentId, validatePermissions:true);
            
            var mediaService = ApplicationContext.Services.MediaService;

            var f = mediaService.CreateMedia(folder.Name, intParentId, Constants.Conventions.MediaTypes.Folder);
            mediaService.Save(f, Security.CurrentUser.Id);

            return Mapper.Map<IMedia, MediaItemDisplay>(f);
        }

        /// <summary>
        /// Used to submit a media file
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// We cannot validate this request with attributes (nicely) due to the nature of the multi-part for data.
        /// </remarks>
        [FileUploadCleanupFilter(false)]
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
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            //get the string json from the request
            string currentFolderId = result.FormData["currentFolder"];
            int parentId = GetParentIdAsInt(currentFolderId, validatePermissions: true);
           
            var tempFiles = new PostedFiles();
            var mediaService = ApplicationContext.Services.MediaService;
            
            //in case we pass a path with a folder in it, we will create it and upload media to it.
            if (result.FormData.ContainsKey("path"))
            {

                var folders = result.FormData["path"].Split('/');

                for (int i = 0; i < folders.Length - 1; i++)
                {
                    var folderName = folders[i];
                    IMedia folderMediaItem;

                    //if uploading directly to media root and not a subfolder
                    if (parentId == -1)
                    {
                        //look for matching folder
                        folderMediaItem =
                            mediaService.GetRootMedia().FirstOrDefault(x => x.Name == folderName && x.ContentType.Alias == Constants.Conventions.MediaTypes.Folder);
                        if (folderMediaItem == null)
                        {
                            //if null, create a folder
                            folderMediaItem = mediaService.CreateMedia(folderName, -1, Constants.Conventions.MediaTypes.Folder);
                            mediaService.Save(folderMediaItem);
                        }
                    }
                    else
                    {
                        //get current parent
                        var mediaRoot = mediaService.GetById(parentId);

                        //if the media root is null, something went wrong, we'll abort
                        if (mediaRoot == null)
                            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                                "The folder: " + folderName + " could not be used for storing images, its ID: " + parentId +
                                " returned null");

                        //look for matching folder
                        folderMediaItem = mediaRoot.Children().FirstOrDefault(x => x.Name == folderName && x.ContentType.Alias == Constants.Conventions.MediaTypes.Folder);
                        if (folderMediaItem == null)
                        {
                            //if null, create a folder
                            folderMediaItem = mediaService.CreateMedia(folderName, mediaRoot, Constants.Conventions.MediaTypes.Folder);
                            mediaService.Save(folderMediaItem);
                        }
                    }
                    //set the media root to the folder id so uploaded files will end there.
                    parentId = folderMediaItem.Id;
                }
            }

            //get the files
            foreach (var file in result.FileData)
            {
                var fileName = file.Headers.ContentDisposition.FileName.Trim(new[] { '\"' }).TrimEnd();
                var safeFileName = fileName.ToSafeFileName();
                var ext = safeFileName.Substring(safeFileName.LastIndexOf('.') + 1).ToLower();

                if (UmbracoConfig.For.UmbracoSettings().Content.IsFileAllowedForUpload(ext))
                {
                    var mediaType = Constants.Conventions.MediaTypes.File;

                    if (result.FormData["contentTypeAlias"] == Constants.Conventions.MediaTypes.AutoSelect)
                    {
                        if (UmbracoConfig.For.UmbracoSettings().Content.ImageFileTypes.Contains(ext))
                        {
                            mediaType = Constants.Conventions.MediaTypes.Image;
                        }
                    }
                    else
                    {
                        mediaType = result.FormData["contentTypeAlias"];
                    }

                    //TODO: make the media item name "nice" since file names could be pretty ugly, we have
                    // string extensions to do much of this but we'll need:
                    // * Pascalcase the name (use string extensions)
                    // * strip the file extension
                    // * underscores to spaces
                    // * probably remove 'ugly' characters - let's discuss
                    // All of this logic should exist in a string extensions method and be unit tested
                    // http://issues.umbraco.org/issue/U4-5572
                    var mediaItemName = fileName;

                    var f = mediaService.CreateMedia(mediaItemName, parentId, mediaType, Security.CurrentUser.Id);

                    var fileInfo = new FileInfo(file.LocalFileName);
                    var fs = fileInfo.OpenReadWithRetry();
                    if (fs == null) throw new InvalidOperationException("Could not acquire file stream");
                    using (fs)
                    {
                        f.SetValue(Constants.Conventions.Media.File, fileName, fs);
                    }

                    var saveResult = mediaService.WithResult().Save(f, Security.CurrentUser.Id);
                    if (saveResult == false)
                    {
                        AddCancelMessage(tempFiles,
                            message: Services.TextService.Localize("speechBubbles/operationCancelledText") + " -- " + mediaItemName,
                            localizeMessage: false);
                    }
                    else
                    {
                        tempFiles.UploadedFiles.Add(new ContentItemFile
                        {
                            FileName = fileName,
                            PropertyAlias = Constants.Conventions.Media.File,
                            TempFilePath = file.LocalFileName
                        });
                    }
                }
                else
                {
                    tempFiles.Notifications.Add(new Notification(
                        Services.TextService.Localize("speechBubbles/operationFailedHeader"),
                        Services.TextService.Localize("media/disallowedFileType"),
                        SpeechBubbleIcon.Warning));
                }
            }

            //Different response if this is a 'blueimp' request
            if (Request.GetQueryNameValuePairs().Any(x => x.Key == "origin"))
            {
                var origin = Request.GetQueryNameValuePairs().First(x => x.Key == "origin");
                if (origin.Value == "blueimp")
                {
                    return Request.CreateResponse(HttpStatusCode.OK,
                        tempFiles,
                        //Don't output the angular xsrf stuff, blue imp doesn't like that
                        new JsonMediaTypeFormatter());
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, tempFiles);
        }

        /// <summary>
        /// Given a parent id which could be a GUID, UDI or an INT, this will resolve the INT
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="validatePermissions">
        /// If true, this will check if the current user has access to the resolved integer parent id
        /// and if that check fails an unauthorized exception will occur
        /// </param>
        /// <returns></returns>
        private int GetParentIdAsInt(string parentId, bool validatePermissions)
        {
            int intParentId;
            GuidUdi parentUdi;

            // test for udi
            if (GuidUdi.TryParse(parentId, out parentUdi))
            {
                parentId = parentUdi.Guid.ToString();
            }

            //if it's not an INT then we'll check for GUID
            if (int.TryParse(parentId, out intParentId) == false)
            {
                // if a guid then try to look up the entity
                Guid idGuid;
                if (Guid.TryParse(parentId, out idGuid))
                {
                    var entity = Services.EntityService.GetByKey(idGuid);
                    if (entity != null)
                    {
                        intParentId = entity.Id;
                    }
                    else
                    {
                        throw new EntityNotFoundException(parentId, "The passed id doesn't exist");
                    }
                }
                else
                {
                    throw new HttpResponseException(
                        Request.CreateValidationErrorResponse("The request was not formatted correctly, the parentId is not an integer, Guid or UDI"));
                }
            }

            //ensure the user has access to this folder by parent id!
            if (validatePermissions && CheckPermissions(
                    new Dictionary<string, object>(),
                    Security.CurrentUser,
                    Services.MediaService,
                    Services.EntityService,
                    intParentId) == false)
            {
                throw new HttpResponseException(Request.CreateResponse(
                    HttpStatusCode.Forbidden,
                    new SimpleNotificationModel(new Notification(
                        Services.TextService.Localize("speechBubbles/operationFailedHeader"),
                        Services.TextService.Localize("speechBubbles/invalidUserPermissionsText"),
                        SpeechBubbleIcon.Warning))));
            }

            return intParentId;
        }

        /// <summary>
        /// Ensures the item can be moved/copied to the new location
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private IMedia ValidateMoveOrCopy(MoveOrCopy model)
        {
            if (model == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var mediaService = Services.MediaService;
            var toMove = mediaService.GetById(model.Id);
            if (toMove == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            if (model.ParentId < 0)
            {
                //cannot move if the content item is not allowed at the root
                if (toMove.ContentType.AllowedAsRoot == false)
                {
                    var notificationModel = new SimpleNotificationModel();
                    notificationModel.AddErrorNotification(Services.TextService.Localize("moveOrCopy/notAllowedAtRoot"), "");
                    throw new HttpResponseException(Request.CreateValidationErrorResponse(notificationModel));
                }
            }
            else
            {
                var parent = mediaService.GetById(model.ParentId);
                if (parent == null)
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }

                //check if the item is allowed under this one
                if (parent.ContentType.AllowedContentTypes.Select(x => x.Id).ToArray()
                    .Any(x => x.Value == toMove.ContentType.Id) == false)
                {
                    var notificationModel = new SimpleNotificationModel();
                    notificationModel.AddErrorNotification(Services.TextService.Localize("moveOrCopy/notAllowedByContentType"), "");
                    throw new HttpResponseException(Request.CreateValidationErrorResponse(notificationModel));
                }

                // Check on paths
                if ((string.Format(",{0},", parent.Path)).IndexOf(string.Format(",{0},", toMove.Id), StringComparison.Ordinal) > -1)
                {
                    var notificationModel = new SimpleNotificationModel();
                    notificationModel.AddErrorNotification(Services.TextService.Localize("moveOrCopy/notAllowedByPath"), "");
                    throw new HttpResponseException(Request.CreateValidationErrorResponse(notificationModel));
                }
            }

            return toMove;
        }

        /// <summary>
        /// Performs a permissions check for the user to check if it has access to the node based on
        /// start node and/or permissions for the node
        /// </summary>
        /// <param name="storage">The storage to add the content item to so it can be reused</param>
        /// <param name="user"></param>
        /// <param name="mediaService"></param>
        /// <param name="entityService"></param>
        /// <param name="nodeId">The content to lookup, if the contentItem is not specified</param>
        /// <param name="media">Specifies the already resolved content item to check against, setting this ignores the nodeId</param>
        /// <returns></returns>
        internal static bool CheckPermissions(IDictionary<string, object> storage, IUser user, IMediaService mediaService, IEntityService entityService, int nodeId, IMedia media = null)
        {
            if (storage == null) throw new ArgumentNullException("storage");
            if (user == null) throw new ArgumentNullException("user");
            if (mediaService == null) throw new ArgumentNullException("mediaService");
            if (entityService == null) throw new ArgumentNullException("entityService");

            if (media == null && nodeId != Constants.System.Root && nodeId != Constants.System.RecycleBinMedia)
            {
                media = mediaService.GetById(nodeId);
                //put the content item into storage so it can be retreived
                // in the controller (saves a lookup)
                storage[typeof(IMedia).ToString()] = media;
            }

            if (media == null && nodeId != Constants.System.Root && nodeId != Constants.System.RecycleBinMedia)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var hasPathAccess = (nodeId == Constants.System.Root)
                ? user.HasMediaRootAccess(entityService)
                : (nodeId == Constants.System.RecycleBinMedia)
                    ? user.HasMediaBinAccess(entityService)
                    : user.HasPathAccess(media, entityService);

            return hasPathAccess;
        }
    }
}
