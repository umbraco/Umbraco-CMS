using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using System.Linq;
using System.Web.Http.Controllers;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;
using Umbraco.Core.Persistence.Querying;
using Notification = Umbraco.Web.Models.ContentEditing.Notification;
using Umbraco.Core.Persistence;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.Models.Validation;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.ContentApps;
using Umbraco.Web.Editors.Binders;
using Umbraco.Web.Editors.Filters;
using Umbraco.Core.Models.Entities;

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
        public MediaController(PropertyEditorCollection propertyEditors, IGlobalSettings globalSettings, IUmbracoContextAccessor umbracoContextAccessor, ISqlContext sqlContext, ServiceContext services, AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState, UmbracoHelper umbracoHelper)
            : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoHelper)
        {
            _propertyEditors = propertyEditors ?? throw new ArgumentNullException(nameof(propertyEditors));
        }

        /// <summary>
        /// Configures this controller with a custom action selector
        /// </summary>
        private class MediaControllerControllerConfigurationAttribute : Attribute, IControllerConfiguration
        {
            public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
            {
                controllerSettings.Services.Replace(typeof(IHttpActionSelector), new ParameterSwapControllerActionSelector(
                    new ParameterSwapControllerActionSelector.ParameterSwapInfo("GetById", "id", typeof(int), typeof(Guid), typeof(Udi)),
                    new ParameterSwapControllerActionSelector.ParameterSwapInfo("GetChildren", "id", typeof(int), typeof(Guid), typeof(Udi))));
            }
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
            var contentType = Services.MediaTypeService.Get(contentTypeAlias);
            if (contentType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var emptyContent = Services.MediaService.CreateMedia("", parentId, contentType.Alias, Security.GetUserId().ResultOr(Constants.Security.SuperUserId));
            var mapped = Mapper.Map<MediaItemDisplay>(emptyContent);

            //remove the listview app if it exists
            mapped.ContentApps = mapped.ContentApps.Where(x => x.Alias != "umbListView").ToList();

            return mapped;
        }

        /// <summary>
        /// Returns an item to be used to display the recycle bin for media
        /// </summary>
        /// <returns></returns>
        public MediaItemDisplay GetRecycleBin()
        {
            var apps = new List<ContentApp>();
            apps.Add(ListViewContentAppFactory.CreateContentApp(Services.DataTypeService, _propertyEditors, "recycleBin", "media", Core.Constants.DataTypes.DefaultMediaListView));
            apps[0].Active = true;
            var display = new MediaItemDisplay
            {
                Id = Constants.System.RecycleBinMedia,
                Alias = "recycleBin",
                ParentId = -1,
                Name = Services.TextService.Localize("general", "recycleBin"),
                ContentTypeAlias = "recycleBin",
                CreateDate = DateTime.Now,
                IsContainer = true,
                Path = "-1," + Constants.System.RecycleBinMedia,
                ContentApps = apps
            };

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
            var foundMedia = GetObjectFromRequest(() => Services.MediaService.GetById(id));

            if (foundMedia == null)
            {
                HandleContentNotFound(id);
                //HandleContentNotFound will throw an exception
                return null;
            }
            return Mapper.Map<MediaItemDisplay>(foundMedia);
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
            var foundMedia = GetObjectFromRequest(() => Services.MediaService.GetById(id));

            if (foundMedia == null)
            {
                HandleContentNotFound(id);
                //HandleContentNotFound will throw an exception
                return null;
            }
            return Mapper.Map<MediaItemDisplay>(foundMedia);
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
            return foundMedia.Select(media => Mapper.Map<MediaItemDisplay>(media));
        }

        /// <summary>
        /// Returns a paged result of media items known to be of a "Folder" type
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public PagedResult<ContentItemBasic<ContentPropertyBasic>> GetChildFolders(int id, int pageNumber = 1, int pageSize = 1000)
        {
            //Suggested convention for folder mediatypes - we can make this more or less complicated as long as we document it...
            //if you create a media type, which has an alias that ends with ...Folder then its a folder: ex: "secureFolder", "bannerFolder", "Folder"
            var folderTypes = Services.MediaTypeService
                .GetAll()
                .Where(x => x.Alias.EndsWith("Folder"))
                .Select(x => x.Id)
                .ToArray();

            if (folderTypes.Length == 0)
            {
                return new PagedResult<ContentItemBasic<ContentPropertyBasic>>(0, pageNumber, pageSize);
            }

            long total;
            var children = Services.MediaService.GetPagedChildren(id, pageNumber - 1, pageSize, out total,
                //lookup these content types
                SqlContext.Query<IMedia>().Where(x => folderTypes.Contains(x.ContentTypeId)),
                Ordering.By("Name", Direction.Ascending));

            return new PagedResult<ContentItemBasic<ContentPropertyBasic>>(total, pageNumber, pageSize)
            {
                Items = children.Select(Mapper.Map<IMedia, ContentItemBasic<ContentPropertyBasic>>)
            };
        }

        /// <summary>
        /// Returns the root media objects
        /// </summary>
        [FilterAllowedOutgoingMedia(typeof(IEnumerable<ContentItemBasic<ContentPropertyBasic>>))]
        public IEnumerable<ContentItemBasic<ContentPropertyBasic>> GetRootMedia()
        {
            // TODO: Add permissions check!

            return Services.MediaService.GetRootMedia()
                           .Select(Mapper.Map<IMedia, ContentItemBasic<ContentPropertyBasic>>);
        }

        #region GetChildren

        private int[] _userStartNodes;
        private readonly PropertyEditorCollection _propertyEditors;

        protected int[] UserStartNodes
        {
            get { return _userStartNodes ?? (_userStartNodes = Security.CurrentUser.CalculateMediaStartNodeIds(Services.EntityService, AppCaches)); }
        }

        /// <summary>
        /// Returns the child media objects - using the entity INT id
        /// </summary>
        [FilterAllowedOutgoingMedia(typeof(IEnumerable<ContentItemBasic<ContentPropertyBasic>>), "Items")]
        public PagedResult<ContentItemBasic<ContentPropertyBasic>> GetChildren(int id,
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
                    return new PagedResult<ContentItemBasic<ContentPropertyBasic>>(0, 0, 0);
                var nodes = Services.MediaService.GetByIds(UserStartNodes).ToArray();
                if (nodes.Length == 0)
                    return new PagedResult<ContentItemBasic<ContentPropertyBasic>>(0, 0, 0);
                if (pageSize < nodes.Length) pageSize = nodes.Length; // bah
                var pr = new PagedResult<ContentItemBasic<ContentPropertyBasic>>(nodes.Length, pageNumber, pageSize)
                {
                    Items = nodes.Select(Mapper.Map<IMedia, ContentItemBasic<ContentPropertyBasic>>)
                };
                return pr;
            }

            // else proceed as usual

            long totalChildren;
            List<IMedia> children;
            if (pageNumber > 0 && pageSize > 0)
            {
                IQuery<IMedia> queryFilter = null;
                if (filter.IsNullOrWhiteSpace() == false)
                {
                    //add the default text filter
                    queryFilter = SqlContext.Query<IMedia>()
                        .Where(x => x.Name.Contains(filter));
                }

                children = Services.MediaService
                    .GetPagedChildren(
                        id, (pageNumber - 1), pageSize,
                        out totalChildren,
                        queryFilter,
                        Ordering.By(orderBy, orderDirection, isCustomField: !orderBySystemField)).ToList();
            }
            else
            {
                //better to not use this without paging where possible, currently only the sort dialog does
                children = Services.MediaService.GetPagedChildren(id,0, int.MaxValue, out var total).ToList();
                totalChildren = children.Count;
            }

            if (totalChildren == 0)
            {
                return new PagedResult<ContentItemBasic<ContentPropertyBasic>>(0, 0, 0);
            }

            var pagedResult = new PagedResult<ContentItemBasic<ContentPropertyBasic>>(totalChildren, pageNumber, pageSize);
            pagedResult.Items = children
                .Select(Mapper.Map<IMedia, ContentItemBasic<ContentPropertyBasic>>);

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
        [FilterAllowedOutgoingMedia(typeof(IEnumerable<ContentItemBasic<ContentPropertyBasic>>), "Items")]
        public PagedResult<ContentItemBasic<ContentPropertyBasic>> GetChildren(Guid id,
           int pageNumber = 0,
           int pageSize = 0,
           string orderBy = "SortOrder",
           Direction orderDirection = Direction.Ascending,
           bool orderBySystemField = true,
           string filter = "")
        {
            var entity = Services.EntityService.Get(id);
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
        [FilterAllowedOutgoingMedia(typeof(IEnumerable<ContentItemBasic<ContentPropertyBasic>>), "Items")]
        public PagedResult<ContentItemBasic<ContentPropertyBasic>> GetChildren(Udi id,
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
                var entity = Services.EntityService.Get(guidUdi.Guid);
                if (entity != null)
                {
                    return GetChildren(entity.Id, pageNumber, pageSize, orderBy, orderDirection, orderBySystemField, filter);
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
            if (foundMedia.Trashed == false)
            {
                var moveResult = Services.MediaService.MoveToRecycleBin(foundMedia, Security.GetUserId().ResultOr(Constants.Security.SuperUserId));
                if (moveResult == false)
                {
                    //returning an object of INotificationModel will ensure that any pending
                    // notification messages are added to the response.
                    return Request.CreateValidationErrorResponse(new SimpleNotificationModel());
                }
            }
            else
            {
                var deleteResult = Services.MediaService.Delete(foundMedia, Security.GetUserId().ResultOr(Constants.Security.SuperUserId));
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
            var destinationParentID = move.ParentId;
            var sourceParentID = toMove.ParentId;

            var moveResult = Services.MediaService.Move(toMove, move.ParentId, Security.GetUserId().ResultOr(Constants.Security.SuperUserId));

            if (sourceParentID == destinationParentID)
            {
                return Request.CreateValidationErrorResponse(new SimpleNotificationModel(new Notification("",Services.TextService.Localize("media", "moveToSameFolderFailed"),NotificationStyle.Error)));
            }
            if (moveResult == false)
            {
                return Request.CreateValidationErrorResponse(new SimpleNotificationModel());
            }
            else
            {
                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(toMove.Path, Encoding.UTF8, "text/plain");
                return response;
            }
        }

        /// <summary>
        /// Saves content
        /// </summary>
        /// <returns></returns>
        [FileUploadCleanupFilter]
        [MediaItemSaveValidation]
        [OutgoingEditorModelEvent]
        public MediaItemDisplay PostSave(
            [ModelBinder(typeof(MediaItemBinder))]
                MediaItemSave contentItem)
        {
            //Recent versions of IE/Edge may send in the full client side file path instead of just the file name.
            //To ensure similar behavior across all browsers no matter what they do - we strip the FileName property of all
            //uploaded files to being *only* the actual file name (as it should be).
            if (contentItem.UploadedFiles != null && contentItem.UploadedFiles.Any())
            {
                foreach (var file in contentItem.UploadedFiles)
                {
                    file.FileName = Path.GetFileName(file.FileName);
                }
            }

            //If we've reached here it means:
            // * Our model has been bound
            // * and validated
            // * any file attachments have been saved to their temporary location for us to use
            // * we have a reference to the DTO object and the persisted object
            // * Permissions are valid

            //Don't update the name if it is empty
            if (contentItem.Name.IsNullOrWhiteSpace() == false)
            {
                contentItem.PersistedContent.Name = contentItem.Name;
            }

            MapPropertyValuesForPersistence<IMedia, MediaItemSave>(
                contentItem,
                contentItem.PropertyCollectionDto,
                (save, property) => property.GetValue(), //get prop val
                (save, property, v) => property.SetValue(v), //set prop val
                null); // media are all invariant

            //we will continue to save if model state is invalid, however we cannot save if critical data is missing.
            //TODO: Allowing media to be saved when it is invalid is odd - media doesn't have a publish phase so suddenly invalid data is allowed to be 'live'
            if (!ModelState.IsValid)
            {
                //check for critical data validation issues, we can't continue saving if this data is invalid
                if (!RequiredForPersistenceAttribute.HasRequiredValuesForPersistence(contentItem))
                {
                    //ok, so the absolute mandatory data is invalid and it's new, we cannot actually continue!
                    // add the model state to the outgoing object and throw validation response
                    var forDisplay = Mapper.Map<MediaItemDisplay>(contentItem.PersistedContent);
                    forDisplay.Errors = ModelState.ToErrorDictionary();
                    throw new HttpResponseException(Request.CreateValidationErrorResponse(forDisplay));
                }
            }

            //save the item
            var saveStatus = Services.MediaService.Save(contentItem.PersistedContent, Security.GetUserId().ResultOr(Constants.Security.SuperUserId));

            //return the updated model
            var display = Mapper.Map<MediaItemDisplay>(contentItem.PersistedContent);

            //lastly, if it is not valid, add the model state to the outgoing object and throw a 403
            HandleInvalidModelState(display);

            //put the correct msgs in
            switch (contentItem.Action)
            {
                case ContentSaveAction.Save:
                case ContentSaveAction.SaveNew:
                    if (saveStatus.Success)
                    {
                        display.AddSuccessNotification(
                            Services.TextService.Localize("speechBubbles", "editMediaSaved"),
                            Services.TextService.Localize("speechBubbles", "editMediaSavedText"));
                    }
                    else
                    {
                        AddCancelMessage(display);

                        //If the item is new and the operation was cancelled, we need to return a different
                        // status code so the UI can handle it since it won't be able to redirect since there
                        // is no Id to redirect to!
                        if (saveStatus.Result.Result == OperationResultType.FailedCancelledByEvent && IsCreatingAction(contentItem.Action))
                        {
                            throw new HttpResponseException(Request.CreateValidationErrorResponse(display));
                        }
                    }

                    break;
            }

            return display;
        }

        /// <summary>
        /// Empties the recycle bin
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [HttpPost]
        public HttpResponseMessage EmptyRecycleBin()
        {
            Services.MediaService.EmptyRecycleBin(Security.GetUserId().ResultOr(Constants.Security.SuperUserId));

            return Request.CreateNotificationSuccessResponse(Services.TextService.Localize("defaultdialogs", "recycleBinIsEmpty"));
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

            var mediaService = Services.MediaService;
            var sortedMedia = new List<IMedia>();
            try
            {
                sortedMedia.AddRange(sorted.IdSortOrder.Select(mediaService.GetById));

                // Save Media with new sort order and update content xml in db accordingly
                if (mediaService.Sort(sortedMedia) == false)
                {
                    Logger.Warn<MediaController>("Media sorting failed, this was probably caused by an event being cancelled");
                    return Request.CreateValidationErrorResponse("Media sorting failed, this was probably caused by an event being cancelled");
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Logger.Error<MediaController>(ex, "Could not update media sort order");
                throw;
            }
        }

        public MediaItemDisplay PostAddFolder(PostedFolder folder)
        {
            var intParentId = GetParentIdAsInt(folder.ParentId, validatePermissions:true);

            var mediaService = Services.MediaService;

            var f = mediaService.CreateMedia(folder.Name, intParentId, Constants.Conventions.MediaTypes.Folder);
            mediaService.Save(f, Security.CurrentUser.Id);

            return Mapper.Map<MediaItemDisplay>(f);
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

            var root = IOHelper.MapPath(SystemDirectories.TempFileUploads);
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
            var mediaService = Services.MediaService;

            //in case we pass a path with a folder in it, we will create it and upload media to it.
            if (result.FormData.ContainsKey("path"))
            {

                var folders = result.FormData["path"].Split(Constants.CharArrays.ForwardSlash);

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
                        folderMediaItem = FindInChildren(mediaRoot.Id, folderName, Constants.Conventions.MediaTypes.Folder);

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
                var fileName = file.Headers.ContentDisposition.FileName.Trim(Constants.CharArrays.DoubleQuote).TrimEnd();
                var safeFileName = fileName.ToSafeFileName();
                var ext = safeFileName.Substring(safeFileName.LastIndexOf('.') + 1).ToLower();

                if (Current.Configs.Settings().Content.IsFileAllowedForUpload(ext))
                {
                    var mediaType = Constants.Conventions.MediaTypes.File;

                    if (result.FormData["contentTypeAlias"] == Constants.Conventions.MediaTypes.AutoSelect)
                    {
                        var mediaTypes = Services.MediaTypeService.GetAll();
                        // Look up MediaTypes
                        foreach (var mediaTypeItem in mediaTypes)
                        {
                            var fileProperty = mediaTypeItem.CompositionPropertyTypes.FirstOrDefault(x => x.Alias == "umbracoFile");
                            if (fileProperty != null) {
                                var dataTypeKey = fileProperty.DataTypeKey;
                                var dataType = Services.DataTypeService.GetDataType(dataTypeKey);

                                if (dataType != null && dataType.Configuration is IFileExtensionsConfig fileExtensionsConfig) {
                                    var fileExtensions = fileExtensionsConfig.FileExtensions;
                                    if (fileExtensions != null)
                                    {
                                        if (fileExtensions.Where(x => x.Value == ext).Count() != 0)
                                        {
                                            mediaType = mediaTypeItem.Alias;
                                            break;
                                        }
                                    }
                                }
                            }

                        }

                        // If media type is still File then let's check if it's an image.
                        if (mediaType == Constants.Conventions.MediaTypes.File && Current.Configs.Settings().Content.ImageFileTypes.Contains(ext))
                        {
                            mediaType = Constants.Conventions.MediaTypes.Image;
                        }
                    }
                    else
                    {
                        mediaType = result.FormData["contentTypeAlias"];
                    }

                    var mediaItemName = fileName.ToFriendlyName();

                    var f = mediaService.CreateMedia(mediaItemName, parentId, mediaType, Security.CurrentUser.Id);

                    var fileInfo = new FileInfo(file.LocalFileName);
                    var fs = fileInfo.OpenReadWithRetry();
                    if (fs == null) throw new InvalidOperationException("Could not acquire file stream");
                    using (fs)
                    {
                        f.SetValue(Services.ContentTypeBaseServices, Constants.Conventions.Media.File,fileName, fs);
                    }

                    var saveResult = mediaService.Save(f, Security.CurrentUser.Id);
                    if (saveResult == false)
                    {
                        AddCancelMessage(tempFiles,
                            message: Services.TextService.Localize("speechBubbles", "operationCancelledText") + " -- " + mediaItemName);
                    }
                    else
                    {
                        tempFiles.UploadedFiles.Add(new ContentPropertyFile
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
                        Services.TextService.Localize("speechBubbles", "operationFailedHeader"),
                        Services.TextService.Localize("media", "disallowedFileType"),
                        NotificationStyle.Warning));
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

        private IMedia FindInChildren(int mediaId, string nameToFind, string contentTypeAlias)
        {
            const int pageSize = 500;
            var page = 0;
            var total = long.MaxValue;
            while (page * pageSize < total)
            {
                var children = Services.MediaService.GetPagedChildren(mediaId, page++, pageSize, out total,
                    SqlContext.Query<IMedia>().Where(x => x.Name == nameToFind));
                var match = children.FirstOrDefault(c => c.ContentType.Alias == contentTypeAlias);
                if (match != null)
                {
                    return match;
                }
            }
            return null;
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
                    var entity = Services.EntityService.Get(idGuid);
                    if (entity != null)
                    {
                        intParentId = entity.Id;
                    }
                    else
                    {
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "The passed id doesn't exist"));
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
                    AppCaches,
                    intParentId) == false)
            {
                throw new HttpResponseException(Request.CreateResponse(
                    HttpStatusCode.Forbidden,
                    new SimpleNotificationModel(new Notification(
                        Services.TextService.Localize("speechBubbles", "operationFailedHeader"),
                        Services.TextService.Localize("speechBubbles", "invalidUserPermissionsText"),
                        NotificationStyle.Warning))));
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
                //cannot move if the content item is not allowed at the root unless there are
                //none allowed at root (in which case all should be allowed at root)
                var mediaTypeService = Services.MediaTypeService;
                if (toMove.ContentType.AllowedAsRoot == false && mediaTypeService.GetAll().Any(ct => ct.AllowedAsRoot))
                {
                    var notificationModel = new SimpleNotificationModel();
                    notificationModel.AddErrorNotification(Services.TextService.Localize("moveOrCopy", "notAllowedAtRoot"), "");
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
                var parentContentType = Services.MediaTypeService.Get(parent.ContentTypeId);
                if (parentContentType.AllowedContentTypes.Select(x => x.Id).ToArray()
                    .Any(x => x.Value == toMove.ContentType.Id) == false)
                {
                    var notificationModel = new SimpleNotificationModel();
                    notificationModel.AddErrorNotification(Services.TextService.Localize("moveOrCopy", "notAllowedByContentType"), "");
                    throw new HttpResponseException(Request.CreateValidationErrorResponse(notificationModel));
                }

                // Check on paths
                if ((string.Format(",{0},", parent.Path)).IndexOf(string.Format(",{0},", toMove.Id), StringComparison.Ordinal) > -1)
                {
                    var notificationModel = new SimpleNotificationModel();
                    notificationModel.AddErrorNotification(Services.TextService.Localize("moveOrCopy", "notAllowedByPath"), "");
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
        internal static bool CheckPermissions(IDictionary<string, object> storage, IUser user, IMediaService mediaService, IEntityService entityService, AppCaches appCaches, int nodeId, IMedia media = null)
        {
            if (storage == null) throw new ArgumentNullException("storage");
            if (user == null) throw new ArgumentNullException("user");
            if (mediaService == null) throw new ArgumentNullException("mediaService");
            if (entityService == null) throw new ArgumentNullException("entityService");

            if (media == null && nodeId != Constants.System.Root && nodeId != Constants.System.RecycleBinMedia)
            {
                media = mediaService.GetById(nodeId);
                //put the content item into storage so it can be retrieved
                // in the controller (saves a lookup)
                storage[typeof(IMedia).ToString()] = media;
            }

            if (media == null && nodeId != Constants.System.Root && nodeId != Constants.System.RecycleBinMedia)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var hasPathAccess = (nodeId == Constants.System.Root)
                ? user.HasMediaRootAccess(entityService, appCaches)
                : (nodeId == Constants.System.RecycleBinMedia)
                    ? user.HasMediaBinAccess(entityService, appCaches)
                    : user.HasPathAccess(media, entityService, appCaches);

            return hasPathAccess;
        }

        public PagedResult<EntityBasic> GetPagedReferences(int id, string entityType, int pageNumber = 1, int pageSize = 100)
        {
            if (pageNumber <= 0 || pageSize <= 0)
            {
                throw new NotSupportedException("Both pageNumber and pageSize must be greater than zero");
            }

            var objectType = ObjectTypes.GetUmbracoObjectType(entityType);
            var udiType = ObjectTypes.GetUdiType(objectType);

            var relations = Services.RelationService.GetPagedParentEntitiesByChildId(id, pageNumber - 1, pageSize, out var totalRecords, objectType);

            return new PagedResult<EntityBasic>(totalRecords, pageNumber, pageSize)
            {
                Items = relations.Cast<ContentEntitySlim>().Select(rel => new EntityBasic
                {
                    Id = rel.Id,
                    Key = rel.Key,
                    Udi = Udi.Create(udiType, rel.Key),
                    Icon = rel.ContentTypeIcon,
                    Name = rel.Name,
                    Alias = rel.ContentTypeAlias
                })
            };
        }
    }
}
