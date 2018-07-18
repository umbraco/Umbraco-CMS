using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Binders;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Web.PublishedCache;
using Umbraco.Core.Events;
using Umbraco.Core.Models.Validation;
using Umbraco.Web.Models;
using Umbraco.Web._Legacy.Actions;
using Constants = Umbraco.Core.Constants;
using ContentVariation = Umbraco.Core.Models.ContentVariation;
using Language = Umbraco.Web.Models.ContentEditing.Language;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// The API controller used for editing content
    /// </summary>
    /// <remarks>
    /// This controller is decorated with the UmbracoApplicationAuthorizeAttribute which means that any user requesting
    /// access to ALL of the methods on this controller will need access to the content application.
    /// </remarks>
    [PluginController("UmbracoApi")]
    [UmbracoApplicationAuthorize(Constants.Applications.Content)]
    [ContentControllerConfiguration]
    public class ContentController : ContentControllerBase
    {
        private readonly IPublishedSnapshotService _publishedSnapshotService;

        public ContentController(IPublishedSnapshotService publishedSnapshotService)
        {
            if (publishedSnapshotService == null) throw new ArgumentNullException(nameof(publishedSnapshotService));
            _publishedSnapshotService = publishedSnapshotService;
        }

        /// <summary>
        /// Configures this controller with a custom action selector
        /// </summary>
        private class ContentControllerConfigurationAttribute : Attribute, IControllerConfiguration
        {
            public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
            {
                controllerSettings.Services.Replace(typeof(IHttpActionSelector), new ParameterSwapControllerActionSelector(
                    new ParameterSwapControllerActionSelector.ParameterSwapInfo("GetNiceUrl", "id", typeof(int), typeof(Guid), typeof(Udi)),
                    new ParameterSwapControllerActionSelector.ParameterSwapInfo("GetById", "id", typeof(int), typeof(Guid), typeof(Udi))    
                ));
            }
        }

        /// <summary>
        /// Returns true if any content types have culture variation enabled
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [WebApi.UmbracoAuthorize, OverrideAuthorization]
        public bool AllowsCultureVariation()
        {
            var contentTypes = Services.ContentTypeService.GetAll();
            return contentTypes.Any(contentType => contentType.VariesByCulture());
        }

        /// <summary>
        /// Return content for the specified ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [FilterAllowedOutgoingContent(typeof(IEnumerable<ContentItemDisplay>))]
        public IEnumerable<ContentItemDisplay> GetByIds([FromUri]int[] ids)
        {
            //fixme what about cultures?

            var foundContent = Services.ContentService.GetByIds(ids);
            return foundContent.Select(x => MapToDisplay(x));
        }

        /// <summary>
        /// Updates the permissions for a content item for a particular user group
        /// </summary>
        /// <param name="saveModel"></param>
        /// <returns></returns>
        /// <remarks>
        /// Permission check is done for letter 'R' which is for <see cref="ActionRights"/> which the user must have access to to update
        /// </remarks>
        [EnsureUserPermissionForContent("saveModel.ContentId", 'R')]
        public IEnumerable<AssignedUserGroupPermissions> PostSaveUserGroupPermissions(UserGroupPermissionsSave saveModel)
        {
            if (saveModel.ContentId <= 0) throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));

            //TODO: Should non-admins be alowed to set granular permissions?

            var content = Services.ContentService.GetById(saveModel.ContentId);
            if (content == null) throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));

            //current permissions explicitly assigned to this content item
            var contentPermissions = Services.ContentService.GetPermissions(content)
                .ToDictionary(x => x.UserGroupId, x => x);

            var allUserGroups = Services.UserService.GetAllUserGroups().ToArray();

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
                        Services.UserService.RemoveUserGroupPermissions(userGroup.Id, content.Id);
                    }
                    //check if they are the defaults, if so we should just remove them if they exist since it's more overhead having them stored
                    else if (userGroup.Permissions.UnsortedSequenceEqual(groupPermissionCodes))
                    {
                        //only remove them if they are actually currently assigned
                        if (contentPermissions.ContainsKey(userGroup.Id))
                        {
                            //remove these permissions from this node for this group since the ones being assigned are the same as the defaults
                            Services.UserService.RemoveUserGroupPermissions(userGroup.Id, content.Id);
                        }
                    }
                    //if they are different we need to update, otherwise there's nothing to update
                    else if (contentPermissions.ContainsKey(userGroup.Id) == false || contentPermissions[userGroup.Id].AssignedPermissions.UnsortedSequenceEqual(groupPermissionCodes) == false)
                    {

                        Services.UserService.ReplaceUserGroupPermissions(userGroup.Id, groupPermissionCodes.Select(x => x[0]), content.Id);
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
        /// Permission check is done for letter 'R' which is for <see cref="ActionRights"/> which the user must have access to to view
        /// </remarks>
        [EnsureUserPermissionForContent("contentId", 'R')]
        public IEnumerable<AssignedUserGroupPermissions> GetDetailedPermissions(int contentId)
        {
            if (contentId <= 0) throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            var content = Services.ContentService.GetById(contentId);
            if (content == null) throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));

            //TODO: Should non-admins be able to see detailed permissions?

            var allUserGroups = Services.UserService.GetAllUserGroups();

            return GetDetailedPermissions(content, allUserGroups);
        }

        private IEnumerable<AssignedUserGroupPermissions> GetDetailedPermissions(IContent content, IEnumerable<IUserGroup> allUserGroups)
        {
            //get all user groups and map their default permissions to the AssignedUserGroupPermissions model.
            //we do this because not all groups will have true assigned permissions for this node so if they don't have assigned permissions, we need to show the defaults.

            var defaultPermissionsByGroup = Mapper.Map<IEnumerable<AssignedUserGroupPermissions>>(allUserGroups).ToArray();

            var defaultPermissionsAsDictionary = defaultPermissionsByGroup
                .ToDictionary(x => Convert.ToInt32(x.Id), x => x);

            //get the actual assigned permissions
            var assignedPermissionsByGroup = Services.ContentService.GetPermissions(content).ToArray();

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
        public ContentItemDisplay GetRecycleBin()
        {
            var display = new ContentItemDisplay
            {
                Id = Constants.System.RecycleBinContent,
                Alias = "recycleBin",
                ParentId = -1,
                Name = Services.TextService.Localize("general/recycleBin"),
                ContentTypeAlias = "recycleBin",
                CreateDate = DateTime.Now,
                IsContainer = true,
                Path = "-1," + Constants.System.RecycleBinContent
            };

            TabsAndPropertiesResolver.AddListView(display, "content", Services.DataTypeService, Services.TextService);

            return display;
        }

        //fixme what about cultures?
        public ContentItemDisplay GetBlueprintById(int id)
        {
            var foundContent = Services.ContentService.GetBlueprintById(id);
            if (foundContent == null)
            {
                HandleContentNotFound(id);
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

            var excludeProps = new[] { "_umb_urls", "_umb_releasedate", "_umb_expiredate", "_umb_template" };
            var propsTab = content.Tabs.Last();
            propsTab.Properties = propsTab.Properties
                .Where(p => excludeProps.Contains(p.Alias) == false);
        }

        /// <summary>
        /// Gets the content json for the content id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        [OutgoingEditorModelEvent]
        [EnsureUserPermissionForContent("id")]
        public ContentItemDisplay GetById(int id, string culture = null)
        {
            var foundContent = GetObjectFromRequest(() => Services.ContentService.GetById(id));
            if (foundContent == null)
            {
                HandleContentNotFound(id);
                return null;//irrelevant since the above throws
        /// <summary>
        /// Gets the content json for the content id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OutgoingEditorModelEvent]
        [EnsureUserPermissionForContent("id")]
        public ContentItemDisplay GetById(Guid id)
        {
            var foundContent = GetObjectFromRequest(() => Services.ContentService.GetById(id));
            if (foundContent == null)
            {
                HandleContentNotFound(id);
            }

            var content = AutoMapperExtensions.MapWithUmbracoContext<IContent, ContentItemDisplay>(foundContent, UmbracoContext);
            return content;
        }

        /// <summary>
        /// Gets the content json for the content id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [OutgoingEditorModelEvent]
        [EnsureUserPermissionForContent("id")]
        public ContentItemDisplay GetById(Udi id)
        {
            var guidUdi = id as GuidUdi;
            if (guidUdi != null)
            {
                return GetById(guidUdi.Guid);
            }

            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

            }

            var content = MapToDisplay(foundContent, culture);
            return content;
        }

        /// <summary>
        /// Gets an empty content item for the
        /// </summary>
        /// <param name="contentTypeAlias"></param>
        /// <param name="parentId"></param>
        /// <returns>
        /// If this is a container type, we'll remove the umbContainerView tab for a new item since
        /// it cannot actually list children if it doesn't exist yet.
        /// </returns>
        [OutgoingEditorModelEvent]
        public ContentItemDisplay GetEmpty(string contentTypeAlias, int parentId)
        {
            var contentType = Services.ContentTypeService.Get(contentTypeAlias);
            if (contentType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var emptyContent = Services.ContentService.Create("", parentId, contentType.Alias, Security.GetUserId().ResultOr(0));
            var mapped = MapToDisplay(emptyContent);

            //remove this tab if it exists: umbContainerView
            var containerTab = mapped.Tabs.FirstOrDefault(x => x.Alias == Constants.Conventions.PropertyGroups.ListViewGroupName);
            mapped.Tabs = mapped.Tabs.Except(new[] { containerTab });

            if (contentType.VariesByCulture())
            {
                //Remove all variants except for the default since currently the default must be saved before other variants can be edited
                //TODO: Allow for editing all variants at once ... this will be a future task
                mapped.Variants = new[] { mapped.Variants.FirstOrDefault(x => x.IsCurrent) };
            }

            return mapped;
        }

        [OutgoingEditorModelEvent]
        public ContentItemDisplay GetEmpty(int blueprintId, int parentId)
        {
            var blueprint = Services.ContentService.GetBlueprintById(blueprintId);
            if (blueprint == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            blueprint.Id = 0;
            blueprint.Name = string.Empty;
            blueprint.ParentId = parentId;

            var mapped = Mapper.Map<ContentItemDisplay>(blueprint);

            //remove this tab if it exists: umbContainerView
            var containerTab = mapped.Tabs.FirstOrDefault(x => x.Alias == Constants.Conventions.PropertyGroups.ListViewGroupName);
            mapped.Tabs = mapped.Tabs.Except(new[] { containerTab });
            return mapped;
        }

        /// <summary>
        /// Gets the Url for a given node ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public HttpResponseMessage GetNiceUrl(int id)
        {
            var url = Umbraco.Url(id);
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(url, Encoding.UTF8, "application/json");
            return response;
        }

        /// <summary>
        /// Gets the Url for a given node ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public HttpResponseMessage GetNiceUrl(Guid id)
        {
            var url = Umbraco.UrlProvider.GetUrl(id);
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(url, Encoding.UTF8, "application/json");
            return response;
        }

        /// <summary>
        /// Gets the Url for a given node ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public HttpResponseMessage GetNiceUrl(Udi id)
        {
            var guidUdi = id as GuidUdi;
            if (guidUdi != null)
            {
                return GetNiceUrl(guidUdi.Guid);
            }
            throw new HttpResponseException(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Gets the children for the content id passed in
        /// </summary>
        /// <returns></returns>
        [FilterAllowedOutgoingContent(typeof(IEnumerable<ContentItemBasic<ContentPropertyBasic, IContent>>), "Items")]
        public PagedResult<ContentItemBasic<ContentPropertyBasic, IContent>> GetChildren(
                int id,
                int pageNumber = 0,  //TODO: This should be '1' as it's not the index
                int pageSize = 0,
                string orderBy = "SortOrder",
                Direction orderDirection = Direction.Ascending,
                bool orderBySystemField = true,
                string filter = "")
        {
            return GetChildren(id, null, pageNumber, pageSize, orderBy, orderDirection, orderBySystemField, filter);
        }

        /// <summary>
        /// Gets the children for the content id passed in
        /// </summary>
        /// <returns></returns>
        [FilterAllowedOutgoingContent(typeof(IEnumerable<ContentItemBasic<ContentPropertyBasic, IContent>>), "Items")]
        public PagedResult<ContentItemBasic<ContentPropertyBasic, IContent>> GetChildren(
                int id,
                string includeProperties,
                int pageNumber = 0,  //TODO: This should be '1' as it's not the index
                int pageSize = 0,
                string orderBy = "SortOrder",
                Direction orderDirection = Direction.Ascending,
                bool orderBySystemField = true,
                string filter = "")
        {
            long totalChildren;
            IContent[] children;
            if (pageNumber > 0 && pageSize > 0)
            {
                IQuery<IContent> queryFilter = null;
                if (filter.IsNullOrWhiteSpace() == false)
                {
                    //add the default text filter
                    queryFilter = SqlContext.Query<IContent>()
                        .Where(x => x.Name.Contains(filter));
                }

                children = Services.ContentService
                    .GetPagedChildren(
                        id, (pageNumber - 1), pageSize,
                        out totalChildren,
                        orderBy, orderDirection, orderBySystemField,
                        queryFilter).ToArray();
            }
            else
            {
                children = Services.ContentService.GetChildren(id).ToArray();
                totalChildren = children.Length;
            }

            if (totalChildren == 0)
            {
                return new PagedResult<ContentItemBasic<ContentPropertyBasic, IContent>>(0, 0, 0);
            }

            var pagedResult = new PagedResult<ContentItemBasic<ContentPropertyBasic, IContent>>(totalChildren, pageNumber, pageSize);
            pagedResult.Items = children.Select(content =>
                Mapper.Map<IContent, ContentItemBasic<ContentPropertyBasic, IContent>>(content,
                    opts =>
                    {
                        // if there's a list of property aliases to map - we will make sure to store this in the mapping context.
                        if (String.IsNullOrWhiteSpace(includeProperties) == false)
                        {
                            opts.Items["IncludeProperties"] = includeProperties.Split(new[] { ", ", "," }, StringSplitOptions.RemoveEmptyEntries);
                        }
                    }));

            return pagedResult;
        }

        /// <summary>
        /// Returns permissions for all nodes passed in for the current user
        /// TODO: This should be moved to the CurrentUserController?
        /// </summary>
        /// <param name="nodeIds"></param>
        /// <returns></returns>
        [HttpPost]
        public Dictionary<int, string[]> GetPermissions(int[] nodeIds)
        {
            var permissions = Services.UserService
                    .GetPermissions(Security.CurrentUser, nodeIds);

            var permissionsDictionary = new Dictionary<int, string[]>();
            foreach (var nodeId in nodeIds)
            {
                var aggregatePerms = permissions.GetAllPermissions(nodeId).ToArray();
                permissionsDictionary.Add(nodeId, aggregatePerms);
            }

            return permissionsDictionary;
        }

        /// <summary>
        /// Checks a nodes permission for the current user
        /// TODO: This should be moved to the CurrentUserController?
        /// </summary>
        /// <param name="permissionToCheck"></param>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        [HttpGet]
        public bool HasPermission(string permissionToCheck, int nodeId)
        {
            var p = Services.UserService.GetPermissions(Security.CurrentUser, nodeId).GetAllPermissions();
            if (p.Contains(permissionToCheck.ToString(CultureInfo.InvariantCulture)))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Creates a blueprint from a content item
        /// </summary>
        /// <param name="contentId">The content id to copy</param>
        /// <param name="name">The name of the blueprint</param>
        /// <returns></returns>
        [HttpPost]
        public SimpleNotificationModel CreateBlueprintFromContent([FromUri]int contentId, [FromUri]string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Value cannot be null or whitespace.", "name");

            var content = Services.ContentService.GetById(contentId);
            if (content == null)
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));

            EnsureUniqueName(name, content, "name");

            var blueprint = Services.ContentService.CreateContentFromBlueprint(content, name, Security.GetUserId().ResultOr(0));

            Services.ContentService.SaveBlueprint(blueprint, Security.GetUserId().ResultOr(0));

            var notificationModel = new SimpleNotificationModel();
            notificationModel.AddSuccessNotification(
                Services.TextService.Localize("blueprints/createdBlueprintHeading"),
                Services.TextService.Localize("blueprints/createdBlueprintMessage", new[] { content.Name })
            );

            return notificationModel;
        }

        private void EnsureUniqueName(string name, IContent content, string modelName)
        {
            var existing = Services.ContentService.GetBlueprintsForContentTypes(content.ContentTypeId);
            if (existing.Any(x => x.Name == name && x.Id != content.Id))
            {
                ModelState.AddModelError(modelName, Services.TextService.Localize("blueprints/duplicateBlueprintMessage"));
                throw new HttpResponseException(Request.CreateValidationErrorResponse(ModelState));
            }
        }

        /// <summary>
        /// Saves content
        /// </summary>
        /// <returns></returns>
        [FileUploadCleanupFilter]
        [ContentPostValidate]
        public ContentItemDisplay PostSaveBlueprint([ModelBinder(typeof(ContentItemBinder))] ContentItemSave contentItem)
        {
            var contentItemDisplay = PostSaveInternal(contentItem,
                content =>
                {
                    EnsureUniqueName(content.Name, content, "Name");

                    Services.ContentService.SaveBlueprint(contentItem.PersistedContent, Security.CurrentUser.Id);
                    //we need to reuse the underlying logic so return the result that it wants
                    return OperationResult.Succeed(new EventMessages());
                });
            SetupBlueprint(contentItemDisplay, contentItemDisplay.PersistedContent);

            return contentItemDisplay;
        }

        /// <summary>
        /// Saves content
        /// </summary>
        /// <returns></returns>
        [FileUploadCleanupFilter]
        [ContentPostValidate]
        [OutgoingEditorModelEvent]
        public ContentItemDisplay PostSave([ModelBinder(typeof(ContentItemBinder))] ContentItemSave contentItem)
        {
            var contentItemDisplay = PostSaveInternal(contentItem, content => Services.ContentService.Save(contentItem.PersistedContent, Security.CurrentUser.Id));
            //ensure the active culture is still selected
            if (!contentItem.Culture.IsNullOrWhiteSpace())
            {
                foreach (var contentVariation in contentItemDisplay.Variants)
                {
                    contentVariation.IsCurrent = contentVariation.Language.IsoCode.InvariantEquals(contentItem.Culture);
                }
            }
            return contentItemDisplay;
        }

        private ContentItemDisplay PostSaveInternal(ContentItemSave contentItem, Func<IContent, OperationResult> saveMethod)
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
                if (!RequiredForPersistenceAttribute.HasRequiredValuesForPersistence(contentItem) && IsCreatingAction(contentItem.Action))
                {
                    //ok, so the absolute mandatory data is invalid and it's new, we cannot actually continue!
                    // add the modelstate to the outgoing object and throw a validation message
                    var forDisplay = MapToDisplay(contentItem.PersistedContent, contentItem.Culture);
                    forDisplay.Errors = ModelState.ToErrorDictionary();
                    throw new HttpResponseException(Request.CreateValidationErrorResponse(forDisplay));

                }

                //if the model state is not valid we cannot publish so change it to save
                switch (contentItem.Action)
                {
                    case ContentSaveAction.Publish:
                        contentItem.Action = ContentSaveAction.Save;
                        break;
                    case ContentSaveAction.PublishNew:
                        contentItem.Action = ContentSaveAction.SaveNew;
                        break;
                }
            }

            //initialize this to successful
            var publishStatus = new PublishResult(null, contentItem.PersistedContent);
            var wasCancelled = false;

            if (contentItem.Action == ContentSaveAction.Save || contentItem.Action == ContentSaveAction.SaveNew)
            {
                //save the item
                var saveResult = saveMethod(contentItem.PersistedContent);

                wasCancelled = saveResult.Success == false && saveResult.Result == OperationResultType.FailedCancelledByEvent;
            }
            else if (contentItem.Action == ContentSaveAction.SendPublish || contentItem.Action == ContentSaveAction.SendPublishNew)
            {
                var sendResult = Services.ContentService.SendToPublication(contentItem.PersistedContent, Security.CurrentUser.Id);
                wasCancelled = sendResult == false;
            }
            else
            {
                PublishInternal(contentItem, ref publishStatus, ref wasCancelled);
            }

            //get the updated model
            var display = MapToDisplay(contentItem.PersistedContent, contentItem.Culture);

            //lasty, if it is not valid, add the modelstate to the outgoing object and throw a 403
            HandleInvalidModelState(display);

            //put the correct msgs in
            switch (contentItem.Action)
            {
                case ContentSaveAction.Save:
                case ContentSaveAction.SaveNew:
                    if (wasCancelled == false)
                    {
                        display.AddSuccessNotification(
                            Services.TextService.Localize("speechBubbles/editContentSavedHeader"),
                            Services.TextService.Localize("speechBubbles/editContentSavedText"));
                    }
                    else
                    {
                        AddCancelMessage(display);
                    }
                    break;
                case ContentSaveAction.SendPublish:
                case ContentSaveAction.SendPublishNew:
                    if (wasCancelled == false)
                    {
                        display.AddSuccessNotification(
                            Services.TextService.Localize("speechBubbles/editContentSendToPublish"),
                            Services.TextService.Localize("speechBubbles/editContentSendToPublishText"));
                    }
                    else
                    {
                        AddCancelMessage(display);
                    }
                    break;
                case ContentSaveAction.Publish:
                case ContentSaveAction.PublishNew:
                    ShowMessageForPublishStatus(publishStatus, display);
                    break;
            }

            //If the item is new and the operation was cancelled, we need to return a different
            // status code so the UI can handle it since it won't be able to redirect since there
            // is no Id to redirect to!
            if (wasCancelled && IsCreatingAction(contentItem.Action))
            {
                throw new HttpResponseException(Request.CreateValidationErrorResponse(display));
            }

            display.PersistedContent = contentItem.PersistedContent;

            return display;
        }

        /// <summary>
        /// Performs the publishing operation for a content item
        /// </summary>
        /// <param name="contentItem"></param>
        /// <param name="publishStatus"></param>
        /// <param name="wasCancelled"></param>
        /// <remarks>
        /// If this is a culture variant than we need to do some validation, if it's not we'll publish as normal
        /// </remarks>
        private void PublishInternal(ContentItemSave contentItem, ref PublishResult publishStatus, ref bool wasCancelled)
        {
            if (!contentItem.PersistedContent.ContentType.VariesByCulture())
            {
                //its invariant, proceed normally
                publishStatus = Services.ContentService.SaveAndPublish(contentItem.PersistedContent, userId: Security.CurrentUser.Id);
                wasCancelled = publishStatus.Result == PublishResultType.FailedCancelledByEvent;
            }
            else
            {
                var canPublish = true;

                //check if we are publishing other variants and validate them
                var allLangs = Services.LocalizationService.GetAllLanguages().ToDictionary(x => x.IsoCode, x => x, StringComparer.InvariantCultureIgnoreCase);
                var otherVariantsToValidate = contentItem.PublishVariations.Where(x => !x.Culture.InvariantEquals(contentItem.Culture)).ToList();

                //validate any mandatory variants that are not in the list
                var mandatoryLangs = Mapper.Map<IEnumerable<ILanguage>, IEnumerable<Language>>(allLangs.Values)
                    .Where(x => otherVariantsToValidate.All(v => !v.Culture.InvariantEquals(x.IsoCode))) //don't include variants above
                    .Where(x => !x.IsoCode.InvariantEquals(contentItem.Culture)) //don't include the current variant
                    .Where(x => x.Mandatory);
                foreach (var lang in mandatoryLangs)
                {
                    //cannot continue publishing since a required language that is not currently being published isn't published
                    if (!contentItem.PersistedContent.IsCulturePublished(lang.IsoCode))
                    {
                        var errMsg = Services.TextService.Localize("speechBubbles/contentReqCulturePublishError", new[] { allLangs[lang.IsoCode].CultureName });
                        ModelState.AddModelError("publish_variant_" + lang.IsoCode + "_", errMsg);
                        canPublish = false;
                    }
                }

                if (canPublish)
                {
                    //validate all other variants to be published
                    foreach (var publishVariation in otherVariantsToValidate)
                    {
                        //validate the content item and the culture property values, we don't need to validate any invariant property values here because they will have
                        //been validated in the post.
                        var valid = contentItem.PersistedContent.IsValid(publishVariation.Culture);
                        if (!valid)
                        {
                            var errMsg = Services.TextService.Localize("speechBubbles/contentCultureValidationError", new[] { allLangs[publishVariation.Culture].CultureName });
                            ModelState.AddModelError("publish_variant_" + publishVariation.Culture + "_", errMsg);
                            canPublish = false;
                        }
                    }
                }

                if (canPublish)
                {
                    //try to publish all the values on the model
                    canPublish = PublishCulture(contentItem, otherVariantsToValidate, allLangs);
                }

                if (canPublish)
                {
                    //proceed to publish if all validation still succeeds
                    publishStatus = Services.ContentService.SavePublishing(contentItem.PersistedContent, Security.CurrentUser.Id);
                    wasCancelled = publishStatus.Result == PublishResultType.FailedCancelledByEvent;
                }
                else
                {
                    //can only save
                    var saveResult = Services.ContentService.Save(contentItem.PersistedContent, Security.CurrentUser.Id);
                    publishStatus = new PublishResult(PublishResultType.FailedCannotPublish, null, contentItem.PersistedContent);
                    wasCancelled = saveResult.Result == OperationResultType.FailedCancelledByEvent;
                }
            }
        }

        /// <summary>
        /// This will call TryPublishValues on the content item for each culture that needs to be published including the invariant culture
        /// </summary>
        /// <param name="contentItem"></param>
        /// <param name="otherVariantsToValidate"></param>
        /// <param name="allLangs"></param>
        /// <returns></returns>
        private bool PublishCulture(ContentItemSave contentItem, IEnumerable<ContentVariationPublish> otherVariantsToValidate, IDictionary<string, ILanguage> allLangs)
        {
            var culturesToPublish = new List<string> { contentItem.Culture };
            culturesToPublish.AddRange(otherVariantsToValidate.Select(x => x.Culture));

            foreach(var culture in culturesToPublish)
            {
                // publishing any culture, implies the invariant culture
                var valid = contentItem.PersistedContent.PublishCulture(culture);
                if (!valid)
                {
                    var errMsg = Services.TextService.Localize("speechBubbles/contentCultureUnexpectedValidationError", new[] { allLangs[culture].CultureName });
                    ModelState.AddModelError("publish_variant_" + culture + "_", errMsg);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Publishes a document with a given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <remarks>
        /// The CanAccessContentAuthorize attribute will deny access to this method if the current user
        /// does not have Publish access to this node.
        /// </remarks>
        ///
        [EnsureUserPermissionForContent("id", 'U')]
        public HttpResponseMessage PostPublishById(int id)
        {
            var foundContent = GetObjectFromRequest(() => Services.ContentService.GetById(id));

            if (foundContent == null)
            {
                return HandleContentNotFound(id, false);
            }

            var publishResult = Services.ContentService.SavePublishing(foundContent, Security.GetUserId().ResultOr(0));
            if (publishResult.Success == false)
            {
                var notificationModel = new SimpleNotificationModel();
                ShowMessageForPublishStatus(publishResult, notificationModel);
                return Request.CreateValidationErrorResponse(notificationModel);
            }

            //return ok
            return Request.CreateResponse(HttpStatusCode.OK);

        }

        [HttpDelete]
        [HttpPost]
        public HttpResponseMessage DeleteBlueprint(int id)
        {
            var found = Services.ContentService.GetBlueprintById(id);

            if (found == null)
            {
                return HandleContentNotFound(id, false);
            }

            Services.ContentService.DeleteBlueprint(found);

            return Request.CreateResponse(HttpStatusCode.OK);
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
        [EnsureUserPermissionForContent("id", 'D')]
        [HttpDelete]
        [HttpPost]
        public HttpResponseMessage DeleteById(int id)
        {
            var foundContent = GetObjectFromRequest(() => Services.ContentService.GetById(id));

            if (foundContent == null)
            {
                return HandleContentNotFound(id, false);
            }

            //if the current item is in the recycle bin
            if (foundContent.Trashed == false)
            {
                var moveResult = Services.ContentService.MoveToRecycleBin(foundContent, Security.GetUserId().ResultOr(0));
                if (moveResult.Success == false)
                {
                    //returning an object of INotificationModel will ensure that any pending
                    // notification messages are added to the response.
                    return Request.CreateValidationErrorResponse(new SimpleNotificationModel());
                }
            }
            else
            {
                var deleteResult = Services.ContentService.Delete(foundContent, Security.GetUserId().ResultOr(0));
                if (deleteResult.Success == false)
                {
                    //returning an object of INotificationModel will ensure that any pending
                    // notification messages are added to the response.
                    return Request.CreateValidationErrorResponse(new SimpleNotificationModel());
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK);
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
        [EnsureUserPermissionForContent(Constants.System.RecycleBinContent)]
        public HttpResponseMessage EmptyRecycleBin()
        {
            Services.ContentService.EmptyRecycleBin();

            return Request.CreateNotificationSuccessResponse(Services.TextService.Localize("defaultdialogs/recycleBinIsEmpty"));
        }

        /// <summary>
        /// Change the sort order for content
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

            try
            {
                var contentService = Services.ContentService;

                // Save content with new sort order and update content xml in db accordingly
                if (contentService.Sort(sorted.IdSortOrder) == false)
                {
                    Logger.Warn<ContentController>("Content sorting failed, this was probably caused by an event being cancelled");
                    return Request.CreateValidationErrorResponse("Content sorting failed, this was probably caused by an event being cancelled");
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Logger.Error<ContentController>("Could not update content sort order", ex);
                throw;
            }
        }

        /// <summary>
        /// Change the sort order for media
        /// </summary>
        /// <param name="move"></param>
        /// <returns></returns>
        [EnsureUserPermissionForContent("move.ParentId", 'M')]
        public HttpResponseMessage PostMove(MoveOrCopy move)
        {
            var toMove = ValidateMoveOrCopy(move);

            Services.ContentService.Move(toMove, move.ParentId, Security.GetUserId().ResultOr(0));

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(toMove.Path, Encoding.UTF8, "application/json");
            return response;
        }

        /// <summary>
        /// Copies a content item and places the copy as a child of a given parent Id
        /// </summary>
        /// <param name="copy"></param>
        /// <returns></returns>
        [EnsureUserPermissionForContent("copy.ParentId", 'C')]
        public HttpResponseMessage PostCopy(MoveOrCopy copy)
        {
            var toCopy = ValidateMoveOrCopy(copy);

            var c = Services.ContentService.Copy(toCopy, copy.ParentId, copy.RelateToOriginal, copy.Recursive, Security.GetUserId().ResultOr(0));

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(c.Path, Encoding.UTF8, "application/json");
            return response;
        }

        /// <summary>
        /// Unpublishes a node with a given Id and returns the unpublished entity
        /// </summary>
        /// <param name="id">The content id to unpublish</param>
        /// <param name="id">The culture variant for the content id to unpublish, if none specified will unpublish all variants of the content</param>
        /// <returns></returns>
        [EnsureUserPermissionForContent("id", 'U')]
        [OutgoingEditorModelEvent]
        public ContentItemDisplay PostUnPublish(int id, string culture = null)
        {
            var foundContent = GetObjectFromRequest(() => Services.ContentService.GetById(id));

            if (foundContent == null)
                HandleContentNotFound(id);

            var unpublishResult = Services.ContentService.Unpublish(foundContent, culture: culture, userId: Security.GetUserId().ResultOr(0));

            var content = MapToDisplay(foundContent, culture);

            if (!unpublishResult.Success)
            {
                AddCancelMessage(content);
                throw new HttpResponseException(Request.CreateValidationErrorResponse(content));
            }
            else
            {
                //fixme should have a better localized method for when we have the UnpublishResultType.SuccessMandatoryCulture status

                content.AddSuccessNotification(
                    Services.TextService.Localize("content/unPublish"),
                    unpublishResult.Result == UnpublishResultType.SuccessCulture
                        ? Services.TextService.Localize("speechBubbles/contentVariationUnpublished", new[] { culture })
                        : Services.TextService.Localize("speechBubbles/contentUnpublished"));

                return content;
            }
        }

        /// <summary>
        /// Maps the dto property values to the persisted model
        /// </summary>
        /// <param name="contentItem"></param>
        private void MapPropertyValues(ContentItemSave contentItem)
        {
            //Don't update the name if it is empty
            if (!contentItem.Name.IsNullOrWhiteSpace())
            {
                if (contentItem.PersistedContent.ContentType.VariesByCulture())
                {
                    if (contentItem.Culture.IsNullOrWhiteSpace())
                        throw new InvalidOperationException($"Cannot set culture name without a culture.");
                    contentItem.PersistedContent.SetCultureName(contentItem.Name, contentItem.Culture);
                }
                else
                {
                    contentItem.PersistedContent.Name = contentItem.Name;
                }
            }

            //TODO: We need to support 'send to publish'

            contentItem.PersistedContent.ExpireDate = contentItem.ExpireDate;
            contentItem.PersistedContent.ReleaseDate = contentItem.ReleaseDate;
            //only set the template if it didn't change
            var templateChanged = (contentItem.PersistedContent.Template == null && contentItem.TemplateAlias.IsNullOrWhiteSpace() == false)
                                                        || (contentItem.PersistedContent.Template != null && contentItem.PersistedContent.Template.Alias != contentItem.TemplateAlias)
                                                        || (contentItem.PersistedContent.Template != null && contentItem.TemplateAlias.IsNullOrWhiteSpace());
            if (templateChanged)
            {
                var template = Services.FileService.GetTemplate(contentItem.TemplateAlias);
                if (template == null && contentItem.TemplateAlias.IsNullOrWhiteSpace() == false)
                {
                    //ModelState.AddModelError("Template", "No template exists with the specified alias: " + contentItem.TemplateAlias);
                    Logger.Warn<ContentController>("No template exists with the specified alias: " + contentItem.TemplateAlias);
                }
                else
                {
                    //NOTE: this could be null if there was a template and the posted template is null, this should remove the assigned template
                    contentItem.PersistedContent.Template = template;
                }
            }

            bool Varies(Property property) => property.PropertyType.VariesByCulture();

            MapPropertyValues<IContent, ContentItemSave>(
                contentItem,
                (save, property) => Varies(property) ? property.GetValue(save.Culture) : property.GetValue(),         //get prop val
                (save, property, v) => { if (Varies(property)) property.SetValue(v, save.Culture); else property.SetValue(v); });  //set prop val
        }

        /// <summary>
        /// Ensures the item can be moved/copied to the new location
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private IContent ValidateMoveOrCopy(MoveOrCopy model)
        {
            if (model == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var contentService = Services.ContentService;
            var toMove = contentService.GetById(model.Id);
            if (toMove == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            if (model.ParentId < 0)
            {
                //cannot move if the content item is not allowed at the root
                if (toMove.ContentType.AllowedAsRoot == false)
                {
                    throw new HttpResponseException(
                            Request.CreateNotificationValidationErrorResponse(
                                    Services.TextService.Localize("moveOrCopy/notAllowedAtRoot")));
                }
            }
            else
            {
                var parent = contentService.GetById(model.ParentId);
                if (parent == null)
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }

                //check if the item is allowed under this one
                if (parent.ContentType.AllowedContentTypes.Select(x => x.Id).ToArray()
                        .Any(x => x.Value == toMove.ContentType.Id) == false)
                {
                    throw new HttpResponseException(
                            Request.CreateNotificationValidationErrorResponse(
                                    Services.TextService.Localize("moveOrCopy/notAllowedByContentType")));
                }

                // Check on paths
                if ((string.Format(",{0},", parent.Path)).IndexOf(string.Format(",{0},", toMove.Id), StringComparison.Ordinal) > -1)
                {
                    throw new HttpResponseException(
                            Request.CreateNotificationValidationErrorResponse(
                                    Services.TextService.Localize("moveOrCopy/notAllowedByPath")));
                }
            }

            return toMove;
        }

        private void ShowMessageForPublishStatus(PublishResult status, INotificationModel display)
        {
            switch (status.Result)
            {
                case PublishResultType.Success:
                case PublishResultType.SuccessAlready:
                    display.AddSuccessNotification(
                            Services.TextService.Localize("speechBubbles/editContentPublishedHeader"),
                            Services.TextService.Localize("speechBubbles/editContentPublishedText"));
                    break;
                case PublishResultType.FailedPathNotPublished:
                    display.AddWarningNotification(
                            Services.TextService.Localize("publish"),
                            Services.TextService.Localize("publish/contentPublishedFailedByParent",
                                new[] { $"{status.Content.Name} ({status.Content.Id})" }).Trim());
                    break;
                case PublishResultType.FailedCancelledByEvent:
                    AddCancelMessage(display, "publish", "speechBubbles/contentPublishedFailedByEvent");
                    break;
                case PublishResultType.FailedAwaitingRelease:
                    display.AddWarningNotification(
                            Services.TextService.Localize("publish"),
                            Services.TextService.Localize("publish/contentPublishedFailedAwaitingRelease",
                                new[] { $"{status.Content.Name} ({status.Content.Id})" }).Trim());
                    break;
                case PublishResultType.FailedHasExpired:
                    display.AddWarningNotification(
                            Services.TextService.Localize("publish"),
                            Services.TextService.Localize("publish/contentPublishedFailedExpired",
                                new[] { $"{status.Content.Name} ({status.Content.Id})", }).Trim());
                    break;
                case PublishResultType.FailedIsTrashed:
                    display.AddWarningNotification(
                        Services.TextService.Localize("publish"),
                        "publish/contentPublishedFailedIsTrashed"); // fixme properly localize!
                    break;
                case PublishResultType.FailedContentInvalid:
                    display.AddWarningNotification(
                            Services.TextService.Localize("publish"),
                            Services.TextService.Localize("publish/contentPublishedFailedInvalid",
                                new[]
                                {
                                    $"{status.Content.Name} ({status.Content.Id})",
                                    string.Join(",", status.InvalidProperties.Select(x => x.Alias))
                                }).Trim());
                    break;
                case PublishResultType.FailedByCulture:
                    display.AddWarningNotification(
                        Services.TextService.Localize("publish"),
                        "publish/contentPublishedFailedByCulture"); // fixme properly localize!
                    break;
                default:
                    throw new IndexOutOfRangeException($"PublishedResultType \"{status.Result}\" was not expected.");
            }
        }

        /// <summary>
        /// Performs a permissions check for the user to check if it has access to the node based on
        /// start node and/or permissions for the node
        /// </summary>
        /// <param name="storage">The storage to add the content item to so it can be reused</param>
        /// <param name="user"></param>
        /// <param name="userService"></param>
        /// <param name="contentService"></param>
        /// <param name="entityService"></param>
        /// <param name="nodeId">The content to lookup, if the contentItem is not specified</param>
        /// <param name="permissionsToCheck"></param>
        /// <param name="contentItem">Specifies the already resolved content item to check against</param>
        /// <returns></returns>
        internal static bool CheckPermissions(
                IDictionary<string, object> storage,
                IUser user,
                IUserService userService,
                IContentService contentService,
                IEntityService entityService,
                int nodeId,
                char[] permissionsToCheck = null,
                IContent contentItem = null)
        {
            if (storage == null) throw new ArgumentNullException("storage");
            if (user == null) throw new ArgumentNullException("user");
            if (userService == null) throw new ArgumentNullException("userService");
            if (contentService == null) throw new ArgumentNullException("contentService");
            if (entityService == null) throw new ArgumentNullException("entityService");

            if (contentItem == null && nodeId != Constants.System.Root && nodeId != Constants.System.RecycleBinContent)
            {
                contentItem = contentService.GetById(nodeId);
                //put the content item into storage so it can be retreived
                // in the controller (saves a lookup)
                storage[typeof(IContent).ToString()] = contentItem;
            }

            if (contentItem == null && nodeId != Constants.System.Root && nodeId != Constants.System.RecycleBinContent)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var hasPathAccess = (nodeId == Constants.System.Root)
                ? user.HasContentRootAccess(entityService)
                : (nodeId == Constants.System.RecycleBinContent)
                    ? user.HasContentBinAccess(entityService)
                    : user.HasPathAccess(contentItem, entityService);

            if (hasPathAccess == false)
            {
                return false;
            }

            if (permissionsToCheck == null || permissionsToCheck.Length == 0)
            {
                return true;
            }

            //get the implicit/inherited permissions for the user for this path,
            //if there is no content item for this id, than just use the id as the path (i.e. -1 or -20)
            var path = contentItem != null ? contentItem.Path : nodeId.ToString();
            var permission = userService.GetPermissionsForPath(user, path);

            var allowed = true;
            foreach (var p in permissionsToCheck)
            {
                if (permission == null
                    || permission.GetAllPermissions().Contains(p.ToString(CultureInfo.InvariantCulture)) == false)
                {
                    allowed = false;
                }
            }
            return allowed;
        }

        /// <summary>
        /// Used to map an <see cref="IContent"/> instance to a <see cref="ContentItemDisplay"/> and ensuring a language is present if required
        /// </summary>
        /// <param name="content"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        private ContentItemDisplay MapToDisplay(IContent content, string culture = null)
        {
            //A culture must exist in the mapping context if this content type is CultureNeutral since for a culture variant to be edited,
            // the Cuture property of ContentItemDisplay must exist (at least currently).
            if (culture == null && content.ContentType.VariesByCulture())
            {
                //If a culture is not explicitly sent up, then it means that the user is editing the default variant language.
                culture = Services.LocalizationService.GetDefaultLanguageIsoCode();
            }

            var display = ContextMapper.Map<IContent, ContentItemDisplay>(content, UmbracoContext,
                new Dictionary<string, object> { { ContextMapper.CultureKey, culture } });

            return display;
        }
    }
}
