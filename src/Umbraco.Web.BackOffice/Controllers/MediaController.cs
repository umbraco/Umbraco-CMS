using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Events;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Models.Validation;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.ContentApps;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;
using Umbraco.Core.Mapping;
using Umbraco.Core.Strings;
using Umbraco.Extensions;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Common.Exceptions;
using Umbraco.Web.Editors.Binders;
using Umbraco.Web.Security;

namespace Umbraco.Web.Editors
{
    /// <remarks>
    /// This controller is decorated with the UmbracoApplicationAuthorizeAttribute which means that any user requesting
    /// access to ALL of the methods on this controller will need access to the media application.
    /// </remarks>
    [PluginController("UmbracoApi")]
    [UmbracoApplicationAuthorize(Constants.Applications.Media)]
    public class MediaController : ContentControllerBase
    {
        private readonly IContentSettings _contentSettings;
        private readonly IIOHelper _ioHelper;
        private readonly IMediaTypeService _mediaTypeService;
        private readonly IMediaService _mediaService;
        private readonly IEntityService _entityService;
        private readonly IWebSecurity _webSecurity;
        private readonly UmbracoMapper _umbracoMapper;
        private readonly IDataTypeService _dataTypeService;
        private readonly ILocalizedTextService _localizedTextService;
        private readonly ISqlContext _sqlContext;
        private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
        private readonly IRelationService _relationService;
        public MediaController(
            ICultureDictionary cultureDictionary,
            ILogger logger,
            IShortStringHelper shortStringHelper,
            IEventMessagesFactory eventMessages,
            ILocalizedTextService localizedTextService,
            IContentSettings contentSettings,
            IIOHelper ioHelper,
            IMediaTypeService mediaTypeService,
            IMediaService mediaService,
            IEntityService entityService,
            IWebSecurity webSecurity,
            UmbracoMapper umbracoMapper,
            IDataTypeService dataTypeService,
            ISqlContext sqlContext,
            IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
            IRelationService relationService,
            PropertyEditorCollection propertyEditors,
            IMediaFileSystem mediaFileSystem)
            : base(cultureDictionary, logger, shortStringHelper, eventMessages, localizedTextService)
        {
            _contentSettings = contentSettings;
            _ioHelper = ioHelper;
            _mediaTypeService = mediaTypeService;
            _mediaService = mediaService;
            _entityService = entityService;
            _webSecurity = webSecurity;
            _umbracoMapper = umbracoMapper;
            _dataTypeService = dataTypeService;
            _localizedTextService = localizedTextService;
            _sqlContext = sqlContext;
            _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
            _relationService = relationService;
            _propertyEditors = propertyEditors;
            _mediaFileSystem = mediaFileSystem;
        }

        /// <summary>
        /// Gets an empty content item for the
        /// </summary>
        /// <param name="contentTypeAlias"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        // [OutgoingEditorModelEvent] // TODO introduce when moved to .NET Core
        public MediaItemDisplay GetEmpty(string contentTypeAlias, int parentId)
        {
            var contentType = _mediaTypeService.Get(contentTypeAlias);
            if (contentType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var emptyContent = _mediaService.CreateMedia("", parentId, contentType.Alias, _webSecurity.GetUserId().ResultOr(Constants.Security.SuperUserId));
            var mapped = _umbracoMapper.Map<MediaItemDisplay>(emptyContent);

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
            apps.Add(ListViewContentAppFactory.CreateContentApp(_dataTypeService, _propertyEditors, "recycleBin", "media", Core.Constants.DataTypes.DefaultMediaListView));
            apps[0].Active = true;
            var display = new MediaItemDisplay
            {
                Id = Constants.System.RecycleBinMedia,
                Alias = "recycleBin",
                ParentId = -1,
                Name = _localizedTextService.Localize("general/recycleBin"),
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
        // [OutgoingEditorModelEvent] // TODO introduce when moved to .NET Core
        [EnsureUserPermissionForMedia("id")]
        [DetermineAmbiguousActionByPassingParameters]
        public MediaItemDisplay GetById(int id)
        {
            var foundContent = GetObjectFromRequest(() => _mediaService.GetById(id));

            if (foundContent == null)
            {
                HandleContentNotFound(id);
                //HandleContentNotFound will throw an exception
                return null;
            }
            return _umbracoMapper.Map<MediaItemDisplay>(foundContent);
        }

        /// <summary>
        /// Gets the media item by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // [OutgoingEditorModelEvent] // TODO introduce when moved to .NET Core
        [EnsureUserPermissionForMedia("id")]
        [DetermineAmbiguousActionByPassingParameters]
        public MediaItemDisplay GetById(Guid id)
        {
            var foundContent = GetObjectFromRequest(() => _mediaService.GetById(id));

            if (foundContent == null)
            {
                HandleContentNotFound(id);
                //HandleContentNotFound will throw an exception
                return null;
            }
            return _umbracoMapper.Map<MediaItemDisplay>(foundContent);
        }

        /// <summary>
        /// Gets the media item by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        // [OutgoingEditorModelEvent] // TODO introduce when moved to .NET Core
        [EnsureUserPermissionForMedia("id")]
        [DetermineAmbiguousActionByPassingParameters]
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
        //[FilterAllowedOutgoingMedia(typeof(IEnumerable<MediaItemDisplay>))] // TODO introduce when moved to .NET Core
        public IEnumerable<MediaItemDisplay> GetByIds([FromQuery]int[] ids)
        {
            var foundMedia = _mediaService.GetByIds(ids);
            return foundMedia.Select(media => _umbracoMapper.Map<MediaItemDisplay>(media));
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
            var folderTypes = _mediaTypeService
                .GetAll()
                .Where(x => x.Alias.EndsWith("Folder"))
                .Select(x => x.Id)
                .ToArray();

            if (folderTypes.Length == 0)
            {
                return new PagedResult<ContentItemBasic<ContentPropertyBasic>>(0, pageNumber, pageSize);
            }

            long total;
            var children = _mediaService.GetPagedChildren(id, pageNumber - 1, pageSize, out total,
                //lookup these content types
                _sqlContext.Query<IMedia>().Where(x => folderTypes.Contains(x.ContentTypeId)),
                Ordering.By("Name", Direction.Ascending));

            return new PagedResult<ContentItemBasic<ContentPropertyBasic>>(total, pageNumber, pageSize)
            {
                Items = children.Select(_umbracoMapper.Map<IMedia, ContentItemBasic<ContentPropertyBasic>>)
            };
        }

        /// <summary>
        /// Returns the root media objects
        /// </summary>
        //[FilterAllowedOutgoingMedia(typeof(IEnumerable<ContentItemBasic<ContentPropertyBasic>>))] // TODO introduce when moved to .NET Core
        public IEnumerable<ContentItemBasic<ContentPropertyBasic>> GetRootMedia()
        {
            // TODO: Add permissions check!

            return _mediaService.GetRootMedia()
                           .Select(_umbracoMapper.Map<IMedia, ContentItemBasic<ContentPropertyBasic>>);
        }

        #region GetChildren

        private int[] _userStartNodes;
        private readonly PropertyEditorCollection _propertyEditors;
        private readonly IMediaFileSystem _mediaFileSystem;


        protected int[] UserStartNodes
        {
            get { return _userStartNodes ?? (_userStartNodes = _webSecurity.CurrentUser.CalculateMediaStartNodeIds(_entityService)); }
        }

        /// <summary>
        /// Returns the child media objects - using the entity INT id
        /// </summary>
        [FilterAllowedOutgoingMedia(typeof(IEnumerable<ContentItemBasic<ContentPropertyBasic>>), "Items")] // TODO introduce when moved to .NET Core//[FilterAllowedOutgoingMedia(typeof(IEnumerable<ContentItemBasic<ContentPropertyBasic>>), "Items")]
        [DetermineAmbiguousActionByPassingParameters]
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
                var nodes = _mediaService.GetByIds(UserStartNodes).ToArray();
                if (nodes.Length == 0)
                    return new PagedResult<ContentItemBasic<ContentPropertyBasic>>(0, 0, 0);
                if (pageSize < nodes.Length) pageSize = nodes.Length; // bah
                var pr = new PagedResult<ContentItemBasic<ContentPropertyBasic>>(nodes.Length, pageNumber, pageSize)
                {
                    Items = nodes.Select(_umbracoMapper.Map<IMedia, ContentItemBasic<ContentPropertyBasic>>)
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
                    queryFilter = _sqlContext.Query<IMedia>()
                        .Where(x => x.Name.Contains(filter));
                }

                children = _mediaService
                    .GetPagedChildren(
                        id, (pageNumber - 1), pageSize,
                        out totalChildren,
                        queryFilter,
                        Ordering.By(orderBy, orderDirection, isCustomField: !orderBySystemField)).ToList();
            }
            else
            {
                //better to not use this without paging where possible, currently only the sort dialog does
                children = _mediaService.GetPagedChildren(id,0, int.MaxValue, out var total).ToList();
                totalChildren = children.Count;
            }

            if (totalChildren == 0)
            {
                return new PagedResult<ContentItemBasic<ContentPropertyBasic>>(0, 0, 0);
            }

            var pagedResult = new PagedResult<ContentItemBasic<ContentPropertyBasic>>(totalChildren, pageNumber, pageSize);
            pagedResult.Items = children
                .Select(_umbracoMapper.Map<IMedia, ContentItemBasic<ContentPropertyBasic>>);

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
        [DetermineAmbiguousActionByPassingParameters]
        public PagedResult<ContentItemBasic<ContentPropertyBasic>> GetChildren(Guid id,
           int pageNumber = 0,
           int pageSize = 0,
           string orderBy = "SortOrder",
           Direction orderDirection = Direction.Ascending,
           bool orderBySystemField = true,
           string filter = "")
        {
            var entity = _entityService.Get(id);
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
        [DetermineAmbiguousActionByPassingParameters]
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
                var entity = _entityService.Get(guidUdi.Guid);
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
            var foundMedia = GetObjectFromRequest(() => _mediaService.GetById(id));

            if (foundMedia == null)
            {
                return HandleContentNotFound(id, false);
            }

            //if the current item is in the recycle bin
            if (foundMedia.Trashed == false)
            {
                var moveResult = _mediaService.MoveToRecycleBin(foundMedia, _webSecurity.GetUserId().ResultOr(Constants.Security.SuperUserId));
                if (moveResult == false)
                {
                    //returning an object of INotificationModel will ensure that any pending
                    // notification messages are added to the response.
                    return Request.CreateValidationErrorResponse(new SimpleNotificationModel());
                }
            }
            else
            {
                var deleteResult = _mediaService.Delete(foundMedia, _webSecurity.GetUserId().ResultOr(Constants.Security.SuperUserId));
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

            var moveResult = _mediaService.Move(toMove, move.ParentId, _webSecurity.GetUserId().ResultOr(Constants.Security.SuperUserId));

            if (sourceParentID == destinationParentID)
            {
                return Request.CreateValidationErrorResponse(new SimpleNotificationModel(new BackOfficeNotification("",_localizedTextService.Localize("media/moveToSameFolderFailed"),NotificationStyle.Error)));
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
        // [OutgoingEditorModelEvent] // TODO introduce when moved to .NET Core
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
                    var forDisplay = _umbracoMapper.Map<MediaItemDisplay>(contentItem.PersistedContent);
                    forDisplay.Errors = ModelState.ToErrorDictionary();
                    throw new HttpResponseException(Request.CreateValidationErrorResponse(forDisplay));
                }
            }

            //save the item
            var saveStatus = _mediaService.Save(contentItem.PersistedContent, _webSecurity.GetUserId().ResultOr(Constants.Security.SuperUserId));

            //return the updated model
            var display = _umbracoMapper.Map<MediaItemDisplay>(contentItem.PersistedContent);

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
                            _localizedTextService.Localize("speechBubbles/editMediaSaved"),
                            _localizedTextService.Localize("speechBubbles/editMediaSavedText"));
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
            _mediaService.EmptyRecycleBin(_webSecurity.GetUserId().ResultOr(Constants.Security.SuperUserId));

            return Request.CreateNotificationSuccessResponse(_localizedTextService.Localize("defaultdialogs/recycleBinIsEmpty"));
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

            var mediaService = _mediaService;
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

            var mediaService = _mediaService;

            var f = mediaService.CreateMedia(folder.Name, intParentId, Constants.Conventions.MediaTypes.Folder);
            mediaService.Save(f, _webSecurity.CurrentUser.Id);

            return _umbracoMapper.Map<MediaItemDisplay>(f);
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

            var root = _ioHelper.MapPath(Constants.SystemDirectories.TempFileUploads);
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
            var mediaService = _mediaService;

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
                var fileName = file.Headers.ContentDisposition.FileName.Trim(new[] { '\"' }).TrimEnd();
                var safeFileName = fileName.ToSafeFileName(ShortStringHelper);
                var ext = safeFileName.Substring(safeFileName.LastIndexOf('.') + 1).ToLower();

                if (_contentSettings.IsFileAllowedForUpload(ext))
                {
                    var mediaType = Constants.Conventions.MediaTypes.File;

                    if (result.FormData["contentTypeAlias"] == Constants.Conventions.MediaTypes.AutoSelect)
                    {
                        if (_contentSettings.ImageFileTypes.Contains(ext))
                        {
                            mediaType = Constants.Conventions.MediaTypes.Image;
                        }
                    }
                    else
                    {
                        mediaType = result.FormData["contentTypeAlias"];
                    }

                    var mediaItemName = fileName.ToFriendlyName();

                    var f = mediaService.CreateMedia(mediaItemName, parentId, mediaType, _webSecurity.CurrentUser.Id);

                    var fileInfo = new FileInfo(file.LocalFileName);
                    var fs = fileInfo.OpenReadWithRetry();
                    if (fs == null) throw new InvalidOperationException("Could not acquire file stream");
                    using (fs)
                    {
                        f.SetValue(_mediaFileSystem, ShortStringHelper, _contentTypeBaseServiceProvider, Constants.Conventions.Media.File,fileName, fs);
                    }

                    var saveResult = mediaService.Save(f, _webSecurity.CurrentUser.Id);
                    if (saveResult == false)
                    {
                        AddCancelMessage(tempFiles,
                            message: _localizedTextService.Localize("speechBubbles/operationCancelledText") + " -- " + mediaItemName);
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
                    tempFiles.Notifications.Add(new BackOfficeNotification(
                        _localizedTextService.Localize("speechBubbles/operationFailedHeader"),
                        _localizedTextService.Localize("media/disallowedFileType"),
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
                var children = _mediaService.GetPagedChildren(mediaId, page, pageSize, out total,
                    _sqlContext.Query<IMedia>().Where(x => x.Name == nameToFind));
                foreach (var c in children)
                    return c; //return first one if any are found
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
        private ActionResult<int> GetParentIdAsInt(string parentId, bool validatePermissions)
        {
            int intParentId;

            // test for udi
            if (UdiParser.TryParse(parentId, out GuidUdi parentUdi))
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
                    var entity = _entityService.Get(idGuid);
                    if (entity != null)
                    {
                        intParentId = entity.Id;
                    }
                    else
                    {
                        return NotFound("The passed id doesn't exist");
                    }
                }
                else
                {
                    throw HttpResponseException.CreateValidationErrorResponse("The request was not formatted correctly, the parentId is not an integer, Guid or UDI");
                }
            }

            //ensure the user has access to this folder by parent id!
            if (validatePermissions && CheckPermissions(
                    new Dictionary<object, object>(),
                    _webSecurity.CurrentUser,
                    _mediaService,
                    _entityService,
                    intParentId) == false)
            {
                throw new HttpResponseException(
                    HttpStatusCode.Forbidden,
                    new SimpleNotificationModel(new BackOfficeNotification(
                        _localizedTextService.Localize("speechBubbles/operationFailedHeader"),
                        _localizedTextService.Localize("speechBubbles/invalidUserPermissionsText"),
                        NotificationStyle.Warning)));
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

            var mediaService = _mediaService;
            var toMove = mediaService.GetById(model.Id);
            if (toMove == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            if (model.ParentId < 0)
            {
                //cannot move if the content item is not allowed at the root unless there are
                //none allowed at root (in which case all should be allowed at root)
                var mediaTypeService = _mediaTypeService;
                if (toMove.ContentType.AllowedAsRoot == false && mediaTypeService.GetAll().Any(ct => ct.AllowedAsRoot))
                {
                    var notificationModel = new SimpleNotificationModel();
                    notificationModel.AddErrorNotification(_localizedTextService.Localize("moveOrCopy/notAllowedAtRoot"), "");
                    throw HttpResponseException.CreateValidationErrorResponse(notificationModel);
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
                var parentContentType = _mediaTypeService.Get(parent.ContentTypeId);
                if (parentContentType.AllowedContentTypes.Select(x => x.Id).ToArray()
                    .Any(x => x.Value == toMove.ContentType.Id) == false)
                {
                    var notificationModel = new SimpleNotificationModel();
                    notificationModel.AddErrorNotification(_localizedTextService.Localize("moveOrCopy/notAllowedByContentType"), "");
                    throw HttpResponseException.CreateValidationErrorResponse(notificationModel);
                }

                // Check on paths
                if ((string.Format(",{0},", parent.Path)).IndexOf(string.Format(",{0},", toMove.Id), StringComparison.Ordinal) > -1)
                {
                    var notificationModel = new SimpleNotificationModel();
                    notificationModel.AddErrorNotification(_localizedTextService.Localize("moveOrCopy/notAllowedByPath"), "");
                    throw HttpResponseException.CreateValidationErrorResponse(notificationModel);
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
        internal static bool CheckPermissions(IDictionary<object, object> storage, IUser user, IMediaService mediaService, IEntityService entityService, int nodeId, IMedia media = null)
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
                ? user.HasMediaRootAccess(entityService)
                : (nodeId == Constants.System.RecycleBinMedia)
                    ? user.HasMediaBinAccess(entityService)
                    : user.HasPathAccess(media, entityService);

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

            var relations = _relationService.GetPagedParentEntitiesByChildId(id, pageNumber - 1, pageSize, out var totalRecords, objectType);

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
