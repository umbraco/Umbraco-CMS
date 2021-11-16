using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Security;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Events;
using Umbraco.Core.Models.ContentEditing;
using Umbraco.Core.Models.Validation;
using Umbraco.Web.Composing;
using Constants = Umbraco.Core.Constants;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Actions;
using Umbraco.Web.ContentApps;
using Umbraco.Web.Editors.Binders;
using Umbraco.Web.Editors.Filters;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence;
using Umbraco.Core.Security;
using Umbraco.Web.Routing;
using Umbraco.Core.Mapping;
using Umbraco.Core.Scoping;

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
        private readonly PropertyEditorCollection _propertyEditors;
        private readonly Lazy<IDictionary<string, ILanguage>> _allLangs;
        private readonly IScopeProvider _scopeProvider;

        public object Domains { get; private set; }

        public ContentController(PropertyEditorCollection propertyEditors, IGlobalSettings globalSettings,
            IUmbracoContextAccessor umbracoContextAccessor, ISqlContext sqlContext, ServiceContext services,
            AppCaches appCaches, IProfilingLogger logger, IRuntimeState runtimeState, UmbracoHelper umbracoHelper,
            IScopeProvider scopeProvider)
            : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoHelper)
        {
            _propertyEditors = propertyEditors ?? throw new ArgumentNullException(nameof(propertyEditors));
            _allLangs = new Lazy<IDictionary<string, ILanguage>>(() => Services.LocalizationService.GetAllLanguages().ToDictionary(x => x.IsoCode, x => x, StringComparer.InvariantCultureIgnoreCase));
            _scopeProvider = scopeProvider;
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
        public IEnumerable<ContentItemDisplay> GetByIds([FromUri] int[] ids)
        {
            var foundContent = Services.ContentService.GetByIds(ids);
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
        [EnsureUserPermissionForContent("saveModel.ContentId", 'R')]
        public IEnumerable<AssignedUserGroupPermissions> PostSaveUserGroupPermissions(UserGroupPermissionsSave saveModel)
        {
            if (saveModel.ContentId <= 0) throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));

            // TODO: Should non-admins be allowed to set granular permissions?

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
        /// Permission check is done for letter 'R' which is for <see cref="ActionRights"/> which the user must have access to view
        /// </remarks>
        [EnsureUserPermissionForContent("contentId", 'R')]
        public IEnumerable<AssignedUserGroupPermissions> GetDetailedPermissions(int contentId)
        {
            if (contentId <= 0) throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            var content = Services.ContentService.GetById(contentId);
            if (content == null) throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));

            // TODO: Should non-admins be able to see detailed permissions?

            var allUserGroups = Services.UserService.GetAllUserGroups();

            return GetDetailedPermissions(content, allUserGroups);
        }

        private IEnumerable<AssignedUserGroupPermissions> GetDetailedPermissions(IContent content, IEnumerable<IUserGroup> allUserGroups)
        {
            //get all user groups and map their default permissions to the AssignedUserGroupPermissions model.
            //we do this because not all groups will have true assigned permissions for this node so if they don't have assigned permissions, we need to show the defaults.

            var defaultPermissionsByGroup = Mapper.MapEnumerable<IUserGroup, AssignedUserGroupPermissions>(allUserGroups);

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
            var apps = new List<ContentApp>();
            apps.Add(ListViewContentAppFactory.CreateContentApp(Services.DataTypeService, _propertyEditors, "recycleBin", "content", Core.Constants.DataTypes.DefaultMembersListView));
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
                        Name = Services.TextService.Localize("general","recycleBin")
                    }
                },
                ContentApps = apps
            };

            return display;
        }

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
        [EnsureUserPermissionForContent("id")]
        public ContentItemDisplay GetById(int id)
        {
            var foundContent = GetObjectFromRequest(() => Services.ContentService.GetById(id));
            if (foundContent == null)
            {
                HandleContentNotFound(id);
                return null;//irrelevant since the above throws
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
        [EnsureUserPermissionForContent("id")]
        public ContentItemDisplay GetById(Guid id)
        {
            var foundContent = GetObjectFromRequest(() => Services.ContentService.GetById(id));
            if (foundContent == null)
            {
                HandleContentNotFound(id);
                return null;//irrelevant since the above throws
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

        /// <summary>
        /// Gets an empty content item for the document type.
        /// </summary>
        /// <param name="contentTypeAlias"></param>
        /// <param name="parentId"></param>
        [OutgoingEditorModelEvent]
        public ContentItemDisplay GetEmpty(string contentTypeAlias, int parentId)
        {
            var contentType = Services.ContentTypeService.Get(contentTypeAlias);
            if (contentType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return GetEmpty(contentType, parentId);
        }

        /// <summary>
        /// Gets a dictionary containing empty content items for every alias specified in the contentTypeAliases array in the body of the request.
        /// </summary>
        /// <remarks>
        /// This is a post request in order to support a large amount of aliases without hitting the URL length limit.
        /// </remarks>
        /// <param name="contentTypesByAliases"></param>
        /// <returns></returns>
        [OutgoingEditorModelEvent]
        [HttpPost]
        public IDictionary<string, ContentItemDisplay> GetEmptyByAliases(ContentTypesByAliases contentTypesByAliases)
        {
            // It's important to do this operation within a scope to reduce the amount of readlock queries. 
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var contentTypes = contentTypesByAliases.ContentTypeAliases.Select(alias => Services.ContentTypeService.Get(alias));
            return GetEmpties(contentTypes, contentTypesByAliases.ParentId).ToDictionary(x => x.ContentTypeAlias);
        }


        /// <summary>
        /// Gets an empty content item for the document type.
        /// </summary>
        /// <param name="contentTypeKey"></param>
        /// <param name="parentId"></param>
        [OutgoingEditorModelEvent]
        public ContentItemDisplay GetEmptyByKey(Guid contentTypeKey, int parentId)
        {
            using (var scope = _scopeProvider.CreateScope())
            {
                var contentType = Services.ContentTypeService.Get(contentTypeKey);
                if (contentType == null)
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }

                var contentItem = GetEmpty(contentType, parentId);
                scope.Complete();

                return contentItem;
            }
        }

        private ContentItemDisplay CleanContentItemDisplay(ContentItemDisplay display)
        {
            // translate the content type name if applicable
            display.ContentTypeName = Services.TextService.UmbracoDictionaryTranslate(display.ContentTypeName);
            // if your user type doesn't have access to the Settings section it would not get this property mapped
            if (display.DocumentType != null)
                display.DocumentType.Name = Services.TextService.UmbracoDictionaryTranslate(display.DocumentType.Name);

            //remove the listview app if it exists
            display.ContentApps = display.ContentApps.Where(x => x.Alias != "umbListView").ToList();
            return display;
        }

        private ContentItemDisplay GetEmpty(IContentType contentType, int parentId)
        {
            var emptyContent = Services.ContentService.Create("", parentId, contentType, Security.GetUserId().ResultOr(0));
            var mapped = MapToDisplay(emptyContent);
            return CleanContentItemDisplay(mapped);
        }

        /// <summary>
        /// Gets an empty <see cref="ContentItemDisplay"/> for each content type in the IEnumerable, all with the same parent ID
        /// </summary>
        /// <remarks>Will attempt to re-use the same permissions for every content as long as the path and user are the same</remarks>
        /// <param name="contentTypes"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        private IEnumerable<ContentItemDisplay> GetEmpties(IEnumerable<IContentType> contentTypes, int parentId)
        {
            var result = new List<ContentItemDisplay>();
            var userId = Security.GetUserId().ResultOr(0);
            var currentUser = Security.CurrentUser;
            // We know that if the ID is less than 0 the parent is null.
            // Since this is called with parent ID it's safe to assume that the parent is the same for all the content types.
            var parent = parentId > 0 ? Services.ContentService.GetById(parentId) : null;
            // Since the parent is the same and the path used to get permissions is based on the parent we only have to do it once
            var path = parent == null ? "-1" : parent.Path;
            var permissions = new Dictionary<string, EntityPermissionSet>
            {
                [path] = Services.UserService.GetPermissionsForPath(currentUser, path)
            };

            foreach (var contentType in contentTypes)
            {
                var emptyContent = Services.ContentService.Create("", parentId, contentType, userId);

                var mapped = MapToDisplay(emptyContent, context =>
                {
                    // Since the permissions depend on current user and path, we add both of these to context as well,
                    // that way we can compare the path and current user when mapping, if they're the same just take permissions
                    // and skip getting them again, in theory they should always be the same, but better safe than sorry.,
                    context.Items["Parent"] = parent;
                    context.Items["CurrentUser"] = currentUser;
                    context.Items["Permissions"] = permissions;
                });
                result.Add(CleanContentItemDisplay(mapped));
            }

            return result;
        }

        private IDictionary<Guid, ContentItemDisplay> GetEmptyByKeysInternal(Guid[] contentTypeKeys, int parentId)
        {
            using var scope = _scopeProvider.CreateScope(autoComplete: true);
            var contentTypes = Services.ContentTypeService.GetAll(contentTypeKeys).ToList();
            return GetEmpties(contentTypes, parentId).ToDictionary(x => x.ContentTypeKey);
        }

        /// <summary>
        /// Gets a collection of empty content items for all document types.
        /// </summary>
        /// <param name="contentTypeKeys"></param>
        /// <param name="parentId"></param>
        [OutgoingEditorModelEvent]
        public IDictionary<Guid, ContentItemDisplay> GetEmptyByKeys([FromUri] Guid[] contentTypeKeys, [FromUri] int parentId)
        {
            return GetEmptyByKeysInternal(contentTypeKeys, parentId);
        }

        /// <summary>
        /// Gets a collection of empty content items for all document types.
        /// </summary>
        /// <remarks>
        /// This is a post request in order to support a large amount of GUIDs without hitting the URL length limit.
        /// </remarks>
        /// <param name="contentTypeByKeys"></param>
        /// <returns></returns>
        [HttpPost]
        [OutgoingEditorModelEvent]
        public IDictionary<Guid, ContentItemDisplay> GetEmptyByKeys(ContentTypesByKeys contentTypeByKeys)
        {
            return GetEmptyByKeysInternal(contentTypeByKeys.ContentTypeKeys, contentTypeByKeys.ParentId);
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

            //remove the listview app if it exists
            mapped.ContentApps = mapped.ContentApps.Where(x => x.Alias != "umbListView").ToList();

            return mapped;
        }

        /// <summary>
        /// Gets the Url for a given node ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public HttpResponseMessage GetNiceUrl(int id)
        {
            var url = UmbracoContext.Url(id);
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(url, Encoding.UTF8, "text/plain");
            return response;
        }

        /// <summary>
        /// Gets the Url for a given node ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public HttpResponseMessage GetNiceUrl(Guid id)
        {
            var url = UmbracoContext.Url(id);
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(url, Encoding.UTF8, "text/plain");
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
        [FilterAllowedOutgoingContent(typeof(IEnumerable<ContentItemBasic<ContentPropertyBasic>>), "Items")]
        public PagedResult<ContentItemBasic<ContentPropertyBasic>> GetChildren(
                int id,
                int pageNumber = 0,  // TODO: This should be '1' as it's not the index
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
                    queryFilter = SqlContext.Query<IContent>()
                        .Where(x => x.Name.Contains(filter));
                }

                children = Services.ContentService
                    .GetPagedChildren(id, pageNumber - 1, pageSize, out totalChildren,
                        queryFilter,
                        Ordering.By(orderBy, orderDirection, cultureName, !orderBySystemField)).ToList();
            }
            else
            {
                //better to not use this without paging where possible, currently only the sort dialog does
                children = Services.ContentService.GetPagedChildren(id, 0, int.MaxValue, out var total).ToList();
                totalChildren = children.Count;
            }

            if (totalChildren == 0)
            {
                return new PagedResult<ContentItemBasic<ContentPropertyBasic>>(0, 0, 0);
            }

            var pagedResult = new PagedResult<ContentItemBasic<ContentPropertyBasic>>(totalChildren, pageNumber, pageSize);
            pagedResult.Items = children.Select(content =>
                Mapper.Map<IContent, ContentItemBasic<ContentPropertyBasic>>(content,
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
        public SimpleNotificationModel CreateBlueprintFromContent([FromUri] int contentId, [FromUri] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));

            var content = Services.ContentService.GetById(contentId);
            if (content == null)
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));

            EnsureUniqueName(name, content, nameof(name));

            var blueprint = Services.ContentService.CreateContentFromBlueprint(content, name, Security.GetUserId().ResultOr(0));

            Services.ContentService.SaveBlueprint(blueprint, Security.GetUserId().ResultOr(0));

            var notificationModel = new SimpleNotificationModel();
            notificationModel.AddSuccessNotification(
                Services.TextService.Localize("blueprints", "createdBlueprintHeading"),
                Services.TextService.Localize("blueprints", "createdBlueprintMessage", new[] { content.Name })
            );

            return notificationModel;
        }

        private void EnsureUniqueName(string name, IContent content, string modelName)
        {
            var existing = Services.ContentService.GetBlueprintsForContentTypes(content.ContentTypeId);
            if (existing.Any(x => x.Name == name && x.Id != content.Id))
            {
                ModelState.AddModelError(modelName, Services.TextService.Localize("blueprints", "duplicateBlueprintMessage"));
                throw new HttpResponseException(Request.CreateValidationErrorResponse(ModelState));
            }
        }

        /// <summary>
        /// Saves content
        /// </summary>
        /// <returns></returns>
        [FileUploadCleanupFilter]
        [ContentSaveValidation]
        public ContentItemDisplay PostSaveBlueprint([ModelBinder(typeof(BlueprintItemBinder))] ContentItemSave contentItem)
        {
            var contentItemDisplay = PostSaveInternal(contentItem,
                content =>
                {
                    EnsureUniqueName(content.Name, content, "Name");

                    Services.ContentService.SaveBlueprint(contentItem.PersistedContent, Security.CurrentUser.Id);
                    //we need to reuse the underlying logic so return the result that it wants
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
        /// <returns></returns>
        [FileUploadCleanupFilter]
        [ContentSaveValidation]
        [OutgoingEditorModelEvent]
        public ContentItemDisplay PostSave([ModelBinder(typeof(ContentItemBinder))] ContentItemSave contentItem)
        {
            var contentItemDisplay = PostSaveInternal(
                contentItem,
                content => Services.ContentService.Save(contentItem.PersistedContent, Security.CurrentUser.Id),
                MapToDisplay);

            return contentItemDisplay;
        }

        private ContentItemDisplay PostSaveInternal(ContentItemSave contentItem, Func<IContent, OperationResult> saveMethod, Func<IContent, ContentItemDisplay> mapToDisplay)
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
            MapValuesForPersistence(contentItem);

            var passesCriticalValidationRules = ValidateCriticalData(contentItem, out var variantCount);

            //we will continue to save if model state is invalid, however we cannot save if critical data is missing.
            if (!ModelState.IsValid)
            {
                //check for critical data validation issues, we can't continue saving if this data is invalid
                if (!passesCriticalValidationRules)
                {
                    //ok, so the absolute mandatory data is invalid and it's new, we cannot actually continue!
                    // add the model state to the outgoing object and throw a validation message
                    var forDisplay = mapToDisplay(contentItem.PersistedContent);
                    forDisplay.Errors = ModelState.ToErrorDictionary();
                    throw new HttpResponseException(Request.CreateValidationErrorResponse(forDisplay));
                }

                //if there's only one variant and the model state is not valid we cannot publish so change it to save
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
                    var sendResult = Services.ContentService.SendToPublication(contentItem.PersistedContent, Security.CurrentUser.Id);
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
                                    Services.TextService.Localize("speechBubbles", "editContentSendToPublish"),
                                    Services.TextService.Localize("speechBubbles", "editVariantSendToPublishText", new[] { variantName }));
                            }
                        }
                        else if (ModelState.IsValid)
                        {
                            globalNotifications.AddSuccessNotification(
                                Services.TextService.Localize("speechBubbles", "editContentSendToPublish"),
                                Services.TextService.Localize("speechBubbles", "editContentSendToPublishText"));
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
                        if (!ValidatePublishBranchPermissions(contentItem, out var noAccess))
                        {
                            globalNotifications.AddErrorNotification(
                                Services.TextService.Localize(null,"publish"),
                                Services.TextService.Localize("publish", "invalidPublishBranchPermissions"));
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
                        if (!ValidatePublishBranchPermissions(contentItem, out var noAccess))
                        {
                            globalNotifications.AddErrorNotification(
                                Services.TextService.Localize(null,"publish"),
                                Services.TextService.Localize("publish", "invalidPublishBranchPermissions"));
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

            //lastly, if it is not valid, add the model state to the outgoing object and throw a 400
            HandleInvalidModelState(display, cultureForInvariantErrors);

            if (wasCancelled)
            {
                AddCancelMessage(display);
                if (IsCreatingAction(contentItem.Action))
                {
                    //If the item is new and the operation was cancelled, we need to return a different
                    // status code so the UI can handle it since it won't be able to redirect since there
                    // is no Id to redirect to!
                    throw new HttpResponseException(Request.CreateValidationErrorResponse(display));
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
                            AddVariantValidationError(variant.Culture, variant.Segment, "publish","contentPublishedFailedByMissingName");
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
        /// <param name="variantSavedLocalizationAlias"></param>
        /// <param name="wasCancelled"></param>
        /// <remarks>
        /// Method is used for normal Saving and Scheduled Publishing
        /// </remarks>
        private void SaveAndNotify(ContentItemSave contentItem, Func<IContent, OperationResult> saveMethod, int variantCount,
            Dictionary<string, SimpleNotificationModel> notifications, SimpleNotificationModel globalNotifications,
            string invariantSavedLocalizationKey, string variantSavedLocalizationAlias, string cultureForInvariantErrors,
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
                            Services.TextService.Localize("speechBubbles", "editContentSavedHeader"),
                            Services.TextService.Localize(null,variantSavedLocalizationAlias, new[] { variantName }));
                    }
                }
                else if (ModelState.IsValid)
                {
                    globalNotifications.AddSuccessNotification(
                        Services.TextService.Localize("speechBubbles", "editContentSavedHeader"),
                        Services.TextService.Localize(invariantSavedLocalizationKey));
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
                        Services.TextService.Localize("speechBubbles", "validationFailedHeader"),
                        Services.TextService.Localize("speechBubbles", "scheduleErrReleaseDate1"));
                return false;
            }

            //2) expire date cannot be less than now
            if (variant.ExpireDate.HasValue && variant.ExpireDate < DateTime.Now)
            {
                globalNotifications.AddErrorNotification(
                        Services.TextService.Localize("speechBubbles", "validationFailedHeader"),
                        Services.TextService.Localize("speechBubbles", "scheduleErrExpireDate1"));
                return false;
            }

            //3) expire date cannot be less than release date
            if (variant.ExpireDate.HasValue && variant.ReleaseDate.HasValue && variant.ExpireDate <= variant.ReleaseDate)
            {
                globalNotifications.AddErrorNotification(
                    Services.TextService.Localize("speechBubbles", "validationFailedHeader"),
                    Services.TextService.Localize("speechBubbles", "scheduleErrExpireDate2"));
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
                    AddVariantValidationError(culture, null, "speechBubbles", "scheduleErrReleaseDate2");
                    isValid = false;
                    continue;
                }
                if (!isPublished && releaseDates.Any(x => nonMandatoryVariantReleaseDates.Any(r => x.Date > r.Date)))
                {
                    //can't continue, a mandatory variant is not published and it's scheduled for publishing after a non-mandatory
                    // TODO: Add segment
                    AddVariantValidationError(culture, null, "speechBubbles", "scheduleErrReleaseDate3");
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
                    AddVariantValidationError(variant.Culture, variant.Segment, "speechBubbles", "scheduleErrReleaseDate1");
                    isValid = false;
                    continue;
                }

                //2) expire date cannot be less than now
                if (variant.ExpireDate.HasValue && variant.ExpireDate < DateTime.Now)
                {
                    AddVariantValidationError(variant.Culture, variant.Segment, "speechBubbles", "scheduleErrExpireDate1");
                    isValid = false;
                    continue;
                }

                //3) expire date cannot be less than release date
                if (variant.ExpireDate.HasValue && variant.ReleaseDate.HasValue && variant.ExpireDate <= variant.ReleaseDate)
                {
                    AddVariantValidationError(variant.Culture, variant.Segment, "speechBubbles", "scheduleErrExpireDate2");
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
        private bool ValidatePublishBranchPermissions(ContentItemSave contentItem, out IReadOnlyList<IUmbracoEntity> noAccess)
        {
            var denied = new List<IUmbracoEntity>();
            var page = 0;
            const int pageSize = 500;
            var total = long.MaxValue;
            while (page * pageSize < total)
            {
                var descendants = Services.EntityService.GetPagedDescendants(contentItem.Id, UmbracoObjectTypes.Document, page++, pageSize, out total,
                                //order by shallowest to deepest, this allows us to check permissions from top to bottom so we can exit
                                //early if a permission higher up fails
                                ordering: Ordering.By("path", Direction.Ascending));

                foreach (var c in descendants)
                {
                    //if this item's path has already been denied or if the user doesn't have access to it, add to the deny list
                    if (denied.Any(x => c.Path.StartsWith($"{x.Path},"))
                        || (ContentPermissionsHelper.CheckPermissions(c,
                            Security.CurrentUser, Services.UserService, Services.EntityService, AppCaches,
                            ActionPublish.ActionLetter) == ContentPermissionsHelper.ContentAccess.Denied))
                    {
                        denied.Add(c);
                    }
                }
            }
            noAccess = denied;
            return denied.Count == 0;
        }

        private IEnumerable<PublishResult> PublishBranchInternal(ContentItemSave contentItem, bool force, string cultureForInvariantErrors,
                out bool wasCancelled, out string[] successfulCultures)
        {
            if (!contentItem.PersistedContent.ContentType.VariesByCulture())
            {
                //its invariant, proceed normally
                var publishStatus = Services.ContentService.SaveAndPublishBranch(contentItem.PersistedContent, force, userId: Security.CurrentUser.Id);
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
                var publishStatus = Services.ContentService.SaveAndPublishBranch(contentItem.PersistedContent, force, culturesToPublish, Security.CurrentUser.Id);
                // TODO: Deal with multiple cancellations
                wasCancelled = publishStatus.Any(x => x.Result == PublishResultType.FailedPublishCancelledByEvent);
                successfulCultures = contentItem.Variants.Where(x => x.Publish).Select(x => x.Culture).ToArray();
                return publishStatus;
            }
            else
            {
                //can only save
                var saveResult = Services.ContentService.Save(contentItem.PersistedContent, Security.CurrentUser.Id);
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
                var publishStatus = Services.ContentService.SaveAndPublish(contentItem.PersistedContent, userId: Security.CurrentUser.Id);
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
                var publishStatus = Services.ContentService.SaveAndPublish(contentItem.PersistedContent, culturesToPublish, Security.CurrentUser.Id);
                wasCancelled = publishStatus.Result == PublishResultType.FailedPublishCancelledByEvent;
                successfulCultures = culturesToPublish;
                return publishStatus;
            }
            else
            {
                //can only save
                var saveResult = Services.ContentService.Save(contentItem.PersistedContent, Security.CurrentUser.Id);
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
                    AddVariantValidationError(r.model.Culture, r.model.Segment, "publish", "contentPublishedFailedReqCultureValidationError");
                    canPublish = false;
                }
                else if (r.publishing && r.isValid && firstInvalidMandatoryCulture != null)
                {
                    //in this case this culture also cannot be published because another mandatory culture is invalid
                    AddVariantValidationError(r.model.Culture, r.model.Segment, "publish", "contentPublishedFailedReqCultureValidationError", firstInvalidMandatoryCulture);
                    canPublish = false;
                }
                else if (!r.publishing)
                {
                    //cannot continue publishing since a required culture that is not currently being published isn't published
                    AddVariantValidationError(r.model.Culture, r.model.Segment, "speechBubbles", "contentReqCulturePublishError");
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
                    AddVariantValidationError(variant.Culture, variant.Segment, "speechBubbles", "contentCultureValidationError");
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
        private void AddVariantValidationError(string culture, string segment, string localizationArea,string localizationAlias, string cultureToken = null)
        {
            var cultureToUse = cultureToken ?? culture;
            var variantName = GetVariantName(cultureToUse, segment);

            var errMsg = Services.TextService.Localize(localizationArea, localizationAlias, new[] { variantName });

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
        ///
        [EnsureUserPermissionForContent("id", 'U')]
        public HttpResponseMessage PostPublishById(int id)
        {
            var foundContent = GetObjectFromRequest(() => Services.ContentService.GetById(id));

            if (foundContent == null)
            {
                return HandleContentNotFound(id, false);
            }

            var publishResult = Services.ContentService.SaveAndPublish(foundContent, userId: Security.GetUserId().ResultOr(0));
            if (publishResult.Success == false)
            {
                var notificationModel = new SimpleNotificationModel();
                AddMessageForPublishStatus(new[] { publishResult }, notificationModel);
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
        [EnsureUserPermissionForContent("id", ActionDelete.ActionLetter)]
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
        [EnsureUserPermissionForContent(Constants.System.RecycleBinContent, ActionDelete.ActionLetter)]
        public HttpResponseMessage EmptyRecycleBin()
        {
            Services.ContentService.EmptyRecycleBin(Security.GetUserId().ResultOr(Constants.Security.SuperUserId));

            return Request.CreateNotificationSuccessResponse(Services.TextService.Localize("defaultdialogs", "recycleBinIsEmpty"));
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
                var sortResult = contentService.Sort(sorted.IdSortOrder, Security.CurrentUser.Id);
                if (!sortResult.Success)
                {
                    Logger.Warn<ContentController>("Content sorting failed, this was probably caused by an event being cancelled");
                    // TODO: Now you can cancel sorting, does the event messages bubble up automatically?
                    return Request.CreateValidationErrorResponse("Content sorting failed, this was probably caused by an event being cancelled");
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Logger.Error<ContentController>(ex, "Could not update content sort order");
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
            response.Content = new StringContent(toMove.Path, Encoding.UTF8, "text/plain");
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
            response.Content = new StringContent(c.Path, Encoding.UTF8, "text/plain");
            return response;
        }

        /// <summary>
        /// Unpublishes a node with a given Id and returns the unpublished entity
        /// </summary>
        /// <param name="model">The content and variants to unpublish</param>
        /// <returns></returns>
        [EnsureUserPermissionForContent("model.Id", 'Z')]
        [OutgoingEditorModelEvent]
        public ContentItemDisplay PostUnpublish(UnpublishContent model)
        {
            var foundContent = GetObjectFromRequest(() => Services.ContentService.GetById(model.Id));

            if (foundContent == null)
                HandleContentNotFound(model.Id);

            var languageCount = _allLangs.Value.Count();
            if (model.Cultures.Length == 0 || model.Cultures.Length == languageCount)
            {
                //this means that the entire content item will be unpublished
                var unpublishResult = Services.ContentService.Unpublish(foundContent, userId: Security.GetUserId().ResultOr(0));

                var content = MapToDisplay(foundContent);

                if (!unpublishResult.Success)
                {
                    AddCancelMessage(content);
                    throw new HttpResponseException(Request.CreateValidationErrorResponse(content));
                }
                else
                {
                    content.AddSuccessNotification(
                        Services.TextService.Localize("content", "unpublish"),
                        Services.TextService.Localize("speechBubbles", "contentUnpublished"));
                    return content;
                }
            }
            else
            {
                //we only want to unpublish some of the variants
                var results = new Dictionary<string, PublishResult>();
                foreach (var c in model.Cultures)
                {
                    var result = Services.ContentService.Unpublish(foundContent, culture: c, userId: Security.GetUserId().ResultOr(0));
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
                           Services.TextService.Localize("content", "unpublish"),
                           Services.TextService.Localize("speechBubbles", "contentMandatoryCultureUnpublished"));
                    return content;
                }

                //otherwise add a message for each one unpublished
                foreach (var r in results)
                {
                    content.AddSuccessNotification(
                           Services.TextService.Localize("conten", "unpublish"),
                           Services.TextService.Localize("speechBubbles", "contentCultureUnpublished", new[] { _allLangs.Value[r.Key].CultureName }));
                }
                return content;

            }

        }

        public ContentDomainsAndCulture GetCultureAndDomains(int id)
        {
            var nodeDomains = Services.DomainService.GetAssignedDomains(id, true).ToArray();
            var wildcard = nodeDomains.FirstOrDefault(d => d.IsWildcard);
            var domains = nodeDomains.Where(d => !d.IsWildcard).Select(d => new DomainDisplay(d.DomainName, d.LanguageId.GetValueOrDefault(0)));
            return new ContentDomainsAndCulture
            {
                Domains = domains,
                Language = wildcard == null || !wildcard.LanguageId.HasValue ? "undefined" : wildcard.LanguageId.ToString()
            };
        }

        [HttpPost]
        public DomainSave PostSaveLanguageAndDomains(DomainSave model)
        {
            foreach (var domain in model.Domains)
            {
                try
                {
                    var uri = DomainUtilities.ParseUriFromDomainName(domain.Name, Request.RequestUri);
                }
                catch (UriFormatException)
                {
                    var response = Request.CreateValidationErrorResponse(Services.TextService.Localize("assignDomain", "invalidDomain"));
                    throw new HttpResponseException(response);
                }
            }

            var node = Services.ContentService.GetById(model.NodeId);

            if (node == null)
            {
                var response = Request.CreateResponse(HttpStatusCode.BadRequest);
                response.Content = new StringContent($"There is no content node with id {model.NodeId}.");
                response.ReasonPhrase = "Node Not Found.";
                throw new HttpResponseException(response);
            }

            var assignedPermissions = Services.UserService.GetAssignedPermissions(Security.CurrentUser, node.Id);

            if (assignedPermissions.Contains(ActionAssignDomain.ActionLetter.ToString(), StringComparer.Ordinal) == false)
            {
                var response = Request.CreateResponse(HttpStatusCode.BadRequest);
                response.Content = new StringContent("You do not have permission to assign domains on that node.");
                response.ReasonPhrase = "Permission Denied.";
                throw new HttpResponseException(response);
            }

            model.Valid = true;
            var domains = Services.DomainService.GetAssignedDomains(model.NodeId, true).ToArray();
            var languages = Services.LocalizationService.GetAllLanguages().ToArray();
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

                var saveAttempt = Services.DomainService.Save(wildcard);
                if (saveAttempt == false)
                {
                    var response = Request.CreateResponse(HttpStatusCode.BadRequest);
                    response.Content = new StringContent("Saving domain failed");
                    response.ReasonPhrase = saveAttempt.Result.Result.ToString();
                    throw new HttpResponseException(response);
                }
            }
            else
            {
                var wildcard = domains.FirstOrDefault(d => d.IsWildcard);
                if (wildcard != null)
                {
                    Services.DomainService.Delete(wildcard);
                }
            }

            // process domains
            // delete every (non-wildcard) domain, that exists in the DB yet is not in the model
            foreach (var domain in domains.Where(d => d.IsWildcard == false && model.Domains.All(m => m.Name.InvariantEquals(d.DomainName) == false)))
            {
                Services.DomainService.Delete(domain);
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
                    Services.DomainService.Save(domain);
                }
                else if (Services.DomainService.Exists(domainModel.Name))
                {
                    domainModel.Duplicate = true;
                    var xdomain = Services.DomainService.GetByName(domainModel.Name);
                    var xrcid = xdomain.RootContentId;
                    if (xrcid.HasValue)
                    {
                        var xcontent = Services.ContentService.GetById(xrcid.Value);
                        var xnames = new List<string>();
                        while (xcontent != null)
                        {
                            xnames.Add(xcontent.Name);
                            if (xcontent.ParentId < -1)
                                xnames.Add("Recycle Bin");
                            xcontent = Services.ContentService.GetParent(xcontent);
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
                    var saveAttempt = Services.DomainService.Save(newDomain);
                    if (saveAttempt == false)
                    {
                        var response = Request.CreateResponse(HttpStatusCode.BadRequest);
                        response.Content = new StringContent("Saving new domain failed");
                        response.ReasonPhrase = saveAttempt.Result.Result.ToString();
                        throw new HttpResponseException(response);
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
                    AddVariantValidationError(culture, segment, "speechBubbles", "contentCultureValidationError");
                }
            }

            base.HandleInvalidModelState(display);

        }

        /// <summary>
        /// Maps the dto property values and names to the persisted model
        /// </summary>
        /// <param name="contentSave"></param>
        private void MapValuesForPersistence(ContentItemSave contentSave)
        {
            // inline method to determine the culture and segment to persist the property
            (string culture, string segment) PropertyCultureAndSegment(Property property, ContentVariantSave variant)
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
                var template = Services.FileService.GetTemplate(contentSave.TemplateAlias);
                if (template == null)
                {
                    //ModelState.AddModelError("Template", "No template exists with the specified alias: " + contentItem.TemplateAlias);
                    Logger.Warn<ContentController, string>("No template exists with the specified alias: {TemplateAlias}", contentSave.TemplateAlias);
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
                                    Services.TextService.Localize("moveOrCopy", "notAllowedAtRoot")));
                }
            }
            else
            {
                var parent = contentService.GetById(model.ParentId);
                if (parent == null)
                {
                    throw new HttpResponseException(HttpStatusCode.NotFound);
                }

                var parentContentType = Services.ContentTypeService.Get(parent.ContentTypeId);
                //check if the item is allowed under this one
                if (parentContentType.AllowedContentTypes.Select(x => x.Id).ToArray()
                        .Any(x => x.Value == toMove.ContentType.Id) == false)
                {
                    throw new HttpResponseException(
                            Request.CreateNotificationValidationErrorResponse(
                                    Services.TextService.Localize("moveOrCopy", "notAllowedByContentType")));
                }

                // Check on paths
                if ((string.Format(",{0},", parent.Path)).IndexOf(string.Format(",{0},", toMove.Id), StringComparison.Ordinal) > -1)
                {
                    throw new HttpResponseException(
                            Request.CreateNotificationValidationErrorResponse(
                                    Services.TextService.Localize("moveOrCopy", "notAllowedByPath")));
                }
            }

            return toMove;
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
                                        Services.TextService.Localize("speechBubbles", "editContentPublishedHeader"),
                                        Services.TextService.Localize("speechBubbles", "editContentPublishedText"));
                                }
                                else
                                {
                                    foreach (var c in successfulCultures)
                                    {
                                        display.AddSuccessNotification(
                                            Services.TextService.Localize("speechBubbles", "editContentPublishedHeader"),
                                            Services.TextService.Localize("speechBubbles", "editVariantPublishedText", new[] { _allLangs.Value[c].CultureName }));
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
                                    Services.TextService.Localize("speechBubbles", "editContentPublishedHeader"),
                                    totalStatusCount > 1
                                        ? Services.TextService.Localize("speechBubbles", "editMultiContentPublishedText", new[] { itemCount.ToInvariantString() })
                                        : Services.TextService.Localize("speechBubbles", "editContentPublishedText"));
                            }
                            else
                            {
                                foreach (var c in successfulCultures)
                                {
                                    display.AddSuccessNotification(
                                        Services.TextService.Localize("speechBubbles", "editContentPublishedHeader"),
                                        totalStatusCount > 1
                                            ? Services.TextService.Localize("speechBubbles", "editMultiVariantPublishedText", new[] { itemCount.ToInvariantString(), _allLangs.Value[c].CultureName })
                                            : Services.TextService.Localize("speechBubbles", "editVariantPublishedText", new[] { _allLangs.Value[c].CultureName }));
                                }
                            }
                        }
                        break;
                    case PublishResultType.FailedPublishPathNotPublished:
                        {
                            //TODO: This doesn't take into account variations with the successfulCultures param
                            var names = string.Join(", ", status.Select(x => $"'{x.Content.Name}'"));
                            display.AddWarningNotification(
                                Services.TextService.Localize(null,"publish"),
                                Services.TextService.Localize("publish", "contentPublishedFailedByParent",
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
                                    Services.TextService.Localize(null,"publish"),
                                    Services.TextService.Localize("publish", "contentPublishedFailedAwaitingRelease",
                                        new[] { names }).Trim());
                        }
                        break;
                    case PublishResultType.FailedPublishHasExpired:
                        {
                            //TODO: This doesn't take into account variations with the successfulCultures param
                            var names = string.Join(", ", status.Select(x => $"'{x.Content.Name}'"));
                            display.AddWarningNotification(
                                Services.TextService.Localize(null,"publish"),
                                Services.TextService.Localize("publish", "contentPublishedFailedExpired",
                                    new[] { names }).Trim());
                        }
                        break;
                    case PublishResultType.FailedPublishIsTrashed:
                        {
                            //TODO: This doesn't take into account variations with the successfulCultures param
                            var names = string.Join(", ", status.Select(x => $"'{x.Content.Name}'"));
                            display.AddWarningNotification(
                                Services.TextService.Localize(null,"publish"),
                                Services.TextService.Localize("publish", "contentPublishedFailedIsTrashed",
                                    new[] { names }).Trim());
                        }
                        break;
                    case PublishResultType.FailedPublishContentInvalid:
                        {
                            if (successfulCultures == null)
                            {
                                var names = string.Join(", ", status.Select(x => $"'{x.Content.Name}'"));
                                display.AddWarningNotification(
                                    Services.TextService.Localize(null,"publish"),
                                    Services.TextService.Localize("publish", "contentPublishedFailedInvalid",
                                        new[] { names }).Trim());
                            }
                            else
                            {
                                foreach (var c in successfulCultures)
                                {
                                    var names = string.Join(", ", status.Select(x => $"'{(x.Content.ContentType.VariesByCulture() ? x.Content.GetCultureName(c) : x.Content.Name)}'"));
                                    display.AddWarningNotification(
                                        Services.TextService.Localize(null,"publish"),
                                        Services.TextService.Localize("publish", "contentPublishedFailedInvalid",
                                            new[] { names }).Trim());
                                }
                            }
                        }
                        break;
                    case PublishResultType.FailedPublishMandatoryCultureMissing:
                        display.AddWarningNotification(
                            Services.TextService.Localize(null,"publish"),
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
        private ContentItemDisplay MapToDisplay(IContent content) =>
            MapToDisplay(content, context =>
            {
                context.Items["CurrentUser"] = Security.CurrentUser;
            });

        /// <summary>
        /// Used to map an <see cref="IContent"/> instance to a <see cref="ContentItemDisplay"/> and ensuring AllowPreview is set correctly.
        /// Also allows you to pass in an action for the mapper context where you can pass additional information on to the mapper.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="contextOptions"></param>
        /// <returns></returns>
        private ContentItemDisplay MapToDisplay(IContent content, Action<MapperContext> contextOptions)
        {
            var display = Mapper.Map<ContentItemDisplay>(content, contextOptions);
            display.AllowPreview = display.AllowPreview && content.Trashed == false && content.ContentType.IsElement == false;
            return display;
        }

        [EnsureUserPermissionForContent("contentId", ActionBrowse.ActionLetter)]
        public IEnumerable<NotifySetting> GetNotificationOptions(int contentId)
        {
            var notifications = new List<NotifySetting>();
            if (contentId <= 0) throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));

            var content = Services.ContentService.GetById(contentId);
            if (content == null) throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));

            var userNotifications = Services.NotificationService.GetUserNotifications(Security.CurrentUser, content.Path).ToList();

            foreach (var a in Current.Actions.Where(x => x.ShowInNotifier))
            {
                var n = new NotifySetting
                {
                    Name = Services.TextService.Localize("actions", a.Alias),
                    Checked = userNotifications.FirstOrDefault(x => x.Action == a.Letter.ToString()) != null,
                    NotifyCode = a.Letter.ToString()
                };
                notifications.Add(n);
            }

            return notifications;
        }

        public void PostNotificationOptions(int contentId, [FromUri] string[] notifyOptions)
        {
            if (contentId <= 0) throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            var content = Services.ContentService.GetById(contentId);
            if (content == null) throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));

            Services.NotificationService.SetNotifications(Security.CurrentUser, content, notifyOptions);
        }

        [HttpGet]
        public PagedResult<ContentVersionMetaViewModel> GetPagedContentVersions(
            int contentId,
            int pageNumber = 1,
            int pageSize = 10,
            string culture = null)
        {
            if (!string.IsNullOrEmpty(culture))
            {
                if (!_allLangs.Value.TryGetValue(culture, out _))
                {
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
                }
            }

            // NOTE: v9 - don't service locate
            var contentVersionService = Current.Factory.GetInstance<IContentVersionService>();

            var results =  contentVersionService.GetPagedContentVersions(contentId, pageNumber - 1, pageSize, out var totalRecords, culture);

            return new PagedResult<ContentVersionMetaViewModel>(totalRecords, pageNumber, pageSize)
            {
                Items = results.Select(x => new ContentVersionMetaViewModel(x))
            };
        }
        
        [HttpPost]
        [EnsureUserPermissionForContent("contentId", ActionUpdate.ActionLetter)]
        public HttpResponseMessage PostSetContentVersionPreventCleanup(int contentId, int versionId, bool preventCleanup)
        {
            var content = Services.ContentService.GetVersion(versionId);
            if (content == null || content.Id != contentId)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            // NOTE: v9 - don't service locate
            var contentVersionService = Current.Factory.GetInstance<IContentVersionService>();

            contentVersionService.SetPreventCleanup(versionId, preventCleanup, Security.GetUserId().ResultOr(0));

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpGet]
        public IEnumerable<RollbackVersion> GetRollbackVersions(int contentId, string culture = null)
        {
            var rollbackVersions = new List<RollbackVersion>();
            var writerIds = new HashSet<int>();

            var versions = Services.ContentService.GetVersionsSlim(contentId, 0, 50);

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

            var users = Services.UserService
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
            var version = Services.ContentService.GetVersion(versionId);
            var content = MapToDisplay(version);

            return culture == null
                ? content.Variants.FirstOrDefault()  //No culture set - so this is an invariant node - so just list me the first item in here
                : content.Variants.FirstOrDefault(x => x.Language.IsoCode == culture);
        }

        [EnsureUserPermissionForContent("contentId", ActionRollback.ActionLetter)]
        [HttpPost]
        public HttpResponseMessage PostRollbackContent(int contentId, int versionId, string culture = "*")
        {
            var rollbackResult = Services.ContentService.Rollback(contentId, versionId, culture, Security.GetUserId().ResultOr(0));

            if (rollbackResult.Success)
                return Request.CreateResponse(HttpStatusCode.OK);

            var notificationModel = new SimpleNotificationModel();

            switch (rollbackResult.Result)
            {
                case OperationResultType.Failed:
                case OperationResultType.FailedCannot:
                case OperationResultType.FailedExceptionThrown:
                case OperationResultType.NoOperation:
                default:
                    notificationModel.AddErrorNotification(
                                    Services.TextService.Localize("speechBubbles", "operationFailedHeader"),
                                    null); // TODO: There is no specific failed to save error message AFAIK
                    break;
                case OperationResultType.FailedCancelledByEvent:
                    notificationModel.AddErrorNotification(
                                    Services.TextService.Localize("speechBubbles", "operationCancelledHeader"),
                                    Services.TextService.Localize("speechBubbles", "operationCancelledText"));
                    break;
            }

            return Request.CreateValidationErrorResponse(notificationModel);
        }


        [EnsureUserPermissionForContent("contentId", ActionProtect.ActionLetter)]
        [HttpGet]
        public HttpResponseMessage GetPublicAccess(int contentId)
        {
            var content = Services.ContentService.GetById(contentId);
            if (content == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            var entry = Services.PublicAccessService.GetEntryForContent(content);
            if (entry == null || entry.ProtectedNodeId != content.Id)
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }

            var loginPageEntity = Services.EntityService.Get(entry.LoginNodeId, UmbracoObjectTypes.Document);
            var errorPageEntity = Services.EntityService.Get(entry.NoAccessNodeId, UmbracoObjectTypes.Document);

            // unwrap the current public access setup for the client
            // - this API method is the single point of entry for both "modes" of public access (single user and role based)
            var usernames = entry.Rules
                .Where(rule => rule.RuleType == Constants.Conventions.PublicAccess.MemberUsernameRuleType)
                .Select(rule => rule.RuleValue).ToArray();

            MemberDisplay[] members;
            switch (Services.MemberService.GetMembershipScenario())
            {
                case MembershipScenario.NativeUmbraco:
                    members = usernames
                        .Select(username => Services.MemberService.GetByUsername(username))
                        .Where(member => member != null)
                        .Select(Mapper.Map<MemberDisplay>)
                        .ToArray();
                    break;
                // TODO: test support custom membership providers
                case MembershipScenario.CustomProviderWithUmbracoLink:
                case MembershipScenario.StandaloneCustomProvider:
                default:
                    var provider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();
                    members = usernames
                        .Select(username => provider.GetUser(username, false))
                        .Where(membershipUser => membershipUser != null)
                        .Select(Mapper.Map<MembershipUser, MemberDisplay>)
                        .ToArray();
                    break;
            }

            var allGroups = Services.MemberGroupService.GetAll().ToArray();
            var groups = entry.Rules
                .Where(rule => rule.RuleType == Constants.Conventions.PublicAccess.MemberRoleRuleType)
                .Select(rule => allGroups.FirstOrDefault(g => g.Name == rule.RuleValue))
                .Where(memberGroup => memberGroup != null)
                .Select(Mapper.Map<MemberGroupDisplay>)
                .ToArray();

            return Request.CreateResponse(HttpStatusCode.OK, new PublicAccess
            {
                Members = members,
                Groups = groups,
                LoginPage = loginPageEntity != null ? Mapper.Map<EntityBasic>(loginPageEntity) : null,
                ErrorPage = errorPageEntity != null ? Mapper.Map<EntityBasic>(errorPageEntity) : null
            });
        }

        // set up public access using role based access
        [EnsureUserPermissionForContent("contentId", ActionProtect.ActionLetter)]
        [HttpPost]
        public HttpResponseMessage PostPublicAccess(int contentId, [FromUri] string[] groups, [FromUri] string[] usernames, int loginPageId, int errorPageId)
        {
            if ((groups == null || groups.Any() == false) && (usernames == null || usernames.Any() == false))
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest));
            }

            var content = Services.ContentService.GetById(contentId);
            var loginPage = Services.ContentService.GetById(loginPageId);
            var errorPage = Services.ContentService.GetById(errorPageId);
            if (content == null || loginPage == null || errorPage == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.BadRequest));
            }

            var isGroupBased = groups != null && groups.Any();
            var candidateRuleValues = isGroupBased
                ? groups
                : usernames;
            var newRuleType = isGroupBased
                ? Constants.Conventions.PublicAccess.MemberRoleRuleType
                : Constants.Conventions.PublicAccess.MemberUsernameRuleType;

            var entry = Services.PublicAccessService.GetEntryForContent(content);

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

            return Services.PublicAccessService.Save(entry).Success
                ? Request.CreateResponse(HttpStatusCode.OK)
                : Request.CreateResponse(HttpStatusCode.InternalServerError);
        }

        [EnsureUserPermissionForContent("contentId", ActionProtect.ActionLetter)]
        [HttpPost]
        public HttpResponseMessage RemovePublicAccess(int contentId)
        {
            var content = Services.ContentService.GetById(contentId);
            if (content == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            var entry = Services.PublicAccessService.GetEntryForContent(content);
            if (entry == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }

            return Services.PublicAccessService.Delete(entry).Success
                ? Request.CreateResponse(HttpStatusCode.OK)
                : Request.CreateResponse(HttpStatusCode.InternalServerError);
        }
    }
}
