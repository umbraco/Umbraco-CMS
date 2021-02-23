using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.ContentApps;
using Umbraco.Cms.Core.Dictionary;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Routing;
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
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Web.BackOffice.Controllers
{
    /// <summary>
    /// The API controller used for editing content
    /// </summary>
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    [Authorize(Policy = AuthorizationPolicies.TreeAccessDocuments)]
    [ParameterSwapControllerActionSelector(nameof(GetById), "id", typeof(int), typeof(Guid), typeof(Udi))]
    [ParameterSwapControllerActionSelector(nameof(GetNiceUrl), "id", typeof(int), typeof(Guid), typeof(Udi))]
    public class ContentController : ContentControllerBase
    {
        private readonly PropertyEditorCollection _propertyEditors;
        private readonly IContentService _contentService;
        private readonly ILocalizedTextService _localizedTextService;
        private readonly IUserService _userService;
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;
        private readonly IEntityService _entityService;
        private readonly IContentTypeService _contentTypeService;
        private readonly UmbracoMapper _umbracoMapper;
        private readonly IPublishedUrlProvider _publishedUrlProvider;
        private readonly IPublicAccessService _publicAccessService;
        private readonly IDomainService _domainService;
        private readonly IDataTypeService _dataTypeService;
        private readonly ILocalizationService _localizationService;
        private readonly IMemberService _memberService;
        private readonly IFileService _fileService;
        private readonly INotificationService _notificationService;
        private readonly ActionCollection _actionCollection;
        private readonly IMemberGroupService _memberGroupService;
        private readonly ISqlContext _sqlContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly Lazy<IDictionary<string, ILanguage>> _allLangs;
        private readonly ILogger<ContentController> _logger;

        public object Domains { get; private set; }

        public ContentController(
            ICultureDictionary cultureDictionary,
            ILoggerFactory loggerFactory,
            IShortStringHelper shortStringHelper,
            IEventMessagesFactory eventMessages,
            ILocalizedTextService localizedTextService,
            PropertyEditorCollection propertyEditors,
            IContentService contentService,
            IUserService userService,
            IBackOfficeSecurityAccessor backofficeSecurityAccessor,
            IEntityService entityService,
            IContentTypeService contentTypeService,
            UmbracoMapper umbracoMapper,
            IPublishedUrlProvider publishedUrlProvider,
            IPublicAccessService publicAccessService,
            IDomainService domainService,
            IDataTypeService dataTypeService,
            ILocalizationService localizationService,
            IMemberService memberService,
            IFileService fileService,
            INotificationService notificationService,
            ActionCollection actionCollection,
            IMemberGroupService memberGroupService,
            ISqlContext sqlContext,
            IJsonSerializer serializer,
            IAuthorizationService authorizationService)
            : base(cultureDictionary, loggerFactory, shortStringHelper, eventMessages, localizedTextService, serializer)
        {
            _propertyEditors = propertyEditors;
            _contentService = contentService;
            _localizedTextService = localizedTextService;
            _userService = userService;
            _backofficeSecurityAccessor = backofficeSecurityAccessor;
            _entityService = entityService;
            _contentTypeService = contentTypeService;
            _umbracoMapper = umbracoMapper;
            _publishedUrlProvider = publishedUrlProvider;
            _publicAccessService = publicAccessService;
            _domainService = domainService;
            _dataTypeService = dataTypeService;
            _localizationService = localizationService;
            _memberService = memberService;
            _fileService = fileService;
            _notificationService = notificationService;
            _actionCollection = actionCollection;
            _memberGroupService = memberGroupService;
            _sqlContext = sqlContext;
            _authorizationService = authorizationService;
            _logger = loggerFactory.CreateLogger<ContentController>();

            _allLangs = new Lazy<IDictionary<string, ILanguage>>(() => _localizationService.GetAllLanguages().ToDictionary(x => x.IsoCode, x => x, StringComparer.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Return content for the specified ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [FilterAllowedOutgoingContent(typeof(IEnumerable<ContentItemDisplay>))]
        public IEnumerable<ContentItemDisplay> GetByIds([FromQuery] int[] ids)
        {
            var foundContent = _contentService.GetByIds(ids);
            return foundContent.Select(MapToDisplay);
        }

        /// <summary>
        /// Updates the permissions for a content item for a particular user group
        /// </summary>
        /// <param name="saveModel"></param>
        /// <returns></returns>
        /// <remarks>
        /// Permission check is done for letter 'R' which is for <see cref="ActionRights"/> which the user must have access to update
        /// </remarks>
        public async Task<ActionResult<IEnumerable<AssignedUserGroupPermissions>>> PostSaveUserGroupPermissions(UserGroupPermissionsSave saveModel)
        {
            if (saveModel.ContentId <= 0) return NotFound();

            // TODO: Should non-admins be allowed to set granular permissions?

            var content = _contentService.GetById(saveModel.ContentId);
            if (content == null) return NotFound();

            // Authorize...
            var resource = new ContentPermissionsResource(content, ActionRights.ActionLetter);
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, content, AuthorizationPolicies.ContentPermissionByResource);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            //current permissions explicitly assigned to this content item
            var contentPermissions = _contentService.GetPermissions(content)
                .ToDictionary(x => x.UserGroupId, x => x);

            var allUserGroups = _userService.GetAllUserGroups().ToArray();

            //loop through each user group
            foreach (var userGroup in allUserGroups)
            {
                //check if there's a permission set posted up for this user group
                IEnumerable<string> groupPermissions;
                if (saveModel.AssignedPermissions.TryGetValue(userGroup.Id, out groupPermissions))
                {
                    //create a string collection of the assigned letters
                    var groupPermissionCodes = groupPermissions.ToArray();

                    //check if there are no permissions assigned for this group save model, if that is the case we want to reset the permissions
                    //for this group/node which will go back to the defaults
                    if (groupPermissionCodes.Length == 0)
                    {
                        _userService.RemoveUserGroupPermissions(userGroup.Id, content.Id);
                    }
                    //check if they are the defaults, if so we should just remove them if they exist since it's more overhead having them stored
                    else if (userGroup.Permissions.UnsortedSequenceEqual(groupPermissionCodes))
                    {
                        //only remove them if they are actually currently assigned
                        if (contentPermissions.ContainsKey(userGroup.Id))
                        {
                            //remove these permissions from this node for this group since the ones being assigned are the same as the defaults
                            _userService.RemoveUserGroupPermissions(userGroup.Id, content.Id);
                        }
                    }
                    //if they are different we need to update, otherwise there's nothing to update
                    else if (contentPermissions.ContainsKey(userGroup.Id) == false || contentPermissions[userGroup.Id].AssignedPermissions.UnsortedSequenceEqual(groupPermissionCodes) == false)
                    {

                        _userService.ReplaceUserGroupPermissions(userGroup.Id, groupPermissionCodes.Select(x => x[0]), content.Id);
                    }
                }
            }

            return GetDetailedPermissions(content, allUserGroups);
        }

        /// <summary>
        /// Returns the user group permissions for user groups assigned to this node
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        /// <remarks>
        /// Permission check is done for letter 'R' which is for <see cref="ActionRights"/> which the user must have access to view
        /// </remarks>
        [Authorize(Policy = AuthorizationPolicies.ContentPermissionAdministrationById)]
        public ActionResult<IEnumerable<AssignedUserGroupPermissions>> GetDetailedPermissions(int contentId)
        {
            if (contentId <= 0) return NotFound();
            var content = _contentService.GetById(contentId);
            if (content == null) return NotFound();

            // TODO: Should non-admins be able to see detailed permissions?

            var allUserGroups = _userService.GetAllUserGroups();

            return GetDetailedPermissions(content, allUserGroups);
        }

        private ActionResult<IEnumerable<AssignedUserGroupPermissions>> GetDetailedPermissions(IContent content, IEnumerable<IUserGroup> allUserGroups)
        {
            //get all user groups and map their default permissions to the AssignedUserGroupPermissions model.
            //we do this because not all groups will have true assigned permissions for this node so if they don't have assigned permissions, we need to show the defaults.

            var defaultPermissionsByGroup = _umbracoMapper.MapEnumerable<IUserGroup, AssignedUserGroupPermissions>(allUserGroups);

            var defaultPermissionsAsDictionary = defaultPermissionsByGroup
                .ToDictionary(x => Convert.ToInt32(x.Id), x => x);

            //get the actual assigned permissions
            var assignedPermissionsByGroup = _contentService.GetPermissions(content).ToArray();

            //iterate over assigned and update the defaults with the real values
            foreach (var assignedGroupPermission in assignedPermissionsByGroup)
            {
                var defaultUserGroupPermissions = defaultPermissionsAsDictionary[assignedGroupPermission.UserGroupId];

                //clone the default permissions model to the assigned ones
                defaultUserGroupPermissions.AssignedPermissions = AssignedUserGroupPermissions.ClonePermissions(defaultUserGroupPermissions.DefaultPermissions);

                //since there is custom permissions assigned to this node for this group, we need to clear all of the default permissions
                //and we'll re-check it if it's one of the explicitly assigned ones
                foreach (var permission in defaultUserGroupPermissions.AssignedPermissions.SelectMany(x => x.Value))
                {
                    permission.Checked = false;
                    permission.Checked = assignedGroupPermission.AssignedPermissions.Contains(permission.PermissionCode, StringComparer.InvariantCulture);
                }

            }

            return defaultPermissionsByGroup;
        }

        /// <summary>
        /// Returns an item to be used to display the recycle bin for content
        /// </summary>
        /// <returns></returns>
        public ActionResult<ContentItemDisplay> GetRecycleBin()
        {
            var apps = new List<ContentApp>();
            apps.Add(ListViewContentAppFactory.CreateContentApp(_dataTypeService, _propertyEditors, "recycleBin", "content", Constants.DataTypes.DefaultMembersListView));
            apps[0].Active = true;
            var display = new ContentItemDisplay
            {
                Id = Constants.System.RecycleBinContent,
                ParentId = -1,
                ContentTypeAlias = "recycleBin",
                IsContainer = true,
                Path = "-1," + Constants.System.RecycleBinContent,
                Variants = new List<ContentVariantDisplay>
                {
                    new ContentVariantDisplay
                    {
                        CreateDate = DateTime.Now,
                        Name = _localizedTextService.Localize("general/recycleBin")
                    }
                },
                ContentApps = apps
            };

            return display;
        }

        public ActionResult<ContentItemDisplay> GetBlueprintById(int id)
        {
            var foundContent = _contentService.GetBlueprintById(id);
            if (foundContent == null)
            {
                return HandleContentNotFound(id);
            }

            var content = MapToDisplay(foundContent);

            SetupBlueprint(content, foundContent);

            return content;
        }

        private static void SetupBlueprint(ContentItemDisplay content, IContent persistedContent)
        {
            content.AllowPreview = false;

            //set a custom path since the tree that renders this has the content type id as the parent
            content.Path = string.Format("-1,{0},{1}", persistedContent.ContentTypeId, content.Id);

            content.AllowedActions = new[] { "A" };
            content.IsBlueprint = true;

            // TODO: exclude the content apps here
            //var excludeProps = new[] { "_umb_urls", "_umb_releasedate", "_umb_expiredate", "_umb_template" };
            //var propsTab = content.Tabs.Last();
            //propsTab.Properties = propsTab.Properties
            //    .Where(p => excludeProps.Contains(p.Alias) == false);
        }

        /// <summary>
        /// Gets the content json for the content id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        [OutgoingEditorModelEvent]
        [Authorize(Policy = AuthorizationPolicies.ContentPermissionBrowseById)]
        public ActionResult<ContentItemDisplay> GetById(int id)
        {
            var foundContent = GetObjectFromRequest(() => _contentService.GetById(id));
            if (foundContent == null)
            {
                return HandleContentNotFound(id);
            }
            var content = MapToDisplay(foundContent);
            return content;
        }

        /// <summary>
        /// Gets the content json for the content guid
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OutgoingEditorModelEvent]
        [Authorize(Policy = AuthorizationPolicies.ContentPermissionBrowseById)]
        public ActionResult<ContentItemDisplay> GetById(Guid id)
        {
            var foundContent = GetObjectFromRequest(() => _contentService.GetById(id));
            if (foundContent == null)
            {
                return HandleContentNotFound(id);
            }

            var content = MapToDisplay(foundContent);
            return content;
        }

        /// <summary>
        /// Gets the content json for the content udi
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OutgoingEditorModelEvent]
        [Authorize(Policy = AuthorizationPolicies.ContentPermissionBrowseById)]
        public ActionResult<ContentItemDisplay> GetById(Udi id)
        {
            var guidUdi = id as GuidUdi;
            if (guidUdi != null)
            {
                return GetById(guidUdi.Guid);
            }

            return NotFound();
        }

        /// <summary>
        /// Gets an empty content item for the document type.
        /// </summary>
        /// <param name="contentTypeAlias"></param>
        /// <param name="parentId"></param>
        [OutgoingEditorModelEvent]
        public ActionResult<ContentItemDisplay> GetEmpty(string contentTypeAlias, int parentId)
        {
            var contentType = _contentTypeService.Get(contentTypeAlias);
            if (contentType == null)
            {
                return NotFound();
            }

            return GetEmptyInner(contentType, parentId);
        }


        /// <summary>
        /// Gets an empty content item for the document type.
        /// </summary>
        /// <param name="contentTypeKey"></param>
        /// <param name="parentId"></param>
        [OutgoingEditorModelEvent]
        public ActionResult<ContentItemDisplay> GetEmptyByKey(Guid contentTypeKey, int parentId)
        {
            var contentType = _contentTypeService.Get(contentTypeKey);
            if (contentType == null)
            {
                return NotFound();
            }

            return GetEmptyInner(contentType, parentId);
        }

        private ContentItemDisplay GetEmptyInner(IContentType contentType, int parentId)
        {
            var emptyContent = _contentService.Create("", parentId, contentType.Alias, _backofficeSecurityAccessor.BackOfficeSecurity.GetUserId().ResultOr(0));
            var mapped = MapToDisplay(emptyContent);
            // translate the content type name if applicable
            mapped.ContentTypeName = _localizedTextService.UmbracoDictionaryTranslate(CultureDictionary, mapped.ContentTypeName);
            // if your user type doesn't have access to the Settings section it would not get this property mapped
            if (mapped.DocumentType != null)
                mapped.DocumentType.Name = _localizedTextService.UmbracoDictionaryTranslate(CultureDictionary, mapped.DocumentType.Name);

            //remove the listview app if it exists
            mapped.ContentApps = mapped.ContentApps.Where(x => x.Alias != "umbListView").ToList();

            return mapped;
        }

        [OutgoingEditorModelEvent]
        public ActionResult<ContentItemDisplay> GetEmptyBlueprint(int blueprintId, int parentId)
        {
            var blueprint = _contentService.GetBlueprintById(blueprintId);
            if (blueprint == null)
            {
                return NotFound();
            }

            blueprint.Id = 0;
            blueprint.Name = string.Empty;
            blueprint.ParentId = parentId;

            var mapped = _umbracoMapper.Map<ContentItemDisplay>(blueprint);

            //remove the listview app if it exists
            mapped.ContentApps = mapped.ContentApps.Where(x => x.Alias != "umbListView").ToList();

            return mapped;
        }

        /// <summary>
        /// Gets the Url for a given node ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult GetNiceUrl(int id)
        {
            var url = _publishedUrlProvider.GetUrl(id);
            return Content(url, MediaTypeNames.Text.Plain, Encoding.UTF8);
        }

        /// <summary>
        /// Gets the Url for a given node ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult GetNiceUrl(Guid id)
        {
            var url = _publishedUrlProvider.GetUrl(id);
            return Content(url, MediaTypeNames.Text.Plain, Encoding.UTF8);
        }

        /// <summary>
        /// Gets the Url for a given node ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult GetNiceUrl(Udi id)
        {
            var guidUdi = id as GuidUdi;
            if (guidUdi != null)
            {
                return GetNiceUrl(guidUdi.Guid);

            }

            return NotFound();
        }

        /// <summary>
        /// Gets the children for the content id passed in
        /// </summary>
        /// <returns></returns>
        [FilterAllowedOutgoingContent(typeof(IEnumerable<ContentItemBasic<ContentPropertyBasic>>), "Items")]
        public PagedResult<ContentItemBasic<ContentPropertyBasic>> GetChildren(
                int id,
                string includeProperties,
                int pageNumber = 0,
                int pageSize = 0,
                string orderBy = "SortOrder",
                Direction orderDirection = Direction.Ascending,
                bool orderBySystemField = true,
                string filter = "",
                string cultureName = "") // TODO: it's not a NAME it's the ISO CODE
        {
            long totalChildren;
            List<IContent> children;

            // Sets the culture to the only existing culture if we only have one culture.
            if (string.IsNullOrWhiteSpace(cultureName))
            {
                if (_allLangs.Value.Count == 1)
                {
                    cultureName = _allLangs.Value.First().Key;
                }
            }

            if (pageNumber > 0 && pageSize > 0)
            {
                IQuery<IContent> queryFilter = null;
                if (filter.IsNullOrWhiteSpace() == false)
                {
                    //add the default text filter
                    queryFilter = _sqlContext.Query<IContent>()
                        .Where(x => x.Name.Contains(filter));
                }

                children = _contentService
                    .GetPagedChildren(id, pageNumber - 1, pageSize, out totalChildren,
                        queryFilter,
                        Ordering.By(orderBy, orderDirection, cultureName, !orderBySystemField)).ToList();
            }
            else
            {
                //better to not use this without paging where possible, currently only the sort dialog does
                children = _contentService.GetPagedChildren(id, 0, int.MaxValue, out var total).ToList();
                totalChildren = children.Count;
            }

            if (totalChildren == 0)
            {
                return new PagedResult<ContentItemBasic<ContentPropertyBasic>>(0, 0, 0);
            }

            var pagedResult = new PagedResult<ContentItemBasic<ContentPropertyBasic>>(totalChildren, pageNumber, pageSize);
            pagedResult.Items = children.Select(content =>
                _umbracoMapper.Map<IContent, ContentItemBasic<ContentPropertyBasic>>(content,
                    context =>
                    {

                        context.SetCulture(cultureName);

                        // if there's a list of property aliases to map - we will make sure to store this in the mapping context.
                        if (!includeProperties.IsNullOrWhiteSpace())
                            context.SetIncludedProperties(includeProperties.Split(new[] { ", ", "," }, StringSplitOptions.RemoveEmptyEntries));
                    }))
                .ToList(); // evaluate now

            return pagedResult;
        }

        /// <summary>
        /// Creates a blueprint from a content item
        /// </summary>
        /// <param name="contentId">The content id to copy</param>
        /// <param name="name">The name of the blueprint</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult<SimpleNotificationModel> CreateBlueprintFromContent([FromQuery] int contentId, [FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));

            var content = _contentService.GetById(contentId);
            if (content == null)
            {
                return NotFound();
            }

            if (!EnsureUniqueName(name, content, nameof(name)))
            {
                return new ValidationErrorResult(ModelState.ToErrorDictionary());
            }

            var blueprint = _contentService.CreateContentFromBlueprint(content, name, _backofficeSecurityAccessor.BackOfficeSecurity.GetUserId().ResultOr(0));

            _contentService.SaveBlueprint(blueprint, _backofficeSecurityAccessor.BackOfficeSecurity.GetUserId().ResultOr(0));

            var notificationModel = new SimpleNotificationModel();
            notificationModel.AddSuccessNotification(
                _localizedTextService.Localize("blueprints/createdBlueprintHeading"),
                _localizedTextService.Localize("blueprints/createdBlueprintMessage", new[] { content.Name })
            );

            return notificationModel;
        }

        private bool EnsureUniqueName(string name, IContent content, string modelName)
        {
            var existing = _contentService.GetBlueprintsForContentTypes(content.ContentTypeId);
            if (existing.Any(x => x.Name == name && x.Id != content.Id))
            {
                ModelState.AddModelError(modelName, _localizedTextService.Localize("blueprints/duplicateBlueprintMessage"));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Saves content
        /// </summary>
        [FileUploadCleanupFilter]
        [ContentSaveValidation]
        public async Task<ActionResult<ContentItemDisplay>> PostSaveBlueprint([ModelBinder(typeof(BlueprintItemBinder))] ContentItemSave contentItem)
        {
            var contentItemDisplay = await PostSaveInternal(
                contentItem,
                content =>
                {
                    if (!EnsureUniqueName(content.Name, content, "Name"))
                    {
                        return OperationResult.Cancel(new EventMessages());
                    }

                    _contentService.SaveBlueprint(contentItem.PersistedContent, _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.Id);

                    // we need to reuse the underlying logic so return the result that it wants
                    return OperationResult.Succeed(new EventMessages());
                },
                content =>
                {
                    var display = MapToDisplay(content);
                    SetupBlueprint(display, content);
                    return display;
                });

            return contentItemDisplay;
        }

        /// <summary>
        /// Saves content
        /// </summary>
        [FileUploadCleanupFilter]
        [ContentSaveValidation]
        [OutgoingEditorModelEvent]
        public async Task<ActionResult<ContentItemDisplay>> PostSave([ModelBinder(typeof(ContentItemBinder))] ContentItemSave contentItem)
        {
            var contentItemDisplay = await PostSaveInternal(
                contentItem,
                content => _contentService.Save(contentItem.PersistedContent, _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.Id),
                MapToDisplay);

            return contentItemDisplay;
        }

        private async Task<ActionResult<ContentItemDisplay>> PostSaveInternal(ContentItemSave contentItem, Func<IContent, OperationResult> saveMethod, Func<IContent, ContentItemDisplay> mapToDisplay)
        {
            // Recent versions of IE/Edge may send in the full client side file path instead of just the file name.
            // To ensure similar behavior across all browsers no matter what they do - we strip the FileName property of all
            // uploaded files to being *only* the actual file name (as it should be).
            if (contentItem.UploadedFiles != null && contentItem.UploadedFiles.Any())
            {
                foreach (var file in contentItem.UploadedFiles)
                {
                    file.FileName = Path.GetFileName(file.FileName);
                }
            }

            // If we've reached here it means:
            // * Our model has been bound
            // * and validated
            // * any file attachments have been saved to their temporary location for us to use
            // * we have a reference to the DTO object and the persisted object
            // * Permissions are valid
            MapValuesForPersistence(contentItem);

            var passesCriticalValidationRules = ValidateCriticalData(contentItem, out var variantCount);

            // we will continue to save if model state is invalid, however we cannot save if critical data is missing.
            if (!ModelState.IsValid)
            {
                // check for critical data validation issues, we can't continue saving if this data is invalid
                if (!passesCriticalValidationRules)
                {
                    // ok, so the absolute mandatory data is invalid and it's new, we cannot actually continue!
                    // add the model state to the outgoing object and throw a validation message
                    var forDisplay = mapToDisplay(contentItem.PersistedContent);
                    forDisplay.Errors = ModelState.ToErrorDictionary();
                    return new ValidationErrorResult(forDisplay);
                }

                // if there's only one variant and the model state is not valid we cannot publish so change it to save
                if (variantCount == 1)
                {
                    switch (contentItem.Action)
                    {
                        case ContentSaveAction.Publish:
                        case ContentSaveAction.PublishWithDescendants:
                        case ContentSaveAction.PublishWithDescendantsForce:
                        case ContentSaveAction.SendPublish:
                        case ContentSaveAction.Schedule:
                            contentItem.Action = ContentSaveAction.Save;
                            break;
                        case ContentSaveAction.PublishNew:
                        case ContentSaveAction.PublishWithDescendantsNew:
                        case ContentSaveAction.PublishWithDescendantsForceNew:
                        case ContentSaveAction.SendPublishNew:
                        case ContentSaveAction.ScheduleNew:
                            contentItem.Action = ContentSaveAction.SaveNew;
                            break;
                    }
                }
            }

            bool wasCancelled;

            //used to track successful notifications
            var globalNotifications = new SimpleNotificationModel();
            var notifications = new Dictionary<string, SimpleNotificationModel>
            {
                //global (non variant specific) notifications
                [string.Empty] = globalNotifications
            };

            //The default validation language will be either: The default languauge, else if the content is brand new and the default culture is
            // not marked to be saved, it will be the first culture in the list marked for saving.
            var defaultCulture = _allLangs.Value.Values.FirstOrDefault(x => x.IsDefault)?.IsoCode;
            var cultureForInvariantErrors = CultureImpact.GetCultureForInvariantErrors(
                contentItem.PersistedContent,
                contentItem.Variants.Where(x => x.Save).Select(x => x.Culture).ToArray(),
                defaultCulture);

            switch (contentItem.Action)
            {
                case ContentSaveAction.Save:
                case ContentSaveAction.SaveNew:
                    SaveAndNotify(contentItem, saveMethod, variantCount, notifications, globalNotifications, "editContentSavedText", "editVariantSavedText", cultureForInvariantErrors, out wasCancelled);
                    break;
                case ContentSaveAction.Schedule:
                case ContentSaveAction.ScheduleNew:

                    if (!SaveSchedule(contentItem, globalNotifications))
                    {
                        wasCancelled = false;
                        break;
                    }
                    SaveAndNotify(contentItem, saveMethod, variantCount, notifications, globalNotifications, "editContentScheduledSavedText", "editVariantSavedText", cultureForInvariantErrors, out wasCancelled);
                    break;

                case ContentSaveAction.SendPublish:
                case ContentSaveAction.SendPublishNew:
                    var sendResult = _contentService.SendToPublication(contentItem.PersistedContent, _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.Id);
                    wasCancelled = sendResult == false;
                    if (sendResult)
                    {
                        if (variantCount > 1)
                        {
                            var variantErrors = ModelState.GetVariantsWithErrors(cultureForInvariantErrors);

                            var validVariants = contentItem.Variants
                                .Where(x => x.Save && !variantErrors.Contains((x.Culture, x.Segment)))
                                .Select(x => (culture: x.Culture, segment: x.Segment));

                            foreach (var (culture, segment) in validVariants)
                            {
                                var variantName = GetVariantName(culture, segment);

                                AddSuccessNotification(notifications, culture, segment,
                                    _localizedTextService.Localize("speechBubbles/editContentSendToPublish"),
                                    _localizedTextService.Localize("speechBubbles/editVariantSendToPublishText", new[] { variantName }));
                            }
                        }
                        else if (ModelState.IsValid)
                        {
                            globalNotifications.AddSuccessNotification(
                                _localizedTextService.Localize("speechBubbles/editContentSendToPublish"),
                                _localizedTextService.Localize("speechBubbles/editContentSendToPublishText"));
                        }
                    }
                    break;
                case ContentSaveAction.Publish:
                case ContentSaveAction.PublishNew:
                    {
                        var publishStatus = PublishInternal(contentItem, defaultCulture, cultureForInvariantErrors, out wasCancelled, out var successfulCultures);
                        AddPublishStatusNotifications(new[] { publishStatus }, globalNotifications, notifications, successfulCultures);
                    }
                    break;
                case ContentSaveAction.PublishWithDescendants:
                case ContentSaveAction.PublishWithDescendantsNew:
                    {
                        if (!await ValidatePublishBranchPermissionsAsync(contentItem))
                        {
                            globalNotifications.AddErrorNotification(
                                _localizedTextService.Localize("publish"),
                                _localizedTextService.Localize("publish/invalidPublishBranchPermissions"));
                            wasCancelled = false;
                            break;
                        }

                        var publishStatus = PublishBranchInternal(contentItem, false, cultureForInvariantErrors, out wasCancelled, out var successfulCultures).ToList();
                        AddPublishStatusNotifications(publishStatus, globalNotifications, notifications, successfulCultures);
                    }
                    break;
                case ContentSaveAction.PublishWithDescendantsForce:
                case ContentSaveAction.PublishWithDescendantsForceNew:
                    {
                        if (!await ValidatePublishBranchPermissionsAsync(contentItem))
                        {
                            globalNotifications.AddErrorNotification(
                                _localizedTextService.Localize("publish"),
                                _localizedTextService.Localize("publish/invalidPublishBranchPermissions"));
                            wasCancelled = false;
                            break;
                        }

                        var publishStatus = PublishBranchInternal(contentItem, true, cultureForInvariantErrors, out wasCancelled, out var successfulCultures).ToList();
                        AddPublishStatusNotifications(publishStatus, globalNotifications, notifications, successfulCultures);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            //get the updated model
            var display = mapToDisplay(contentItem.PersistedContent);

            //merge the tracked success messages with the outgoing model
            display.Notifications.AddRange(globalNotifications.Notifications);
            foreach (var v in display.Variants.Where(x => x.Language != null))
            {
                if (notifications.TryGetValue(v.Language.IsoCode, out var n))
                    v.Notifications.AddRange(n.Notifications);
            }

            HandleInvalidModelState(display, cultureForInvariantErrors);

            //lastly, if it is not valid, add the model state to the outgoing object and throw a 400
            if (!ModelState.IsValid)
            {
                display.Errors = ModelState.ToErrorDictionary();
                return new ValidationErrorResult(display);
            }

            if (wasCancelled)
            {
                AddCancelMessage(display);
                if (IsCreatingAction(contentItem.Action))
                {
                    //If the item is new and the operation was cancelled, we need to return a different
                    // status code so the UI can handle it since it won't be able to redirect since there
                    // is no Id to redirect to!
                    return new ValidationErrorResult(display);
                }
            }

            display.PersistedContent = contentItem.PersistedContent;

            return display;
        }

        private void AddPublishStatusNotifications(IReadOnlyCollection<PublishResult> publishStatus, SimpleNotificationModel globalNotifications, Dictionary<string, SimpleNotificationModel> variantNotifications, string[] successfulCultures)
        {
            //global notifications
            AddMessageForPublishStatus(publishStatus, globalNotifications, successfulCultures);
            //variant specific notifications
            foreach (var c in successfulCultures ?? Array.Empty<string>())
                AddMessageForPublishStatus(publishStatus, variantNotifications.GetOrCreate(c), successfulCultures);
        }

        /// <summary>
        /// Validates critical data for persistence and updates the ModelState and result accordingly
        /// </summary>
        /// <param name="contentItem"></param>
        /// <param name="variantCount">Returns the total number of variants (will be one if it's an invariant content item)</param>
        /// <returns></returns>
        /// <remarks>
        /// For invariant, the variants collection count will be 1 and this will check if that invariant item has the critical values for persistence (i.e. Name)
        ///
        /// For variant, each variant will be checked for critical data for persistence and if it's not there then it's flags will be reset and it will not
        /// be persisted. However, we also need to deal with the case where all variants don't pass this check and then there is nothing to save. This also deals
        /// with removing the Name validation keys based on data annotations validation for items that haven't been marked to be saved.
        /// </remarks>
        /// <returns>
        /// returns false if persistence cannot take place, returns true if persistence can take place even if there are validation errors
        /// </returns>
        private bool ValidateCriticalData(ContentItemSave contentItem, out int variantCount)
        {
            var variants = contentItem.Variants.ToList();
            variantCount = variants.Count;
            var savedCount = 0;
            var variantCriticalValidationErrors = new List<string>();
            for (var i = 0; i < variants.Count; i++)
            {
                var variant = variants[i];
                if (variant.Save)
                {
                    //ensure the variant has all critical required data to be persisted
                    if (!RequiredForPersistenceAttribute.HasRequiredValuesForPersistence(variant))
                    {
                        variantCriticalValidationErrors.Add(variant.Culture);
                        //if there's no Name, it cannot be persisted at all reset the flags, this cannot be saved or published
                        variant.Save = variant.Publish = false;

                        //if there's more than 1 variant, then we need to add the culture specific error
                        //messages based on the variants in error so that the messages show in the publish/save dialog
                        if (variants.Count > 1)
                            AddVariantValidationError(variant.Culture, variant.Segment, "publish/contentPublishedFailedByMissingName");
                        else
                            return false; //It's invariant and is missing critical data, it cannot be saved
                    }

                    savedCount++;
                }
                else
                {
                    var msKey = $"Variants[{i}].Name";
                    if (ModelState.ContainsKey(msKey))
                    {
                        //if it's not being saved, remove the validation key
                        if (!variant.Save) ModelState.Remove(msKey);
                    }
                }
            }

            if (savedCount == variantCriticalValidationErrors.Count)
            {
                //in this case there can be nothing saved since all variants marked to be saved haven't passed critical validation rules
                return false;
            }

            return true;
        }



        /// <summary>
        /// Helper method to perform the saving of the content and add the notifications to the result
        /// </summary>
        /// <param name="contentItem"></param>
        /// <param name="saveMethod"></param>
        /// <param name="variantCount"></param>
        /// <param name="notifications"></param>
        /// <param name="globalNotifications"></param>
        /// <param name="invariantSavedLocalizationKey"></param>
        /// <param name="variantSavedLocalizationKey"></param>
        /// <param name="wasCancelled"></param>
        /// <remarks>
        /// Method is used for normal Saving and Scheduled Publishing
        /// </remarks>
        private void SaveAndNotify(ContentItemSave contentItem, Func<IContent, OperationResult> saveMethod, int variantCount,
            Dictionary<string, SimpleNotificationModel> notifications, SimpleNotificationModel globalNotifications,
            string invariantSavedLocalizationKey, string variantSavedLocalizationKey, string cultureForInvariantErrors,
            out bool wasCancelled)
        {
            var saveResult = saveMethod(contentItem.PersistedContent);
            wasCancelled = saveResult.Success == false && saveResult.Result == OperationResultType.FailedCancelledByEvent;
            if (saveResult.Success)
            {
                if (variantCount > 1)
                {
                    var variantErrors = ModelState.GetVariantsWithErrors(cultureForInvariantErrors);

                    var savedWithoutErrors = contentItem.Variants
                        .Where(x => x.Save && !variantErrors.Contains((x.Culture, x.Segment)))
                        .Select(x => (culture: x.Culture, segment: x.Segment));

                    foreach (var (culture, segment) in savedWithoutErrors)
                    {
                        var variantName = GetVariantName(culture, segment);

                        AddSuccessNotification(notifications, culture, segment,
                            _localizedTextService.Localize("speechBubbles/editContentSavedHeader"),
                            _localizedTextService.Localize(variantSavedLocalizationKey, new[] { variantName }));
                    }
                }
                else if (ModelState.IsValid)
                {
                    globalNotifications.AddSuccessNotification(
                        _localizedTextService.Localize("speechBubbles/editContentSavedHeader"),
                        _localizedTextService.Localize(invariantSavedLocalizationKey));
                }
            }
        }

        /// <summary>
        /// Validates the incoming schedule and update the model
        /// </summary>
        /// <param name="contentItem"></param>
        /// <param name="globalNotifications"></param>
        private bool SaveSchedule(ContentItemSave contentItem, SimpleNotificationModel globalNotifications)
        {
            if (!contentItem.PersistedContent.ContentType.VariesByCulture())
                return SaveScheduleInvariant(contentItem, globalNotifications);
            else
                return SaveScheduleVariant(contentItem);
        }

        private bool SaveScheduleInvariant(ContentItemSave contentItem, SimpleNotificationModel globalNotifications)
        {
            var variant = contentItem.Variants.First();

            var currRelease = contentItem.PersistedContent.ContentSchedule.GetSchedule(ContentScheduleAction.Release).ToList();
            var currExpire = contentItem.PersistedContent.ContentSchedule.GetSchedule(ContentScheduleAction.Expire).ToList();

            //Do all validation of data first

            //1) release date cannot be less than now
            if (variant.ReleaseDate.HasValue && variant.ReleaseDate < DateTime.Now)
            {
                globalNotifications.AddErrorNotification(
                        _localizedTextService.Localize("speechBubbles", "validationFailedHeader"),
                        _localizedTextService.Localize("speechBubbles", "scheduleErrReleaseDate1"));
                return false;
            }

            //2) expire date cannot be less than now
            if (variant.ExpireDate.HasValue && variant.ExpireDate < DateTime.Now)
            {
                globalNotifications.AddErrorNotification(
                        _localizedTextService.Localize("speechBubbles", "validationFailedHeader"),
                        _localizedTextService.Localize("speechBubbles", "scheduleErrExpireDate1"));
                return false;
            }

            //3) expire date cannot be less than release date
            if (variant.ExpireDate.HasValue && variant.ReleaseDate.HasValue && variant.ExpireDate <= variant.ReleaseDate)
            {
                globalNotifications.AddErrorNotification(
                    _localizedTextService.Localize("speechBubbles", "validationFailedHeader"),
                    _localizedTextService.Localize("speechBubbles", "scheduleErrExpireDate2"));
                return false;
            }


            //Now we can do the data updates

            //remove any existing release dates so we can replace it
            //if there is a release date in the request or if there was previously a release and the request value is null then we are clearing the schedule
            if (variant.ReleaseDate.HasValue || currRelease.Count > 0)
                contentItem.PersistedContent.ContentSchedule.Clear(ContentScheduleAction.Release);

            //remove any existing expire dates so we can replace it
            //if there is an expiry date in the request or if there was a previous expiry and the request value is null then we are clearing the schedule
            if (variant.ExpireDate.HasValue || currExpire.Count > 0)
                contentItem.PersistedContent.ContentSchedule.Clear(ContentScheduleAction.Expire);

            //add the new schedule
            contentItem.PersistedContent.ContentSchedule.Add(variant.ReleaseDate, variant.ExpireDate);
            return true;
        }

        private bool SaveScheduleVariant(ContentItemSave contentItem)
        {
            //All variants in this collection should have a culture if we get here but we'll double check and filter here)
            var cultureVariants = contentItem.Variants.Where(x => !x.Culture.IsNullOrWhiteSpace()).ToList();
            var mandatoryCultures = _allLangs.Value.Values.Where(x => x.IsMandatory).Select(x => x.IsoCode).ToList();

            //Make a copy of the current schedule and apply updates to it

            var schedCopy = (ContentScheduleCollection)contentItem.PersistedContent.ContentSchedule.DeepClone();

            foreach (var variant in cultureVariants.Where(x => x.Save))
            {
                var currRelease = schedCopy.GetSchedule(variant.Culture, ContentScheduleAction.Release).ToList();
                var currExpire = schedCopy.GetSchedule(variant.Culture, ContentScheduleAction.Expire).ToList();

                //remove any existing release dates so we can replace it
                //if there is a release date in the request or if there was previously a release and the request value is null then we are clearing the schedule
                if (variant.ReleaseDate.HasValue || currRelease.Count > 0)
                    schedCopy.Clear(variant.Culture, ContentScheduleAction.Release);

                //remove any existing expire dates so we can replace it
                //if there is an expiry date in the request or if there was a previous expiry and the request value is null then we are clearing the schedule
                if (variant.ExpireDate.HasValue || currExpire.Count > 0)
                    schedCopy.Clear(variant.Culture, ContentScheduleAction.Expire);

                //add the new schedule
                schedCopy.Add(variant.Culture, variant.ReleaseDate, variant.ExpireDate);
            }

            //now validate the new schedule to make sure it passes all of the rules

            var isValid = true;

            //create lists of mandatory/non-mandatory states
            var mandatoryVariants = new List<(string culture, bool isPublished, List<DateTime> releaseDates)>();
            var nonMandatoryVariants = new List<(string culture, bool isPublished, List<DateTime> releaseDates)>();
            foreach (var groupedSched in schedCopy.FullSchedule.GroupBy(x => x.Culture))
            {
                var isPublished = contentItem.PersistedContent.Published && contentItem.PersistedContent.IsCulturePublished(groupedSched.Key);
                var releaseDates = groupedSched.Where(x => x.Action == ContentScheduleAction.Release).Select(x => x.Date).ToList();
                if (mandatoryCultures.Contains(groupedSched.Key, StringComparer.InvariantCultureIgnoreCase))
                    mandatoryVariants.Add((groupedSched.Key, isPublished, releaseDates));
                else
                    nonMandatoryVariants.Add((groupedSched.Key, isPublished, releaseDates));
            }

            var nonMandatoryVariantReleaseDates = nonMandatoryVariants.SelectMany(x => x.releaseDates).ToList();

            //validate that the mandatory languages have the right data
            foreach (var (culture, isPublished, releaseDates) in mandatoryVariants)
            {
                if (!isPublished && releaseDates.Count == 0)
                {
                    //can't continue, a mandatory variant is not published and not scheduled for publishing
                    // TODO: Add segment
                    AddVariantValidationError(culture, null, "speechBubbles/scheduleErrReleaseDate2");
                    isValid = false;
                    continue;
                }
                if (!isPublished && releaseDates.Any(x => nonMandatoryVariantReleaseDates.Any(r => x.Date > r.Date)))
                {
                    //can't continue, a mandatory variant is not published and it's scheduled for publishing after a non-mandatory
                    // TODO: Add segment
                    AddVariantValidationError(culture, null, "speechBubbles/scheduleErrReleaseDate3");
                    isValid = false;
                    continue;
                }
            }

            if (!isValid) return false;

            //now we can validate the more basic rules for individual variants
            foreach (var variant in cultureVariants.Where(x => x.ReleaseDate.HasValue || x.ExpireDate.HasValue))
            {
                //1) release date cannot be less than now
                if (variant.ReleaseDate.HasValue && variant.ReleaseDate < DateTime.Now)
                {
                    AddVariantValidationError(variant.Culture, variant.Segment, "speechBubbles/scheduleErrReleaseDate1");
                    isValid = false;
                    continue;
                }

                //2) expire date cannot be less than now
                if (variant.ExpireDate.HasValue && variant.ExpireDate < DateTime.Now)
                {
                    AddVariantValidationError(variant.Culture, variant.Segment, "speechBubbles/scheduleErrExpireDate1");
                    isValid = false;
                    continue;
                }

                //3) expire date cannot be less than release date
                if (variant.ExpireDate.HasValue && variant.ReleaseDate.HasValue && variant.ExpireDate <= variant.ReleaseDate)
                {
                    AddVariantValidationError(variant.Culture, variant.Segment, "speechBubbles/scheduleErrExpireDate2");
                    isValid = false;
                    continue;
                }
            }

            if (!isValid) return false;


            //now that we are validated, we can assign the copied schedule back to the model
            contentItem.PersistedContent.ContentSchedule = schedCopy;
            return true;
        }

        /// <summary>
        /// Used to add success notifications globally and for the culture
        /// </summary>
        /// <param name="notifications"></param>
        /// <param name="culture"></param>
        /// <param name="header"></param>
        /// <param name="msg"></param>
        /// <remarks>
        /// global notifications will be shown if all variant processing is successful and the save/publish dialog is closed, otherwise
        /// variant specific notifications are used to show success messages in the save/publish dialog.
        /// </remarks>
        private static void AddSuccessNotification(IDictionary<string, SimpleNotificationModel> notifications, string culture, string segment, string header, string msg)
        {
            //add the global notification (which will display globally if all variants are successfully processed)
            notifications[string.Empty].AddSuccessNotification(header, msg);
            //add the variant specific notification (which will display in the dialog if all variants are not successfully processed)
            var key = culture + "_" + segment;
            notifications.GetOrCreate(key).AddSuccessNotification(header, msg);
        }

        /// <summary>
        /// The user must have publish access to all descendant nodes of the content item in order to continue
        /// </summary>
        /// <param name="contentItem"></param>
        /// <returns></returns>
        private async Task<bool> ValidatePublishBranchPermissionsAsync(ContentItemSave contentItem)
        {
            // Authorize...
            var requirement = new ContentPermissionsPublishBranchRequirement(ActionPublish.ActionLetter);
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, contentItem.PersistedContent, requirement);
            return authorizationResult.Succeeded;
        }

        private IEnumerable<PublishResult> PublishBranchInternal(ContentItemSave contentItem, bool force, string cultureForInvariantErrors,
                out bool wasCancelled, out string[] successfulCultures)
        {
            if (!contentItem.PersistedContent.ContentType.VariesByCulture())
            {
                //its invariant, proceed normally
                var publishStatus = _contentService.SaveAndPublishBranch(contentItem.PersistedContent, force, userId: _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.Id);
                // TODO: Deal with multiple cancellations
                wasCancelled = publishStatus.Any(x => x.Result == PublishResultType.FailedPublishCancelledByEvent);
                successfulCultures = null; //must be null! this implies invariant
                return publishStatus;
            }

            var mandatoryCultures = _allLangs.Value.Values.Where(x => x.IsMandatory).Select(x => x.IsoCode).ToList();

            var variantErrors = ModelState.GetVariantsWithErrors(cultureForInvariantErrors);

            var variants = contentItem.Variants.ToList();

            //validate if we can publish based on the mandatory language requirements
            var canPublish = ValidatePublishingMandatoryLanguages(
                variantErrors,
                contentItem, variants, mandatoryCultures,
                mandatoryVariant => mandatoryVariant.Publish);

            //Now check if there are validation errors on each variant.
            //If validation errors are detected on a variant and it's state is set to 'publish', then we
            //need to change it to 'save'.
            //It is a requirement that this is performed AFTER ValidatePublishingMandatoryLanguages.

            foreach (var variant in contentItem.Variants)
            {
                if (variantErrors.Contains((variant.Culture, variant.Segment)))
                    variant.Publish = false;
            }

            var culturesToPublish = variants.Where(x => x.Publish).Select(x => x.Culture).ToArray();

            if (canPublish)
            {
                //proceed to publish if all validation still succeeds
                var publishStatus = _contentService.SaveAndPublishBranch(contentItem.PersistedContent, force, culturesToPublish, _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.Id);
                // TODO: Deal with multiple cancellations
                wasCancelled = publishStatus.Any(x => x.Result == PublishResultType.FailedPublishCancelledByEvent);
                successfulCultures = contentItem.Variants.Where(x => x.Publish).Select(x => x.Culture).ToArray();
                return publishStatus;
            }
            else
            {
                //can only save
                var saveResult = _contentService.Save(contentItem.PersistedContent, _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.Id);
                var publishStatus = new[]
                {
                    new PublishResult(PublishResultType.FailedPublishMandatoryCultureMissing, null, contentItem.PersistedContent)
                };
                wasCancelled = saveResult.Result == OperationResultType.FailedCancelledByEvent;
                successfulCultures = Array.Empty<string>();
                return publishStatus;
            }

        }

        /// <summary>
        /// Performs the publishing operation for a content item
        /// </summary>
        /// <param name="contentItem"></param>
        /// <param name="wasCancelled"></param>
        /// <param name="successfulCultures">
        /// if the content is variant this will return an array of cultures that will be published (passed validation rules)
        /// </param>
        /// <remarks>
        /// If this is a culture variant than we need to do some validation, if it's not we'll publish as normal
        /// </remarks>
        private PublishResult PublishInternal(ContentItemSave contentItem, string defaultCulture, string cultureForInvariantErrors, out bool wasCancelled, out string[] successfulCultures)
        {
            if (!contentItem.PersistedContent.ContentType.VariesByCulture())
            {
                //its invariant, proceed normally
                var publishStatus = _contentService.SaveAndPublish(contentItem.PersistedContent, userId: _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.Id);
                wasCancelled = publishStatus.Result == PublishResultType.FailedPublishCancelledByEvent;
                successfulCultures = null; //must be null! this implies invariant
                return publishStatus;
            }

            var mandatoryCultures = _allLangs.Value.Values.Where(x => x.IsMandatory).Select(x => x.IsoCode).ToList();

            var variantErrors = ModelState.GetVariantsWithErrors(cultureForInvariantErrors);

            var variants = contentItem.Variants.ToList();

            //validate if we can publish based on the mandatory languages selected
            var canPublish = ValidatePublishingMandatoryLanguages(
                variantErrors,
                contentItem, variants, mandatoryCultures,
                mandatoryVariant => mandatoryVariant.Publish);

            //if none are published and there are validation errors for mandatory cultures, then we can't publish anything


            //Now check if there are validation errors on each variant.
            //If validation errors are detected on a variant and it's state is set to 'publish', then we
            //need to change it to 'save'.
            //It is a requirement that this is performed AFTER ValidatePublishingMandatoryLanguages.
            foreach (var variant in contentItem.Variants)
            {
                if (variantErrors.Contains((variant.Culture, variant.Segment)))
                    variant.Publish = false;
            }

            //At this stage all variants might have failed validation which means there are no cultures flagged for publishing!
            var culturesToPublish = variants.Where(x => x.Publish).Select(x => x.Culture).ToArray();
            canPublish = canPublish && culturesToPublish.Length > 0;

            if (canPublish)
            {
                //try to publish all the values on the model - this will generally only fail if someone is tampering with the request
                //since there's no reason variant rules would be violated in normal cases.
                canPublish = PublishCulture(contentItem.PersistedContent, variants, defaultCulture);
            }

            if (canPublish)
            {
                //proceed to publish if all validation still succeeds
                var publishStatus = _contentService.SaveAndPublish(contentItem.PersistedContent, culturesToPublish, _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.Id);
                wasCancelled = publishStatus.Result == PublishResultType.FailedPublishCancelledByEvent;
                successfulCultures = culturesToPublish;
                return publishStatus;
            }
            else
            {
                //can only save
                var saveResult = _contentService.Save(contentItem.PersistedContent, _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.Id);
                var publishStatus = new PublishResult(PublishResultType.FailedPublishMandatoryCultureMissing, null, contentItem.PersistedContent);
                wasCancelled = saveResult.Result == OperationResultType.FailedCancelledByEvent;
                successfulCultures = Array.Empty<string>();
                return publishStatus;
            }
        }

        /// <summary>
        /// Validate if publishing is possible based on the mandatory language requirements
        /// </summary>
        /// <param name="variantsWithValidationErrors"></param>
        /// <param name="contentItem"></param>
        /// <param name="variants"></param>
        /// <param name="mandatoryCultures"></param>
        /// <param name="publishingCheck"></param>
        /// <returns></returns>
        private bool ValidatePublishingMandatoryLanguages(
            IReadOnlyCollection<(string culture, string segment)> variantsWithValidationErrors,
            ContentItemSave contentItem,
            IReadOnlyCollection<ContentVariantSave> variants,
            IReadOnlyList<string> mandatoryCultures,
            Func<ContentVariantSave, bool> publishingCheck)
        {
            var canPublish = true;
            var result = new List<(ContentVariantSave model, bool publishing, bool isValid)>();

            foreach (var culture in mandatoryCultures)
            {
                //Check if a mandatory language is missing from being published

                var mandatoryVariant = variants.First(x => x.Culture.InvariantEquals(culture));

                var isPublished = contentItem.PersistedContent.Published && contentItem.PersistedContent.IsCulturePublished(culture);
                var isPublishing = isPublished || publishingCheck(mandatoryVariant);
                var isValid = !variantsWithValidationErrors.Select(v => v.culture).InvariantContains(culture);

                result.Add((mandatoryVariant, isPublished || isPublishing, isValid));
            }

            //iterate over the results by invalid first
            string firstInvalidMandatoryCulture = null;
            foreach (var r in result.OrderBy(x => x.isValid))
            {
                if (!r.isValid)
                    firstInvalidMandatoryCulture = r.model.Culture;

                if (r.publishing && !r.isValid)
                {
                    //flagged for publishing but the mandatory culture is invalid
                    AddVariantValidationError(r.model.Culture, r.model.Segment, "publish/contentPublishedFailedReqCultureValidationError");
                    canPublish = false;
                }
                else if (r.publishing && r.isValid && firstInvalidMandatoryCulture != null)
                {
                    //in this case this culture also cannot be published because another mandatory culture is invalid
                    AddVariantValidationError(r.model.Culture, r.model.Segment, "publish/contentPublishedFailedReqCultureValidationError", firstInvalidMandatoryCulture);
                    canPublish = false;
                }
                else if (!r.publishing)
                {
                    //cannot continue publishing since a required culture that is not currently being published isn't published
                    AddVariantValidationError(r.model.Culture, r.model.Segment, "speechBubbles/contentReqCulturePublishError");
                    canPublish = false;
                }
            }

            return canPublish;
        }

        /// <summary>
        /// Call PublishCulture on the content item for each culture to get a validation result for each culture
        /// </summary>
        /// <param name="persistentContent"></param>
        /// <param name="cultureVariants"></param>
        /// <returns></returns>
        /// <remarks>
        /// This would generally never fail unless someone is tampering with the request
        /// </remarks>
        private bool PublishCulture(IContent persistentContent, IEnumerable<ContentVariantSave> cultureVariants, string defaultCulture)
        {
            foreach (var variant in cultureVariants.Where(x => x.Publish))
            {
                // publishing any culture, implies the invariant culture
                var valid = persistentContent.PublishCulture(CultureImpact.Explicit(variant.Culture, defaultCulture.InvariantEquals(variant.Culture)));
                if (!valid)
                {
                    AddVariantValidationError(variant.Culture, variant.Segment, "speechBubbles/contentCultureValidationError");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Adds a generic culture error for use in displaying the culture validation error in the save/publish/etc... dialogs
        /// </summary>
        /// <param name="culture">Culture to assign the error to</param>
        /// <param name="segment">Segment to assign the error to</param>
        /// <param name="localizationKey"></param>
        /// <param name="cultureToken">
        /// The culture used in the localization message, null by default which means <see cref="culture"/> will be used.
        /// </param>
        private void AddVariantValidationError(string culture, string segment, string localizationKey, string cultureToken = null)
        {
            var cultureToUse = cultureToken ?? culture;
            var variantName = GetVariantName(cultureToUse, segment);

            var errMsg = _localizedTextService.Localize(localizationKey, new[] { variantName });

            ModelState.AddVariantValidationError(culture, segment, errMsg);
        }

        /// <summary>
        /// Creates the human readable variant name based on culture and segment
        /// </summary>
        /// <param name="culture">Culture</param>
        /// <param name="segment">Segment</param>
        /// <returns></returns>
        private string GetVariantName(string culture, string segment)
        {
            if (culture.IsNullOrWhiteSpace() && segment.IsNullOrWhiteSpace())
            {
                // TODO: Get name for default variant from somewhere?
                return "Default";
            }

            var cultureName = culture == null ? null : _allLangs.Value[culture].CultureName;
            var variantName = string.Join(" — ", new[] { segment, cultureName }.Where(x => !x.IsNullOrWhiteSpace()));

            // Format: <segment> [&mdash;] <culture name>
            return variantName;
        }

        /// <summary>
        /// Publishes a document with a given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <remarks>
        /// The EnsureUserPermissionForContent attribute will deny access to this method if the current user
        /// does not have Publish access to this node.
        /// </remarks>
        [Authorize(Policy = AuthorizationPolicies.ContentPermissionPublishById)]
        public IActionResult PostPublishById(int id)
        {
            var foundContent = GetObjectFromRequest(() => _contentService.GetById(id));

            if (foundContent == null)
            {
                return HandleContentNotFound(id);
            }

            var publishResult = _contentService.SaveAndPublish(foundContent, userId: _backofficeSecurityAccessor.BackOfficeSecurity.GetUserId().ResultOr(0));
            if (publishResult.Success == false)
            {
                var notificationModel = new SimpleNotificationModel();
                AddMessageForPublishStatus(new[] { publishResult }, notificationModel);
                return new ValidationErrorResult(notificationModel);
            }

            return Ok();

        }

        [HttpDelete]
        [HttpPost]
        public IActionResult DeleteBlueprint(int id)
        {
            var found = _contentService.GetBlueprintById(id);

            if (found == null)
            {
                return HandleContentNotFound(id);
            }

            _contentService.DeleteBlueprint(found);

            return Ok();
        }

        /// <summary>
        /// Moves an item to the recycle bin, if it is already there then it will permanently delete it
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <remarks>
        /// The CanAccessContentAuthorize attribute will deny access to this method if the current user
        /// does not have Delete access to this node.
        /// </remarks>
        [Authorize(Policy = AuthorizationPolicies.ContentPermissionDeleteById)]
        [HttpDelete]
        [HttpPost]
        public IActionResult DeleteById(int id)
        {
            var foundContent = GetObjectFromRequest(() => _contentService.GetById(id));

            if (foundContent == null)
            {
                return HandleContentNotFound(id);
            }

            //if the current item is in the recycle bin
            if (foundContent.Trashed == false)
            {
                var moveResult = _contentService.MoveToRecycleBin(foundContent, _backofficeSecurityAccessor.BackOfficeSecurity.GetUserId().ResultOr(0));
                if (moveResult.Success == false)
                {
                    //returning an object of INotificationModel will ensure that any pending
                    // notification messages are added to the response.
                    return new ValidationErrorResult(new SimpleNotificationModel());
                }
            }
            else
            {
                var deleteResult = _contentService.Delete(foundContent, _backofficeSecurityAccessor.BackOfficeSecurity.GetUserId().ResultOr(0));
                if (deleteResult.Success == false)
                {
                    //returning an object of INotificationModel will ensure that any pending
                    // notification messages are added to the response.
                    return new ValidationErrorResult(new SimpleNotificationModel());
                }
            }

            return Ok();
        }

        /// <summary>
        /// Empties the recycle bin
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// attributed with EnsureUserPermissionForContent to verify the user has access to the recycle bin
        /// </remarks>
        [HttpDelete]
        [HttpPost]
        [Authorize(Policy = AuthorizationPolicies.ContentPermissionEmptyRecycleBin)]
        public IActionResult EmptyRecycleBin()
        {
            _contentService.EmptyRecycleBin(_backofficeSecurityAccessor.BackOfficeSecurity.GetUserId().ResultOr(Constants.Security.SuperUserId));

            return new UmbracoNotificationSuccessResponse(_localizedTextService.Localize("defaultdialogs/recycleBinIsEmpty"));
        }

        /// <summary>
        /// Change the sort order for content
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
            var resource = new ContentPermissionsResource(_contentService.GetById(sorted.ParentId), ActionSort.ActionLetter);
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, resource, AuthorizationPolicies.ContentPermissionByResource);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            try
            {
                // Save content with new sort order and update content xml in db accordingly
                var sortResult = _contentService.Sort(sorted.IdSortOrder, _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser.Id);
                if (!sortResult.Success)
                {
                    _logger.LogWarning("Content sorting failed, this was probably caused by an event being cancelled");
                    // TODO: Now you can cancel sorting, does the event messages bubble up automatically?
                    return new ValidationErrorResult("Content sorting failed, this was probably caused by an event being cancelled");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not update content sort order");
                throw;
            }
        }

        /// <summary>
        /// Change the sort order for media
        /// </summary>
        /// <param name="move"></param>
        /// <returns></returns>
        public async Task<IActionResult> PostMove(MoveOrCopy move)
        {
            // Authorize...
            var resource = new ContentPermissionsResource(_contentService.GetById(move.ParentId), ActionMove.ActionLetter);
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, resource, AuthorizationPolicies.ContentPermissionByResource);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            var toMoveResult = ValidateMoveOrCopy(move);
            if (!(toMoveResult.Result is null))
            {
                return toMoveResult.Result;
            }
            var toMove = toMoveResult.Value;

            _contentService.Move(toMove, move.ParentId, _backofficeSecurityAccessor.BackOfficeSecurity.GetUserId().ResultOr(0));

            return Content(toMove.Path, MediaTypeNames.Text.Plain, Encoding.UTF8);
        }

        /// <summary>
        /// Copies a content item and places the copy as a child of a given parent Id
        /// </summary>
        /// <param name="copy"></param>
        /// <returns></returns>
        public async Task<ActionResult<IContent>> PostCopy(MoveOrCopy copy)
        {
            // Authorize...
            var resource = new ContentPermissionsResource(_contentService.GetById(copy.ParentId), ActionCopy.ActionLetter);
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, resource, AuthorizationPolicies.ContentPermissionByResource);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            var toCopyResult = ValidateMoveOrCopy(copy);
            if (!(toCopyResult.Result is null))
            {
                return toCopyResult.Result;
            }
            var toCopy = toCopyResult.Value;
            var c = _contentService.Copy(toCopy, copy.ParentId, copy.RelateToOriginal, copy.Recursive, _backofficeSecurityAccessor.BackOfficeSecurity.GetUserId().ResultOr(0));

            return Content(c.Path, MediaTypeNames.Text.Plain, Encoding.UTF8);
        }

        /// <summary>
        /// Unpublishes a node with a given Id and returns the unpublished entity
        /// </summary>
        /// <param name="model">The content and variants to unpublish</param>
        /// <returns></returns>
        [OutgoingEditorModelEvent]
        public async Task<ActionResult<ContentItemDisplay>> PostUnpublish(UnpublishContent model)
        {
            var foundContent = _contentService.GetById(model.Id);

            if (foundContent == null)
            {
                return HandleContentNotFound(model.Id);
            }

            // Authorize...
            var resource = new ContentPermissionsResource(foundContent, ActionUnpublish.ActionLetter);
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, resource, AuthorizationPolicies.ContentPermissionByResource);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            var languageCount = _allLangs.Value.Count();
            if (model.Cultures.Length == 0 || model.Cultures.Length == languageCount)
            {
                //this means that the entire content item will be unpublished
                var unpublishResult = _contentService.Unpublish(foundContent, userId: _backofficeSecurityAccessor.BackOfficeSecurity.GetUserId().ResultOr(0));

                var content = MapToDisplay(foundContent);

                if (!unpublishResult.Success)
                {
                    AddCancelMessage(content);
                    return new ValidationErrorResult(content);
                }
                else
                {
                    content.AddSuccessNotification(
                        _localizedTextService.Localize("content/unpublish"),
                        _localizedTextService.Localize("speechBubbles/contentUnpublished"));
                    return content;
                }
            }
            else
            {
                //we only want to unpublish some of the variants
                var results = new Dictionary<string, PublishResult>();
                foreach (var c in model.Cultures)
                {
                    var result = _contentService.Unpublish(foundContent, culture: c, userId: _backofficeSecurityAccessor.BackOfficeSecurity.GetUserId().ResultOr(0));
                    results[c] = result;
                    if (result.Result == PublishResultType.SuccessUnpublishMandatoryCulture)
                    {
                        //if this happens, it means they are all unpublished, we don't need to continue
                        break;
                    }
                }

                var content = MapToDisplay(foundContent);

                //check for this status and return the correct message
                if (results.Any(x => x.Value.Result == PublishResultType.SuccessUnpublishMandatoryCulture))
                {
                    content.AddSuccessNotification(
                           _localizedTextService.Localize("content/unpublish"),
                           _localizedTextService.Localize("speechBubbles/contentMandatoryCultureUnpublished"));
                    return content;
                }

                //otherwise add a message for each one unpublished
                foreach (var r in results)
                {
                    content.AddSuccessNotification(
                           _localizedTextService.Localize("content/unpublish"),
                           _localizedTextService.Localize("speechBubbles/contentCultureUnpublished", new[] { _allLangs.Value[r.Key].CultureName }));
                }
                return content;

            }

        }

        public ContentDomainsAndCulture GetCultureAndDomains(int id)
        {
            var nodeDomains = _domainService.GetAssignedDomains(id, true).ToArray();
            var wildcard = nodeDomains.FirstOrDefault(d => d.IsWildcard);
            var domains = nodeDomains.Where(d => !d.IsWildcard).Select(d => new DomainDisplay(d.DomainName, d.LanguageId.GetValueOrDefault(0)));
            return new ContentDomainsAndCulture
            {
                Domains = domains,
                Language = wildcard == null || !wildcard.LanguageId.HasValue ? "undefined" : wildcard.LanguageId.ToString()
            };
        }

        [HttpPost]
        public ActionResult<DomainSave> PostSaveLanguageAndDomains(DomainSave model)
        {
            foreach (var domain in model.Domains)
            {
                try
                {
                    var uri = DomainUtilities.ParseUriFromDomainName(domain.Name, new Uri(Request.GetEncodedUrl()));
                }
                catch (UriFormatException)
                {
                    return new ValidationErrorResult(_localizedTextService.Localize("assignDomain/invalidDomain"));
                }
            }

            var node = _contentService.GetById(model.NodeId);

            if (node == null)
            {
                HttpContext.SetReasonPhrase("Node Not Found.");
                return NotFound("There is no content node with id {model.NodeId}.");
            }

            var permission = _userService.GetPermissions(_backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser, node.Path);


            if (permission.AssignedPermissions.Contains(ActionAssignDomain.ActionLetter.ToString(), StringComparer.Ordinal) == false)
            {
                HttpContext.SetReasonPhrase("Permission Denied.");
                return BadRequest("You do not have permission to assign domains on that node.");
            }

            model.Valid = true;
            var domains = _domainService.GetAssignedDomains(model.NodeId, true).ToArray();
            var languages = _localizationService.GetAllLanguages().ToArray();
            var language = model.Language > 0 ? languages.FirstOrDefault(l => l.Id == model.Language) : null;

            // process wildcard
            if (language != null)
            {
                // yet there is a race condition here...
                var wildcard = domains.FirstOrDefault(d => d.IsWildcard);
                if (wildcard != null)
                {
                    wildcard.LanguageId = language.Id;
                }
                else
                {
                    wildcard = new UmbracoDomain("*" + model.NodeId)
                    {
                        LanguageId = model.Language,
                        RootContentId = model.NodeId
                    };
                }

                var saveAttempt = _domainService.Save(wildcard);
                if (saveAttempt == false)
                {
                    HttpContext.SetReasonPhrase(saveAttempt.Result.Result.ToString());
                    return BadRequest("Saving domain failed");
                }
            }
            else
            {
                var wildcard = domains.FirstOrDefault(d => d.IsWildcard);
                if (wildcard != null)
                {
                    _domainService.Delete(wildcard);
                }
            }

            // process domains
            // delete every (non-wildcard) domain, that exists in the DB yet is not in the model
            foreach (var domain in domains.Where(d => d.IsWildcard == false && model.Domains.All(m => m.Name.InvariantEquals(d.DomainName) == false)))
            {
                _domainService.Delete(domain);
            }

            var names = new List<string>();

            // create or update domains in the model
            foreach (var domainModel in model.Domains.Where(m => string.IsNullOrWhiteSpace(m.Name) == false))
            {
                language = languages.FirstOrDefault(l => l.Id == domainModel.Lang);
                if (language == null)
                {
                    continue;
                }

                var name = domainModel.Name.ToLowerInvariant();
                if (names.Contains(name))
                {
                    domainModel.Duplicate = true;
                    continue;
                }
                names.Add(name);
                var domain = domains.FirstOrDefault(d => d.DomainName.InvariantEquals(domainModel.Name));
                if (domain != null)
                {
                    domain.LanguageId = language.Id;
                    _domainService.Save(domain);
                }
                else if (_domainService.Exists(domainModel.Name))
                {
                    domainModel.Duplicate = true;
                    var xdomain = _domainService.GetByName(domainModel.Name);
                    var xrcid = xdomain.RootContentId;
                    if (xrcid.HasValue)
                    {
                        var xcontent = _contentService.GetById(xrcid.Value);
                        var xnames = new List<string>();
                        while (xcontent != null)
                        {
                            xnames.Add(xcontent.Name);
                            if (xcontent.ParentId < -1)
                                xnames.Add("Recycle Bin");
                            xcontent = _contentService.GetParent(xcontent);
                        }
                        xnames.Reverse();
                        domainModel.Other = "/" + string.Join("/", xnames);
                    }
                }
                else
                {
                    // yet there is a race condition here...
                    var newDomain = new UmbracoDomain(name)
                    {
                        LanguageId = domainModel.Lang,
                        RootContentId = model.NodeId
                    };
                    var saveAttempt = _domainService.Save(newDomain);
                    if (saveAttempt == false)
                    {
                        HttpContext.SetReasonPhrase(saveAttempt.Result.Result.ToString());
                        return BadRequest("Saving new domain failed");
                    }
                }
            }

            model.Valid = model.Domains.All(m => m.Duplicate == false);

            return model;
        }

        /// <summary>
        /// Ensure there is culture specific errors in the result if any errors are for culture properties
        /// and we're dealing with variant content, then call the base class HandleInvalidModelState
        /// </summary>
        /// <param name="display"></param>
        /// <remarks>
        /// This is required to wire up the validation in the save/publish dialog
        /// </remarks>
        private void HandleInvalidModelState(ContentItemDisplay display, string cultureForInvariantErrors)
        {
            if (!ModelState.IsValid && display.Variants.Count() > 1)
            {
                //Add any culture specific errors here
                var variantErrors = ModelState.GetVariantsWithErrors(cultureForInvariantErrors);

                foreach (var (culture, segment) in variantErrors)
                {
                    AddVariantValidationError(culture, segment, "speechBubbles/contentCultureValidationError");
                }
            }
        }

        /// <summary>
        /// Maps the dto property values and names to the persisted model
        /// </summary>
        /// <param name="contentSave"></param>
        private void MapValuesForPersistence(ContentItemSave contentSave)
        {
            // inline method to determine the culture and segment to persist the property
            (string culture, string segment) PropertyCultureAndSegment(IProperty property, ContentVariantSave variant)
            {
                var culture = property.PropertyType.VariesByCulture() ? variant.Culture : null;
                var segment = property.PropertyType.VariesBySegment() ? variant.Segment : null;
                return (culture, segment);
            }

            var variantIndex = 0;

            //loop through each variant, set the correct name and property values
            foreach (var variant in contentSave.Variants)
            {
                //Don't update anything for this variant if Save is not true
                if (!variant.Save) continue;

                //Don't update the name if it is empty
                if (!variant.Name.IsNullOrWhiteSpace())
                {
                    if (contentSave.PersistedContent.ContentType.VariesByCulture())
                    {
                        if (variant.Culture.IsNullOrWhiteSpace())
                            throw new InvalidOperationException($"Cannot set culture name without a culture.");
                        contentSave.PersistedContent.SetCultureName(variant.Name, variant.Culture);
                    }
                    else
                    {
                        contentSave.PersistedContent.Name = variant.Name;
                    }
                }

                //This is important! We only want to process invariant properties with the first variant, for any other variant
                // we need to exclude invariant properties from being processed, otherwise they will be double processed for the
                // same value which can cause some problems with things such as file uploads.
                var propertyCollection = variantIndex == 0
                    ? variant.PropertyCollectionDto
                    : new ContentPropertyCollectionDto
                    {
                        Properties = variant.PropertyCollectionDto.Properties.Where(
                            x => !x.Culture.IsNullOrWhiteSpace() || !x.Segment.IsNullOrWhiteSpace())
                    };

                //for each variant, map the property values
                MapPropertyValuesForPersistence<IContent, ContentItemSave>(
                    contentSave,
                    propertyCollection,
                    (save, property) =>
                    {
                        // Get property value
                        (var culture, var segment) = PropertyCultureAndSegment(property, variant);
                        return property.GetValue(culture, segment);
                    },
                    (save, property, v) =>
                    {
                        // Set property value
                        (var culture, var segment) = PropertyCultureAndSegment(property, variant);
                        property.SetValue(v, culture, segment);
                    },
                    variant.Culture);

                variantIndex++;
            }

            // handle template
            if (string.IsNullOrWhiteSpace(contentSave.TemplateAlias)) // cleared: clear if not already null
            {
                if (contentSave.PersistedContent.TemplateId != null)
                {
                    contentSave.PersistedContent.TemplateId = null;
                }
            }
            else // set: update if different
            {
                var template = _fileService.GetTemplate(contentSave.TemplateAlias);
                if (template == null)
                {
                    //ModelState.AddModelError("Template", "No template exists with the specified alias: " + contentItem.TemplateAlias);
                    _logger.LogWarning("No template exists with the specified alias: {TemplateAlias}", contentSave.TemplateAlias);
                }
                else if (template.Id != contentSave.PersistedContent.TemplateId)
                {
                    contentSave.PersistedContent.TemplateId = template.Id;
                }
            }
        }

        /// <summary>
        /// Ensures the item can be moved/copied to the new location
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private ActionResult<IContent> ValidateMoveOrCopy(MoveOrCopy model)
        {
            if (model == null)
            {
                return NotFound();
            }

            var contentService = _contentService;
            var toMove = contentService.GetById(model.Id);
            if (toMove == null)
            {
                return NotFound();
            }
            if (model.ParentId < 0)
            {
                //cannot move if the content item is not allowed at the root
                if (toMove.ContentType.AllowedAsRoot == false)
                {
                    return ValidationErrorResult.CreateNotificationValidationErrorResult(
                                    _localizedTextService.Localize("moveOrCopy/notAllowedAtRoot"));
                }
            }
            else
            {
                var parent = contentService.GetById(model.ParentId);
                if (parent == null)
                {
                    return NotFound();
                }

                var parentContentType = _contentTypeService.Get(parent.ContentTypeId);
                //check if the item is allowed under this one
                if (parentContentType.AllowedContentTypes.Select(x => x.Id).ToArray()
                        .Any(x => x.Value == toMove.ContentType.Id) == false)
                {
                    return ValidationErrorResult.CreateNotificationValidationErrorResult(
                                    _localizedTextService.Localize("moveOrCopy/notAllowedByContentType"));
                }

                // Check on paths
                if ($",{parent.Path},".IndexOf($",{toMove.Id},", StringComparison.Ordinal) > -1)
                {
                    return ValidationErrorResult.CreateNotificationValidationErrorResult(
                                    _localizedTextService.Localize("moveOrCopy/notAllowedByPath"));
                }
            }

            return new ActionResult<IContent>(toMove);
        }

        /// <summary>
        /// Adds notification messages to the outbound display model for a given published status
        /// </summary>
        /// <param name="statuses"></param>
        /// <param name="display"></param>
        /// <param name="successfulCultures">
        /// This is null when dealing with invariant content, else it's the cultures that were successfully published
        /// </param>
        private void AddMessageForPublishStatus(IReadOnlyCollection<PublishResult> statuses, INotificationModel display, string[] successfulCultures = null)
        {
            var totalStatusCount = statuses.Count();

            //Put the statuses into groups, each group results in a different message
            var statusGroup = statuses.GroupBy(x =>
            {
                switch (x.Result)
                {
                    case PublishResultType.SuccessPublish:
                    case PublishResultType.SuccessPublishCulture:
                        //these 2 belong to a single group
                        return PublishResultType.SuccessPublish;
                    case PublishResultType.FailedPublishAwaitingRelease:
                    case PublishResultType.FailedPublishCultureAwaitingRelease:
                        //these 2 belong to a single group
                        return PublishResultType.FailedPublishAwaitingRelease;
                    case PublishResultType.FailedPublishHasExpired:
                    case PublishResultType.FailedPublishCultureHasExpired:
                        //these 2 belong to a single group
                        return PublishResultType.FailedPublishHasExpired;
                    case PublishResultType.SuccessPublishAlready:
                    case PublishResultType.FailedPublishPathNotPublished:
                    case PublishResultType.FailedPublishCancelledByEvent:
                    case PublishResultType.FailedPublishIsTrashed:
                    case PublishResultType.FailedPublishContentInvalid:
                    case PublishResultType.FailedPublishMandatoryCultureMissing:
                        //the rest that we are looking for each belong in their own group
                        return x.Result;
                    default:
                        throw new IndexOutOfRangeException($"{x.Result}\" was not expected.");
                }
            });

            foreach (var status in statusGroup)
            {
                switch (status.Key)
                {
                    case PublishResultType.SuccessPublishAlready:
                        {
                            // TODO: Here we should have messaging for when there are release dates specified like https://github.com/umbraco/Umbraco-CMS/pull/3507
                            // but this will take a bit of effort because we need to deal with variants, different messaging, etc... A quick attempt was made here:
                            // http://github.com/umbraco/Umbraco-CMS/commit/9b3de7b655e07c612c824699b48a533c0448131a

                            //special case, we will only show messages for this if:
                            // * it's not a bulk publish operation
                            // * it's a bulk publish operation and all successful statuses are this one
                            var itemCount = status.Count();
                            if (totalStatusCount == 1 || totalStatusCount == itemCount)
                            {
                                if (successfulCultures == null || totalStatusCount == itemCount)
                                {
                                    //either invariant single publish, or bulk publish where all statuses are already published
                                    display.AddSuccessNotification(
                                        _localizedTextService.Localize("speechBubbles/editContentPublishedHeader"),
                                        _localizedTextService.Localize("speechBubbles/editContentPublishedText"));
                                }
                                else
                                {
                                    foreach (var c in successfulCultures)
                                    {
                                        display.AddSuccessNotification(
                                            _localizedTextService.Localize("speechBubbles/editContentPublishedHeader"),
                                            _localizedTextService.Localize("speechBubbles/editVariantPublishedText", new[] { _allLangs.Value[c].CultureName }));
                                    }
                                }
                            }
                        }
                        break;
                    case PublishResultType.SuccessPublish:
                        {
                            // TODO: Here we should have messaging for when there are release dates specified like https://github.com/umbraco/Umbraco-CMS/pull/3507
                            // but this will take a bit of effort because we need to deal with variants, different messaging, etc... A quick attempt was made here:
                            // http://github.com/umbraco/Umbraco-CMS/commit/9b3de7b655e07c612c824699b48a533c0448131a

                            var itemCount = status.Count();
                            if (successfulCultures == null)
                            {
                                display.AddSuccessNotification(
                                    _localizedTextService.Localize("speechBubbles/editContentPublishedHeader"),
                                    totalStatusCount > 1
                                        ? _localizedTextService.Localize("speechBubbles/editMultiContentPublishedText", new[] { itemCount.ToInvariantString() })
                                        : _localizedTextService.Localize("speechBubbles/editContentPublishedText"));
                            }
                            else
                            {
                                foreach (var c in successfulCultures)
                                {
                                    display.AddSuccessNotification(
                                        _localizedTextService.Localize("speechBubbles/editContentPublishedHeader"),
                                        totalStatusCount > 1
                                            ? _localizedTextService.Localize("speechBubbles/editMultiVariantPublishedText", new[] { itemCount.ToInvariantString(), _allLangs.Value[c].CultureName })
                                            : _localizedTextService.Localize("speechBubbles/editVariantPublishedText", new[] { _allLangs.Value[c].CultureName }));
                                }
                            }
                        }
                        break;
                    case PublishResultType.FailedPublishPathNotPublished:
                        {
                            //TODO: This doesn't take into account variations with the successfulCultures param
                            var names = string.Join(", ", status.Select(x => $"'{x.Content.Name}'"));
                            display.AddWarningNotification(
                                _localizedTextService.Localize("publish"),
                                _localizedTextService.Localize("publish/contentPublishedFailedByParent",
                                    new[] { names }).Trim());
                        }
                        break;
                    case PublishResultType.FailedPublishCancelledByEvent:
                        {
                            //TODO: This doesn't take into account variations with the successfulCultures param
                            var names = string.Join(", ", status.Select(x => $"'{x.Content.Name}'"));
                            AddCancelMessage(display, message: "publish/contentPublishedFailedByEvent", messageParams: new[] { names });
                        }
                        break;
                    case PublishResultType.FailedPublishAwaitingRelease:
                        {
                            //TODO: This doesn't take into account variations with the successfulCultures param
                            var names = string.Join(", ", status.Select(x => $"'{x.Content.Name}'"));
                            display.AddWarningNotification(
                                    _localizedTextService.Localize("publish"),
                                    _localizedTextService.Localize("publish/contentPublishedFailedAwaitingRelease",
                                        new[] { names }).Trim());
                        }
                        break;
                    case PublishResultType.FailedPublishHasExpired:
                        {
                            //TODO: This doesn't take into account variations with the successfulCultures param
                            var names = string.Join(", ", status.Select(x => $"'{x.Content.Name}'"));
                            display.AddWarningNotification(
                                _localizedTextService.Localize("publish"),
                                _localizedTextService.Localize("publish/contentPublishedFailedExpired",
                                    new[] { names }).Trim());
                        }
                        break;
                    case PublishResultType.FailedPublishIsTrashed:
                        {
                            //TODO: This doesn't take into account variations with the successfulCultures param
                            var names = string.Join(", ", status.Select(x => $"'{x.Content.Name}'"));
                            display.AddWarningNotification(
                                _localizedTextService.Localize("publish"),
                                _localizedTextService.Localize("publish/contentPublishedFailedIsTrashed",
                                    new[] { names }).Trim());
                        }
                        break;
                    case PublishResultType.FailedPublishContentInvalid:
                        {
                            if (successfulCultures == null)
                            {
                                var names = string.Join(", ", status.Select(x => $"'{x.Content.Name}'"));
                                display.AddWarningNotification(
                                    _localizedTextService.Localize("publish"),
                                    _localizedTextService.Localize("publish/contentPublishedFailedInvalid",
                                        new[] { names }).Trim());
                            }
                            else
                            {
                                foreach (var c in successfulCultures)
                                {
                                    var names = string.Join(", ", status.Select(x => $"'{(x.Content.ContentType.VariesByCulture() ? x.Content.GetCultureName(c) : x.Content.Name)}'"));
                                    display.AddWarningNotification(
                                        _localizedTextService.Localize("publish"),
                                        _localizedTextService.Localize("publish/contentPublishedFailedInvalid",
                                            new[] { names }).Trim());
                                }
                            }
                        }
                        break;
                    case PublishResultType.FailedPublishMandatoryCultureMissing:
                        display.AddWarningNotification(
                            _localizedTextService.Localize("publish"),
                            "publish/contentPublishedFailedByCulture");
                        break;
                    default:
                        throw new IndexOutOfRangeException($"PublishedResultType \"{status.Key}\" was not expected.");
                }
            }
        }

        /// <summary>
        /// Used to map an <see cref="IContent"/> instance to a <see cref="ContentItemDisplay"/> and ensuring a language is present if required
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private ContentItemDisplay MapToDisplay(IContent content)
        {
            var display = _umbracoMapper.Map<ContentItemDisplay>(content, context =>
            {
                context.Items["CurrentUser"] = _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser;
            });
            display.AllowPreview = display.AllowPreview && content.Trashed == false && content.ContentType.IsElement == false;
            return display;
        }

        [Authorize(Policy = AuthorizationPolicies.ContentPermissionBrowseById)]
        public ActionResult<IEnumerable<NotifySetting>> GetNotificationOptions(int contentId)
        {
            var notifications = new List<NotifySetting>();
            if (contentId <= 0) return NotFound();

            var content = _contentService.GetById(contentId);
            if (content == null) return NotFound();

            var userNotifications = _notificationService.GetUserNotifications(_backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser, content.Path).ToList();

            foreach (var a in _actionCollection.Where(x => x.ShowInNotifier))
            {
                var n = new NotifySetting
                {
                    Name = _localizedTextService.Localize("actions", a.Alias),
                    Checked = userNotifications.FirstOrDefault(x => x.Action == a.Letter.ToString()) != null,
                    NotifyCode = a.Letter.ToString()
                };
                notifications.Add(n);
            }

            return notifications;
        }

        public IActionResult PostNotificationOptions(int contentId, [FromQuery(Name = "notifyOptions[]")] string[] notifyOptions)
        {
            if (contentId <= 0) return NotFound();
            var content = _contentService.GetById(contentId);
            if (content == null) return NotFound();

            _notificationService.SetNotifications(_backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser, content, notifyOptions);

            return NoContent();
        }

        [HttpGet]
        public IEnumerable<RollbackVersion> GetRollbackVersions(int contentId, string culture = null)
        {
            var rollbackVersions = new List<RollbackVersion>();
            var writerIds = new HashSet<int>();

            var versions = _contentService.GetVersionsSlim(contentId, 0, 50);

            //Not all nodes are variants & thus culture can be null
            if (culture != null)
            {
                //Get cultures that were published with the version = their update date is equal to the version's
                versions = versions.Where(x => x.UpdateDate == x.GetUpdateDate(culture));
            }

            //First item is our current item/state (cant rollback to ourselves)
            versions = versions.Skip(1);

            foreach (var version in versions)
            {
                var rollbackVersion = new RollbackVersion
                {
                    VersionId = version.VersionId,
                    VersionDate = version.UpdateDate,
                    VersionAuthorId = version.WriterId
                };

                rollbackVersions.Add(rollbackVersion);

                writerIds.Add(version.WriterId);
            }

            var users = _userService
                .GetUsersById(writerIds.ToArray())
                .ToDictionary(x => x.Id, x => x.Name);

            foreach (var rollbackVersion in rollbackVersions)
            {
                if (users.TryGetValue(rollbackVersion.VersionAuthorId, out var userName))
                    rollbackVersion.VersionAuthorName = userName;
            }

            return rollbackVersions;
        }

        [HttpGet]
        public ContentVariantDisplay GetRollbackVersion(int versionId, string culture = null)
        {
            var version = _contentService.GetVersion(versionId);
            var content = MapToDisplay(version);

            return culture == null
                ? content.Variants.FirstOrDefault()  //No culture set - so this is an invariant node - so just list me the first item in here
                : content.Variants.FirstOrDefault(x => x.Language.IsoCode == culture);
        }

        [Authorize(Policy = AuthorizationPolicies.ContentPermissionRollbackById)]
        [HttpPost]
        public IActionResult PostRollbackContent(int contentId, int versionId, string culture = "*")
        {
            var rollbackResult = _contentService.Rollback(contentId, versionId, culture, _backofficeSecurityAccessor.BackOfficeSecurity.GetUserId().ResultOr(0));

            if (rollbackResult.Success)
                return Ok();

            var notificationModel = new SimpleNotificationModel();

            switch (rollbackResult.Result)
            {
                case OperationResultType.Failed:
                case OperationResultType.FailedCannot:
                case OperationResultType.FailedExceptionThrown:
                case OperationResultType.NoOperation:
                default:
                    notificationModel.AddErrorNotification(
                                    _localizedTextService.Localize("speechBubbles/operationFailedHeader"),
                                    null); // TODO: There is no specific failed to save error message AFAIK
                    break;
                case OperationResultType.FailedCancelledByEvent:
                    notificationModel.AddErrorNotification(
                                    _localizedTextService.Localize("speechBubbles/operationCancelledHeader"),
                                    _localizedTextService.Localize("speechBubbles/operationCancelledText"));
                    break;
            }

            return new ValidationErrorResult(notificationModel);
        }

        [Authorize(Policy = AuthorizationPolicies.ContentPermissionProtectById)]
        [HttpGet]
        public IActionResult GetPublicAccess(int contentId)
        {
            var content = _contentService.GetById(contentId);
            if (content == null)
            {
                return NotFound();
            }

            var entry = _publicAccessService.GetEntryForContent(content);
            if (entry == null || entry.ProtectedNodeId != content.Id)
            {
                return Ok();
            }

            var loginPageEntity = _entityService.Get(entry.LoginNodeId, UmbracoObjectTypes.Document);
            var errorPageEntity = _entityService.Get(entry.NoAccessNodeId, UmbracoObjectTypes.Document);

            // unwrap the current public access setup for the client
            // - this API method is the single point of entry for both "modes" of public access (single user and role based)
            var usernames = entry.Rules
                .Where(rule => rule.RuleType == Constants.Conventions.PublicAccess.MemberUsernameRuleType)
                .Select(rule => rule.RuleValue).ToArray();

            var members = usernames
                .Select(username => _memberService.GetByUsername(username))
                .Where(member => member != null)
                .Select(_umbracoMapper.Map<MemberDisplay>)
                .ToArray();

            var allGroups = _memberGroupService.GetAll().ToArray();
            var groups = entry.Rules
                .Where(rule => rule.RuleType == Constants.Conventions.PublicAccess.MemberRoleRuleType)
                .Select(rule => allGroups.FirstOrDefault(g => g.Name == rule.RuleValue))
                .Where(memberGroup => memberGroup != null)
                .Select(_umbracoMapper.Map<MemberGroupDisplay>)
                .ToArray();

            return Ok(new PublicAccess
            {
                Members = members,
                Groups = groups,
                LoginPage = loginPageEntity != null ? _umbracoMapper.Map<EntityBasic>(loginPageEntity) : null,
                ErrorPage = errorPageEntity != null ? _umbracoMapper.Map<EntityBasic>(errorPageEntity) : null
            });
        }

        // set up public access using role based access
        [Authorize(Policy = AuthorizationPolicies.ContentPermissionProtectById)]
        [HttpPost]
        public IActionResult PostPublicAccess(int contentId, [FromQuery(Name = "groups[]")] string[] groups, [FromQuery(Name = "usernames[]")] string[] usernames, int loginPageId, int errorPageId)
        {
            if ((groups == null || groups.Any() == false) && (usernames == null || usernames.Any() == false))
            {
                return BadRequest();
            }

            var content = _contentService.GetById(contentId);
            var loginPage = _contentService.GetById(loginPageId);
            var errorPage = _contentService.GetById(errorPageId);
            if (content == null || loginPage == null || errorPage == null)
            {
                return BadRequest();
            }

            var isGroupBased = groups != null && groups.Any();
            var candidateRuleValues = isGroupBased
                ? groups
                : usernames;
            var newRuleType = isGroupBased
                ? Constants.Conventions.PublicAccess.MemberRoleRuleType
                : Constants.Conventions.PublicAccess.MemberUsernameRuleType;

            var entry = _publicAccessService.GetEntryForContent(content);

            if (entry == null || entry.ProtectedNodeId != content.Id)
            {
                entry = new PublicAccessEntry(content, loginPage, errorPage, new List<PublicAccessRule>());

                foreach (var ruleValue in candidateRuleValues)
                {
                    entry.AddRule(ruleValue, newRuleType);
                }
            }
            else
            {
                entry.LoginNodeId = loginPage.Id;
                entry.NoAccessNodeId = errorPage.Id;

                var currentRules = entry.Rules.ToArray();
                var obsoleteRules = currentRules.Where(rule =>
                    rule.RuleType != newRuleType
                    || candidateRuleValues.Contains(rule.RuleValue) == false
                );
                var newRuleValues = candidateRuleValues.Where(group =>
                    currentRules.Any(rule =>
                        rule.RuleType == newRuleType
                        && rule.RuleValue == group
                    ) == false
                );
                foreach (var rule in obsoleteRules)
                {
                    entry.RemoveRule(rule);
                }
                foreach (var ruleValue in newRuleValues)
                {
                    entry.AddRule(ruleValue, newRuleType);
                }
            }

            return _publicAccessService.Save(entry).Success
                ? (IActionResult)Ok()
                : Problem();
        }

        [Authorize(Policy = AuthorizationPolicies.ContentPermissionProtectById)]
        [HttpPost]
        public IActionResult RemovePublicAccess(int contentId)
        {
            var content = _contentService.GetById(contentId);
            if (content == null)
            {
                return NotFound();
            }

            var entry = _publicAccessService.GetEntryForContent(content);
            if (entry == null)
            {
                return Ok();
            }

            return _publicAccessService.Delete(entry).Success
                ? (IActionResult)Ok()
                : Problem();
        }
    }
}
