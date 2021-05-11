using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.ContentApps;
using Umbraco.Cms.Core.Dictionary;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.BackOffice.ActionResults;
using Umbraco.Cms.Web.BackOffice.Authorization;
using Umbraco.Cms.Web.BackOffice.Extensions;
using Umbraco.Cms.Web.BackOffice.Filters;
using Umbraco.Cms.Web.BackOffice.ModelBinders;
using Umbraco.Cms.Web.Common.ActionsResults;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Controllers
{
    /// <remarks>
    /// This controller is decorated with the UmbracoApplicationAuthorizeAttribute which means that any user requesting
    /// access to ALL of the methods on this controller will need access to the media application.
    /// </remarks>
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    [Authorize(Policy = AuthorizationPolicies.SectionAccessMedia)]
    [ParameterSwapControllerActionSelector(nameof(GetById), "id", typeof(int), typeof(Guid), typeof(Udi))]
    [ParameterSwapControllerActionSelector(nameof(GetChildren), "id", typeof(int), typeof(Guid), typeof(Udi))]
    public class MediaController : ContentControllerBase
    {
        private readonly IShortStringHelper _shortStringHelper;
        private readonly ContentSettings _contentSettings;
        private readonly IMediaTypeService _mediaTypeService;
        private readonly IMediaService _mediaService;
        private readonly IEntityService _entityService;
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly IUmbracoMapper _umbracoMapper;
        private readonly IDataTypeService _dataTypeService;
        private readonly ILocalizedTextService _localizedTextService;
        private readonly ISqlContext _sqlContext;
        private readonly IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
        private readonly IRelationService _relationService;
        private readonly IImageUrlGenerator _imageUrlGenerator;
        private readonly IJsonSerializer _serializer;
        private readonly IAuthorizationService _authorizationService;
        private readonly AppCaches _appCaches;
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
            IBackOfficeSecurityAccessor backofficeSecurityAccessor,
            IUmbracoMapper umbracoMapper,
            IDataTypeService dataTypeService,
            ISqlContext sqlContext,
            IContentTypeBaseServiceProvider contentTypeBaseServiceProvider,
            IRelationService relationService,
            PropertyEditorCollection propertyEditors,
            MediaFileManager mediaFileManager,
            IHostingEnvironment hostingEnvironment,
            IImageUrlGenerator imageUrlGenerator,
            IJsonSerializer serializer,
            IAuthorizationService authorizationService,
            AppCaches appCaches)
            : base(cultureDictionary, loggerFactory, shortStringHelper, eventMessages, localizedTextService, serializer)
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
            _mediaFileManager = mediaFileManager;
            _hostingEnvironment = hostingEnvironment;
            _logger = loggerFactory.CreateLogger<MediaController>();
            _imageUrlGenerator = imageUrlGenerator;
            _serializer = serializer;
            _authorizationService = authorizationService;
            _appCaches = appCaches;
        }

        /// <summary>
        /// Gets an empty content item for the
        /// </summary>
        /// <param name="contentTypeAlias"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        [OutgoingEditorModelEvent]
        public ActionResult<MediaItemDisplay> GetEmpty(string contentTypeAlias, int parentId)
        {
            var contentType = _mediaTypeService.Get(contentTypeAlias);
            if (contentType == null)
            {
                return NotFound();
            }

            var emptyContent = _mediaService.CreateMedia("", parentId, contentType.Alias, _backofficeSecurityAccessor.BackOfficeSecurity.GetUserId().ResultOr(Constants.Security.SuperUserId));
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
            apps.Add(ListViewContentAppFactory.CreateContentApp(_dataTypeService, _propertyEditors, "recycleBin", "media", Constants.DataTypes.DefaultMediaListView));
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
        [OutgoingEditorModelEvent]
        [Authorize(Policy = AuthorizationPolicies.MediaPermissionPathById)]
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
        [OutgoingEditorModelEvent]
        [Authorize(Policy = AuthorizationPolicies.MediaPermissionPathById)]
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
        [OutgoingEditorModelEvent]
        [Authorize(Policy = AuthorizationPolicies.MediaPermissionPathById)]
        public ActionResult<MediaItemDisplay> GetById(Udi id)
        {
            var guidUdi = id as GuidUdi;
            if (guidUdi != null)
            {
                return GetById(guidUdi.Guid);
            }

            return NotFound();
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
        private readonly MediaFileManager _mediaFileManager;
        private readonly IHostingEnvironment _hostingEnvironment;


        protected int[] UserStartNodes
        {
            get { return _userStartNodes ?? (_userStartNodes = _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.CalculateMediaStartNodeIds(_entityService, _appCaches)); }
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
        public ActionResult<PagedResult<ContentItemBasic<ContentPropertyBasic>>> GetChildren(Guid id,
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

            return NotFound();
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
        public ActionResult<PagedResult<ContentItemBasic<ContentPropertyBasic>>> GetChildren(Udi id,
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

            return NotFound();
        }

        #endregion

        /// <summary>
        /// Moves an item to the recycle bin, if it is already there then it will permanently delete it
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = AuthorizationPolicies.MediaPermissionPathById)]
        [HttpPost]
        public IActionResult DeleteById(int id)
        {
            var foundMedia = GetObjectFromRequest(() => _mediaService.GetById(id));

            if (foundMedia == null)
            {
                return HandleContentNotFound(id);
            }

            //if the current item is in the recycle bin
            if (foundMedia.Trashed == false)
            {
                var moveResult = _mediaService.MoveToRecycleBin(foundMedia, _backofficeSecurityAccessor.BackOfficeSecurity.GetUserId().ResultOr(Constants.Security.SuperUserId));
                if (moveResult == false)
                {
                    //returning an object of INotificationModel will ensure that any pending
                    // notification messages are added to the response.
                    return new ValidationErrorResult(new SimpleNotificationModel());
                }
            }
            else
            {
                var deleteResult = _mediaService.Delete(foundMedia, _backofficeSecurityAccessor.BackOfficeSecurity.GetUserId().ResultOr(Constants.Security.SuperUserId));
                if (deleteResult == false)
                {
                    //returning an object of INotificationModel will ensure that any pending
                    // notification messages are added to the response.
                    return new ValidationErrorResult(new SimpleNotificationModel());
                }
            }

            return Ok();
        }

        /// <summary>
        /// Change the sort order for media
        /// </summary>
        /// <param name="move"></param>
        /// <returns></returns>
        public async Task<IActionResult> PostMove(MoveOrCopy move)
        {
            // Authorize...
            var requirement = new MediaPermissionsResourceRequirement();
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, new MediaPermissionsResource(_mediaService.GetById(move.Id)), requirement);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            var toMoveResult = ValidateMoveOrCopy(move);
            var toMove = toMoveResult.Value;
            if (toMove is null && toMoveResult is IConvertToActionResult convertToActionResult)
            {
                return convertToActionResult.Convert();
            }

            var destinationParentID = move.ParentId;
            var sourceParentID = toMove.ParentId;

            var moveResult = _mediaService.Move(toMove, move.ParentId, _backofficeSecurityAccessor.BackOfficeSecurity.GetUserId().ResultOr(Constants.Security.SuperUserId));

            if (sourceParentID == destinationParentID)
            {
                return new ValidationErrorResult(new SimpleNotificationModel(new BackOfficeNotification("",_localizedTextService.Localize("media/moveToSameFolderFailed"),NotificationStyle.Error)));
            }
            if (moveResult == false)
            {
                return new ValidationErrorResult(new SimpleNotificationModel());
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
        [OutgoingEditorModelEvent]
        public ActionResult<MediaItemDisplay> PostSave(
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
                    return new ValidationErrorResult(forDisplay);
                }
            }

            //save the item
            var saveStatus = _mediaService.Save(contentItem.PersistedContent, _backofficeSecurityAccessor.BackOfficeSecurity.GetUserId().ResultOr(Constants.Security.SuperUserId));

            //return the updated model
            var display = _umbracoMapper.Map<MediaItemDisplay>(contentItem.PersistedContent);

            //lastly, if it is not valid, add the model state to the outgoing object and throw a 403
            if (!ModelState.IsValid)
            {
                display.Errors = ModelState.ToErrorDictionary();
                return new ValidationErrorResult(display, StatusCodes.Status403Forbidden);
            }

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
                            return new ValidationErrorResult(display);
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
            _mediaService.EmptyRecycleBin(_backofficeSecurityAccessor.BackOfficeSecurity.GetUserId().ResultOr(Constants.Security.SuperUserId));

            return new UmbracoNotificationSuccessResponse(_localizedTextService.Localize("defaultdialogs/recycleBinIsEmpty"));
        }

        /// <summary>
        /// Change the sort order for media
        /// </summary>
        /// <param name="sorted"></param>
        /// <returns></returns>
        public async Task<IActionResult> PostSort(ContentSortOrder sorted)
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

            // Authorize...
            var requirement = new MediaPermissionsResourceRequirement();
            var resource = new MediaPermissionsResource(sorted.ParentId);
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, resource, requirement);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            var sortedMedia = new List<IMedia>();
            try
            {
                sortedMedia.AddRange(sorted.IdSortOrder.Select(_mediaService.GetById));

                // Save Media with new sort order and update content xml in db accordingly
                if (_mediaService.Sort(sortedMedia) == false)
                {
                    _logger.LogWarning("Media sorting failed, this was probably caused by an event being cancelled");
                    return new ValidationErrorResult("Media sorting failed, this was probably caused by an event being cancelled");
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not update media sort order");
                throw;
            }
        }

        public async Task<ActionResult<MediaItemDisplay>> PostAddFolder(PostedFolder folder)
        {
            var parentIdResult = await GetParentIdAsIntAsync(folder.ParentId, validatePermissions:true);
            if (!(parentIdResult.Result is null))
            {
                return new ActionResult<MediaItemDisplay>(parentIdResult.Result);
            }
            var parentId = parentIdResult.Value;
            if (!parentId.HasValue)
            {
                return NotFound("The passed id doesn't exist");
            }

            var f = _mediaService.CreateMedia(folder.Name, parentId.Value, Constants.Conventions.MediaTypes.Folder);
            _mediaService.Save(f, _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.Id);

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
            var parentIdResult = await GetParentIdAsIntAsync(currentFolder, validatePermissions:true);
            if (!(parentIdResult.Result is null))
            {
                return parentIdResult.Result;
            }

            var parentId = parentIdResult.Value;
            if (!parentId.HasValue)
            {
                return NotFound("The passed id doesn't exist");
            }

            var tempFiles = new PostedFiles();


            //in case we pass a path with a folder in it, we will create it and upload media to it.
            if (!string.IsNullOrEmpty(path))
            {

                var folders = path.Split(Constants.CharArrays.ForwardSlash);

                for (int i = 0; i < folders.Length - 1; i++)
                {
                    var folderName = folders[i];
                    IMedia folderMediaItem;

                    //if uploading directly to media root and not a subfolder
                    if (parentId == -1)
                    {
                        //look for matching folder
                        folderMediaItem =
                            _mediaService.GetRootMedia().FirstOrDefault(x => x.Name == folderName && x.ContentType.Alias == Constants.Conventions.MediaTypes.Folder);
                        if (folderMediaItem == null)
                        {
                            //if null, create a folder
                            folderMediaItem = _mediaService.CreateMedia(folderName, -1, Constants.Conventions.MediaTypes.Folder);
                            _mediaService.Save(folderMediaItem);
                        }
                    }
                    else
                    {
                        //get current parent
                        var mediaRoot = _mediaService.GetById(parentId.Value);

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
                            folderMediaItem = _mediaService.CreateMedia(folderName, mediaRoot, Constants.Conventions.MediaTypes.Folder);
                            _mediaService.Save(folderMediaItem);
                        }
                    }
                    //set the media root to the folder id so uploaded files will end there.
                    parentId = folderMediaItem.Id;
                }
            }

            //get the files
            foreach (var formFile in file)
            {
                var fileName =  formFile.FileName.Trim(Constants.CharArrays.DoubleQuote).TrimEnd();
                var safeFileName = fileName.ToSafeFileName(ShortStringHelper);
                var ext = safeFileName.Substring(safeFileName.LastIndexOf('.') + 1).ToLower();

                if (_contentSettings.IsFileAllowedForUpload(ext))
                {
                    var mediaType = Constants.Conventions.MediaTypes.File;

                    if (contentTypeAlias == Constants.Conventions.MediaTypes.AutoSelect)
                    {
                        var mediaTypes = _mediaTypeService.GetAll();
                        // Look up MediaTypes
                        foreach (var mediaTypeItem in mediaTypes)
                        {
                            var fileProperty = mediaTypeItem.CompositionPropertyTypes.FirstOrDefault(x => x.Alias == "umbracoFile");
                            if (fileProperty != null) {
                                var dataTypeKey = fileProperty.DataTypeKey;
                                var dataType = _dataTypeService.GetDataType(dataTypeKey);

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
                        if (mediaType == Constants.Conventions.MediaTypes.File && _imageUrlGenerator.SupportedImageFileTypes.Contains(ext))
                        {
                            mediaType = Constants.Conventions.MediaTypes.Image;
                        }
                    }
                    else
                    {
                        mediaType = contentTypeAlias;
                    }

                    var mediaItemName = fileName.ToFriendlyName();

                    var f = _mediaService.CreateMedia(mediaItemName, parentId.Value, mediaType, _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.Id);


                    await using (var stream = formFile.OpenReadStream())
                    {
                        f.SetValue(_mediaFileManager,_shortStringHelper, _contentTypeBaseServiceProvider, _serializer, Constants.Conventions.Media.File,fileName, stream);
                    }


                    var saveResult = _mediaService.Save(f, _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.Id);
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
        private async Task<ActionResult<int?>> GetParentIdAsIntAsync(string parentId, bool validatePermissions)
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
                    return new ValidationErrorResult("The request was not formatted correctly, the parentId is not an integer, Guid or UDI");
                }
            }

            // Authorize...
            //ensure the user has access to this folder by parent id!
            if (validatePermissions)
            {
                var requirement = new MediaPermissionsResourceRequirement();
                var authorizationResult = await _authorizationService.AuthorizeAsync(User, new MediaPermissionsResource(_mediaService.GetById(intParentId)), requirement);
                if (!authorizationResult.Succeeded)
                {
                    return new ValidationErrorResult(
                    new SimpleNotificationModel(new BackOfficeNotification(
                        _localizedTextService.Localize("speechBubbles/operationFailedHeader"),
                        _localizedTextService.Localize("speechBubbles/invalidUserPermissionsText"),
                        NotificationStyle.Warning)),
                        StatusCodes.Status403Forbidden);
                }
            }

            return intParentId;
        }

        /// <summary>
        /// Ensures the item can be moved/copied to the new location
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private ActionResult<IMedia> ValidateMoveOrCopy(MoveOrCopy model)
        {
            if (model == null)
            {
                return NotFound();
            }


            var toMove = _mediaService.GetById(model.Id);
            if (toMove == null)
            {
                return NotFound();
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
                    return new ValidationErrorResult(notificationModel);
                }
            }
            else
            {
                var parent = _mediaService.GetById(model.ParentId);
                if (parent == null)
                {
                    return NotFound();
                }

                //check if the item is allowed under this one
                var parentContentType = _mediaTypeService.Get(parent.ContentTypeId);
                if (parentContentType.AllowedContentTypes.Select(x => x.Id).ToArray()
                    .Any(x => x.Value == toMove.ContentType.Id) == false)
                {
                    var notificationModel = new SimpleNotificationModel();
                    notificationModel.AddErrorNotification(_localizedTextService.Localize("moveOrCopy/notAllowedByContentType"), "");
                    return new ValidationErrorResult(notificationModel);
                }

                // Check on paths
                if ((string.Format(",{0},", parent.Path)).IndexOf(string.Format(",{0},", toMove.Id), StringComparison.Ordinal) > -1)
                {
                    var notificationModel = new SimpleNotificationModel();
                    notificationModel.AddErrorNotification(_localizedTextService.Localize("moveOrCopy/notAllowedByPath"), "");
                    return new ValidationErrorResult(notificationModel);
                }
            }

            return new ActionResult<IMedia>(toMove);
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
