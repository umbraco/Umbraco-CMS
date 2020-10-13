﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Events;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Mapping;
using Umbraco.Core.Media;
using Umbraco.Core.Models;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Models.Validation;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Extensions;
using Umbraco.Web.BackOffice.Filters;
using Umbraco.Web.BackOffice.ModelBinders;
using Umbraco.Web.Common.ActionResults;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Common.Exceptions;
using Umbraco.Web.ContentApps;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.BackOffice.Controllers
{
    /// <remarks>
    /// This controller is decorated with the UmbracoApplicationAuthorizeAttribute which means that any user requesting
    /// access to ALL of the methods on this controller will need access to the media application.
    /// </remarks>
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    [UmbracoApplicationAuthorize(Constants.Applications.Media)]
    public class MediaController : ContentControllerBase
    {
        private readonly IShortStringHelper _shortStringHelper;
        private readonly ContentSettings _contentSettings;
        private readonly IMediaTypeService _mediaTypeService;
        private readonly IMediaService _mediaService;
        private readonly IEntityService _entityService;
        private readonly IBackofficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly UmbracoMapper _umbracoMapper;
        private readonly IDataTypeService _dataTypeService;
        private readonly ILocalizedTextService _localizedTextService;
        private readonly ISqlContext _sqlContext;
        private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
        private readonly IRelationService _relationService;
        private readonly IImageUrlGenerator _imageUrlGenerator;
        private readonly ILogger<MediaController> _logger;

        public MediaController(
            ICultureDictionary cultureDictionary,
            ILoggerFactory loggerFactory,
            IShortStringHelper shortStringHelper,
            IEventMessagesFactory eventMessages,
            ILocalizedTextService localizedTextService,
            IOptions<ContentSettings> contentSettings,
            IMediaTypeService mediaTypeService,
            IMediaService mediaService,
            IEntityService entityService,
            IBackofficeSecurityAccessor backofficeSecurityAccessor,
            UmbracoMapper umbracoMapper,
            IDataTypeService dataTypeService,
            ISqlContext sqlContext,
            IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
            IRelationService relationService,
            PropertyEditorCollection propertyEditors,
            IMediaFileSystem mediaFileSystem,
            IHostingEnvironment hostingEnvironment,
            IImageUrlGenerator imageUrlGenerator)
            : base(cultureDictionary, loggerFactory, shortStringHelper, eventMessages, localizedTextService)
        {
            _shortStringHelper = shortStringHelper;
            _contentSettings = contentSettings.Value;
            _mediaTypeService = mediaTypeService;
            _mediaService = mediaService;
            _entityService = entityService;
            _backofficeSecurityAccessor = backofficeSecurityAccessor;
            _umbracoMapper = umbracoMapper;
            _dataTypeService = dataTypeService;
            _localizedTextService = localizedTextService;
            _sqlContext = sqlContext;
            _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
            _relationService = relationService;
            _propertyEditors = propertyEditors;
            _mediaFileSystem = mediaFileSystem;
            _hostingEnvironment = hostingEnvironment;
            _logger = loggerFactory.CreateLogger<MediaController>();
            _imageUrlGenerator = imageUrlGenerator;
        }

        /// <summary>
        /// Gets an empty content item for the
        /// </summary>
        /// <param name="contentTypeAlias"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        [TypeFilter(typeof(OutgoingEditorModelEventAttribute))]
        public MediaItemDisplay GetEmpty(string contentTypeAlias, int parentId)
        {
            var contentType = _mediaTypeService.Get(contentTypeAlias);
            if (contentType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var emptyContent = _mediaService.CreateMedia("", parentId, contentType.Alias, _backofficeSecurityAccessor.BackofficeSecurity.GetUserId().ResultOr(Constants.Security.SuperUserId));
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
        [TypeFilter(typeof(OutgoingEditorModelEventAttribute))]
        [EnsureUserPermissionForMedia("id")]
        [DetermineAmbiguousActionByPassingParameters]
        public MediaItemDisplay GetById(int id)
        {
            var foundMedia = GetObjectFromRequest(() => _mediaService.GetById(id));

            if (foundMedia == null)
            {
                HandleContentNotFound(id);
                //HandleContentNotFound will throw an exception
                return null;
            }
            return _umbracoMapper.Map<MediaItemDisplay>(foundMedia);
        }

        /// <summary>
        /// Gets the media item by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [TypeFilter(typeof(OutgoingEditorModelEventAttribute))]
        [EnsureUserPermissionForMedia("id")]
        [DetermineAmbiguousActionByPassingParameters]
        public MediaItemDisplay GetById(Guid id)
        {
            var foundMedia = GetObjectFromRequest(() => _mediaService.GetById(id));

            if (foundMedia == null)
            {
                HandleContentNotFound(id);
                //HandleContentNotFound will throw an exception
                return null;
            }
            return _umbracoMapper.Map<MediaItemDisplay>(foundMedia);
        }

        /// <summary>
        /// Gets the media item by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [TypeFilter(typeof(OutgoingEditorModelEventAttribute))]
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
        [FilterAllowedOutgoingMedia(typeof(IEnumerable<MediaItemDisplay>))]
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
        [FilterAllowedOutgoingMedia(typeof(IEnumerable<ContentItemBasic<ContentPropertyBasic>>))]
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
        private readonly IHostingEnvironment _hostingEnvironment;


        protected int[] UserStartNodes
        {
            get { return _userStartNodes ?? (_userStartNodes = _backofficeSecurityAccessor.BackofficeSecurity.CurrentUser.CalculateMediaStartNodeIds(_entityService)); }
        }

        /// <summary>
        /// Returns the child media objects - using the entity INT id
        /// </summary>
        [FilterAllowedOutgoingMedia(typeof(IEnumerable<ContentItemBasic<ContentPropertyBasic>>), "Items")]
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
        public IActionResult DeleteById(int id)
        {
            var foundMedia = GetObjectFromRequest(() => _mediaService.GetById(id));

            if (foundMedia == null)
            {
                return HandleContentNotFound(id, false);
            }

            //if the current item is in the recycle bin
            if (foundMedia.Trashed == false)
            {
                var moveResult = _mediaService.MoveToRecycleBin(foundMedia, _backofficeSecurityAccessor.BackofficeSecurity.GetUserId().ResultOr(Constants.Security.SuperUserId));
                if (moveResult == false)
                {
                    //returning an object of INotificationModel will ensure that any pending
                    // notification messages are added to the response.
                    throw HttpResponseException.CreateValidationErrorResponse(new SimpleNotificationModel());
                }
            }
            else
            {
                var deleteResult = _mediaService.Delete(foundMedia, _backofficeSecurityAccessor.BackofficeSecurity.GetUserId().ResultOr(Constants.Security.SuperUserId));
                if (deleteResult == false)
                {
                    //returning an object of INotificationModel will ensure that any pending
                    // notification messages are added to the response.
                    throw HttpResponseException.CreateValidationErrorResponse(new SimpleNotificationModel());
                }
            }

            return Ok();
        }

        /// <summary>
        /// Change the sort order for media
        /// </summary>
        /// <param name="move"></param>
        /// <returns></returns>
        [EnsureUserPermissionForMedia("move.Id")]
        public IActionResult PostMove(MoveOrCopy move)
        {
            var toMove = ValidateMoveOrCopy(move);
            var destinationParentID = move.ParentId;
            var sourceParentID = toMove.ParentId;

            var moveResult = _mediaService.Move(toMove, move.ParentId, _backofficeSecurityAccessor.BackofficeSecurity.GetUserId().ResultOr(Constants.Security.SuperUserId));

            if (sourceParentID == destinationParentID)
            {
                throw HttpResponseException.CreateValidationErrorResponse(new SimpleNotificationModel(new BackOfficeNotification("",_localizedTextService.Localize("media/moveToSameFolderFailed"),NotificationStyle.Error)));
            }
            if (moveResult == false)
            {
                throw HttpResponseException.CreateValidationErrorResponse(new SimpleNotificationModel());
            }
            else
            {
                return Content(toMove.Path, MediaTypeNames.Text.Plain, Encoding.UTF8);
            }
        }

        /// <summary>
        /// Saves content
        /// </summary>
        /// <returns></returns>
        [FileUploadCleanupFilter]
        [MediaItemSaveValidation]
        [TypeFilter(typeof(OutgoingEditorModelEventAttribute))]
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
                    throw HttpResponseException.CreateValidationErrorResponse(forDisplay);
                }
            }

            //save the item
            var saveStatus = _mediaService.Save(contentItem.PersistedContent, _backofficeSecurityAccessor.BackofficeSecurity.GetUserId().ResultOr(Constants.Security.SuperUserId));

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
                            throw HttpResponseException.CreateValidationErrorResponse(display);
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
        public IActionResult EmptyRecycleBin()
        {
            _mediaService.EmptyRecycleBin(_backofficeSecurityAccessor.BackofficeSecurity.GetUserId().ResultOr(Constants.Security.SuperUserId));

            return new UmbracoNotificationSuccessResponse(_localizedTextService.Localize("defaultdialogs/recycleBinIsEmpty"));
        }

        /// <summary>
        /// Change the sort order for media
        /// </summary>
        /// <param name="sorted"></param>
        /// <returns></returns>
        [EnsureUserPermissionForMedia("sorted.ParentId")]
        public IActionResult PostSort(ContentSortOrder sorted)
        {
            if (sorted == null)
            {
                return NotFound();
            }

            //if there's nothing to sort just return ok
            if (sorted.IdSortOrder.Length == 0)
            {
                return Ok();
            }

            var mediaService = _mediaService;
            var sortedMedia = new List<IMedia>();
            try
            {
                sortedMedia.AddRange(sorted.IdSortOrder.Select(mediaService.GetById));

                // Save Media with new sort order and update content xml in db accordingly
                if (mediaService.Sort(sortedMedia) == false)
                {
                    _logger.LogWarning("Media sorting failed, this was probably caused by an event being cancelled");
                    throw HttpResponseException.CreateValidationErrorResponse("Media sorting failed, this was probably caused by an event being cancelled");
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not update media sort order");
                throw;
            }
        }

        public ActionResult<MediaItemDisplay> PostAddFolder(PostedFolder folder)
        {
            var parentId = GetParentIdAsInt(folder.ParentId, validatePermissions:true);
            if (!parentId.HasValue)
            {
                return NotFound("The passed id doesn't exist");
            }


            var mediaService = _mediaService;

            var f = mediaService.CreateMedia(folder.Name, parentId.Value, Constants.Conventions.MediaTypes.Folder);
            mediaService.Save(f, _backofficeSecurityAccessor.BackofficeSecurity.CurrentUser.Id);

            return _umbracoMapper.Map<MediaItemDisplay>(f);
        }

        /// <summary>
        /// Used to submit a media file
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// We cannot validate this request with attributes (nicely) due to the nature of the multi-part for data.
        /// </remarks>
        public async Task<IActionResult> PostAddFile([FromForm]string path, [FromForm]string currentFolder, [FromForm]string contentTypeAlias, List<IFormFile> file)
        {
            var root = _hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.TempFileUploads);
            //ensure it exists
            Directory.CreateDirectory(root);

            //must have a file
            if (file.Count == 0)
            {
                return NotFound();
            }

            //get the string json from the request
            var parentId = GetParentIdAsInt(currentFolder, validatePermissions: true);
            if (!parentId.HasValue)
            {
                return NotFound("The passed id doesn't exist");
            }
            var tempFiles = new PostedFiles();
            var mediaService = _mediaService;

            //in case we pass a path with a folder in it, we will create it and upload media to it.
            if (!string.IsNullOrEmpty(path))
            {

                var folders = path.Split('/');

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
                        var mediaRoot = mediaService.GetById(parentId.Value);

                        //if the media root is null, something went wrong, we'll abort
                        if (mediaRoot == null)
                            return Problem(
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
            foreach (var formFile in file)
            {
                var fileName =  formFile.FileName.Trim(new[] { '\"' }).TrimEnd();
                var safeFileName = fileName.ToSafeFileName(ShortStringHelper);
                var ext = safeFileName.Substring(safeFileName.LastIndexOf('.') + 1).ToLower();

                if (_contentSettings.IsFileAllowedForUpload(ext))
                {
                    var mediaType = Constants.Conventions.MediaTypes.File;

                    if (contentTypeAlias == Constants.Conventions.MediaTypes.AutoSelect)
                    {
                        if (_imageUrlGenerator.SupportedImageFileTypes.Contains(ext))
                        {
                            mediaType = Constants.Conventions.MediaTypes.Image;
                        }
                    }
                    else
                    {
                        mediaType = contentTypeAlias;
                    }

                    var mediaItemName = fileName.ToFriendlyName();

                    var f = mediaService.CreateMedia(mediaItemName, parentId.Value, mediaType, _backofficeSecurityAccessor.BackofficeSecurity.CurrentUser.Id);


                    await using (var stream = formFile.OpenReadStream())
                    {
                        f.SetValue(_mediaFileSystem,_shortStringHelper, _contentTypeBaseServiceProvider, Constants.Conventions.Media.File,fileName, stream);
                    }


                    var saveResult = mediaService.Save(f, _backofficeSecurityAccessor.BackofficeSecurity.CurrentUser.Id);
                    if (saveResult == false)
                    {
                        AddCancelMessage(tempFiles,
                            message: _localizedTextService.Localize("speechBubbles/operationCancelledText") + " -- " + mediaItemName);
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
            if (HttpContext.Request.Query.Any(x => x.Key == "origin"))
            {
                var origin = HttpContext.Request.Query.First(x => x.Key == "origin");
                if (origin.Value == "blueimp")
                {
                    return new JsonResult(tempFiles); //Don't output the angular xsrf stuff, blue imp doesn't like that
                }
            }

            return Ok();
        }

        private IMedia FindInChildren(int mediaId, string nameToFind, string contentTypeAlias)
        {
            const int pageSize = 500;
            var page = 0;
            var total = long.MaxValue;
            while (page * pageSize < total)
            {
                var children = _mediaService.GetPagedChildren(mediaId, page++, pageSize, out total,
                    _sqlContext.Query<IMedia>().Where(x => x.Name == nameToFind));
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
        private int? GetParentIdAsInt(string parentId, bool validatePermissions)
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
                        return null;
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
                    _backofficeSecurityAccessor.BackofficeSecurity.CurrentUser,
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
