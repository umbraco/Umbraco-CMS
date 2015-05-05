using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Umbraco.Web.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;
using Umbraco.Web.Mvc;
using Umbraco.Web.Security;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Binders;
using Umbraco.Web.WebApi.Filters;
using umbraco;
using Umbraco.Core.Models;
using Umbraco.Core.Dynamics;
using umbraco.BusinessLogic.Actions;
using umbraco.cms.businesslogic.web;
using umbraco.presentation.preview;
using Constants = Umbraco.Core.Constants;

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
    [UmbracoApplicationAuthorizeAttribute(Constants.Applications.Content)]
    public class ContentController : ContentControllerBase
    {        
        /// <summary>
        /// Constructor
        /// </summary>
        public ContentController()
            : this(UmbracoContext.Current)
        {            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        public ContentController(UmbracoContext umbracoContext)
            : base(umbracoContext)
        {            
        }

        /// <summary>
        /// Return content for the specified ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [FilterAllowedOutgoingContent(typeof(IEnumerable<ContentItemDisplay>))]
        public IEnumerable<ContentItemDisplay> GetByIds([FromUri]int[] ids)
        {
            var foundContent = Services.ContentService.GetByIds(ids);
            return foundContent.Select(Mapper.Map<IContent, ContentItemDisplay>);
        }

        /// <summary>
        /// Gets the content json for the content id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [EnsureUserPermissionForContent("id")]
        public ContentItemDisplay GetById(int id)
        {
            var foundContent = GetObjectFromRequest(() => Services.ContentService.GetById(id));            
            if (foundContent == null)
            {
                HandleContentNotFound(id);
            }
            
            var content = Mapper.Map<IContent, ContentItemDisplay>(foundContent);
            return content;
       }

        [EnsureUserPermissionForContent("id")]
        public ContentItemDisplay GetWithTreeDefinition(int id)
        {
            var foundContent = GetObjectFromRequest(() => Services.ContentService.GetById(id));
            if (foundContent == null)
            {
                HandleContentNotFound(id);
            }

            var content = Mapper.Map<IContent, ContentItemDisplay>(foundContent);
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
        public ContentItemDisplay GetEmpty(string contentTypeAlias, int parentId)
        {
            var contentType = Services.ContentTypeService.GetContentType(contentTypeAlias);
            if (contentType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var emptyContent = Services.ContentService.CreateContent("", parentId, contentType.Alias, UmbracoUser.Id);
            var mapped = Mapper.Map<IContent, ContentItemDisplay>(emptyContent);

            //remove this tab if it exists: umbContainerView
            var containerTab = mapped.Tabs.FirstOrDefault(x => x.Alias == Constants.Conventions.PropertyGroups.ListViewGroupName);
            mapped.Tabs = mapped.Tabs.Except(new[] {containerTab});
            return mapped;
        }

        /// <summary>
        /// Gets the Url for a given node ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public HttpResponseMessage GetNiceUrl(int id)
        {
            var url = Umbraco.NiceUrl(id);
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(url, Encoding.UTF8, "application/json");        
            return response;
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
            string filter = "")
        {
            long totalChildren;
            IContent[] children;
            if (pageNumber > 0 && pageSize > 0)
            {
                children = Services.ContentService.GetPagedChildren(id, (pageNumber - 1), pageSize, out totalChildren, orderBy, orderDirection, filter).ToArray();
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
            pagedResult.Items = children
                .Select(Mapper.Map<IContent, ContentItemBasic<ContentPropertyBasic, IContent>>);

            return pagedResult;
        }

        [HttpGet]
        public bool GetHasPermission(string permissionToCheck, int nodeId)
        {
            var p = Services.UserService.GetPermissions(Security.CurrentUser, nodeId).FirstOrDefault();
            if (p != null && p.AssignedPermissions.Contains(permissionToCheck.ToString(CultureInfo.InvariantCulture)))
            {
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Saves content
        /// </summary>
        /// <returns></returns>
        [FileUploadCleanupFilter]
        [ContentPostValidate]
        public ContentItemDisplay PostSave(
            [ModelBinder(typeof(ContentItemBinder))]
                ContentItemSave contentItem)
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
                if (ValidationHelper.ModelHasRequiredForPersistenceErrors(contentItem) && IsCreatingAction(contentItem.Action))
                {
                    //ok, so the absolute mandatory data is invalid and it's new, we cannot actually continue!
                    // add the modelstate to the outgoing object and throw a validation message
                    var forDisplay = Mapper.Map<IContent, ContentItemDisplay>(contentItem.PersistedContent);
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
            var publishStatus = Attempt<PublishStatus>.Succeed();

            if (contentItem.Action == ContentSaveAction.Save || contentItem.Action == ContentSaveAction.SaveNew)
            {
                //save the item
                Services.ContentService.Save(contentItem.PersistedContent, Security.CurrentUser.Id);
            }
            else if (contentItem.Action == ContentSaveAction.SendPublish || contentItem.Action == ContentSaveAction.SendPublishNew)
            {
                Services.ContentService.SendToPublication(contentItem.PersistedContent, Security.CurrentUser.Id);
            }
            else
            {
                //publish the item and check if it worked, if not we will show a diff msg below
                publishStatus = Services.ContentService.SaveAndPublishWithStatus(contentItem.PersistedContent, Security.CurrentUser.Id);
            }
            

            //return the updated model
            var display = Mapper.Map<IContent, ContentItemDisplay>(contentItem.PersistedContent);

            //lasty, if it is not valid, add the modelstate to the outgoing object and throw a 403
            HandleInvalidModelState(display);

            //put the correct msgs in 
            switch (contentItem.Action)
            {
                case ContentSaveAction.Save:
                case ContentSaveAction.SaveNew:
                    display.AddSuccessNotification(ui.Text("speechBubbles", "editContentSavedHeader"), ui.Text("speechBubbles", "editContentSavedText"));
                    break;
                case ContentSaveAction.SendPublish:
                case ContentSaveAction.SendPublishNew:
                    display.AddSuccessNotification(ui.Text("speechBubbles", "editContentSendToPublish"), ui.Text("speechBubbles", "editContentSendToPublishText"));
                    break;
                case ContentSaveAction.Publish:
                case ContentSaveAction.PublishNew:
                    ShowMessageForPublishStatus(publishStatus.Result, display);
                    break;
            }

            UpdatePreviewContext(contentItem.PersistedContent.Id);

            return display;
        }

        /// <summary>
        /// Checks if the user is currently in preview mode and if so will update the preview content for this item
        /// </summary>
        /// <param name="contentId"></param>
        private void UpdatePreviewContext(int contentId)
        {
            var previewId = Request.GetPreviewCookieValue();
            if (previewId.IsNullOrWhiteSpace()) return;
            Guid id;
            if (Guid.TryParse(previewId, out id))
            {
                var d = new Document(contentId);
                var pc = new PreviewContent(UmbracoUser, id, false);
                pc.PrepareDocument(UmbracoUser, d, true);
                pc.SavePreviewSet();
            }          
        }

        /// <summary>
        /// Maps the dto property values to the persisted model
        /// </summary>
        /// <param name="contentItem"></param>
        private void MapPropertyValues(ContentItemSave contentItem)
        {
            UpdateName(contentItem);

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
                    LogHelper.Warn<ContentController>("No template exists with the specified alias: " + contentItem.TemplateAlias);
                }
                else
                {
                    //NOTE: this could be null if there was a template and the posted template is null, this should remove the assigned template
                    contentItem.PersistedContent.Template = template;
                }
            }

            base.MapPropertyValues(contentItem);
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

            var publishResult = Services.ContentService.PublishWithStatus(foundContent, UmbracoUser.Id);
            if (publishResult.Success == false)
            {
                switch (publishResult.Result.StatusType)
                {
                    case PublishStatusType.FailedPathNotPublished:
                        return Request.CreateValidationErrorResponse(
                            ui.Text("publish", "contentPublishedFailedByParent",
                                    string.Format("{0} ({1})", publishResult.Result.ContentItem.Name, publishResult.Result.ContentItem.Id),
                                    Security.CurrentUser).Trim());
                    case PublishStatusType.FailedCancelledByEvent:
                        return Request.CreateValidationErrorResponse(
                            ui.Text("speechBubbles", "contentPublishedFailedByEvent"));
                    case PublishStatusType.FailedHasExpired:
                    case PublishStatusType.FailedAwaitingRelease:
                    case PublishStatusType.FailedIsTrashed:
                    case PublishStatusType.FailedContentInvalid:
                        return Request.CreateValidationErrorResponse(
                           ui.Text("publish", "contentPublishedFailedInvalid",
                                  new[]
                                       {
                                           string.Format("{0} ({1})", publishResult.Result.ContentItem.Name, publishResult.Result.ContentItem.Id),
                                           string.Join(",", publishResult.Result.InvalidProperties.Select(x => x.Alias))
                                       }, Security.CurrentUser));
                }
            }

            //return ok
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
            if (foundContent.IsInRecycleBin() == false)
            {
                Services.ContentService.MoveToRecycleBin(foundContent, UmbracoUser.Id);                
            }
            else
            {
                Services.ContentService.Delete(foundContent, UmbracoUser.Id);
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
            return Request.CreateResponse(HttpStatusCode.OK);
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

            var contentService = Services.ContentService;
            var sortedContent = new List<IContent>();
            try
            {
                sortedContent.AddRange(Services.ContentService.GetByIds(sorted.IdSortOrder));

                // Save content with new sort order and update content xml in db accordingly
                if (contentService.Sort(sortedContent) == false)
                {
                    LogHelper.Warn<MediaController>("Content sorting failed, this was probably caused by an event being cancelled");
                    return Request.CreateValidationErrorResponse("Content sorting failed, this was probably caused by an event being cancelled");
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogHelper.Error<MediaController>("Could not update content sort order", ex);
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

            Services.ContentService.Move(toMove, move.ParentId);

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

            var c = Services.ContentService.Copy(toCopy, copy.ParentId, copy.RelateToOriginal);

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(c.Path, Encoding.UTF8, "application/json");
            return response;
        }

        /// <summary>
        /// Unpublishes a node with a given Id and returns the unpublished entity
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [EnsureUserPermissionForContent("id", 'U')]
        public ContentItemDisplay PostUnPublish(int id)
        {
            var foundContent = GetObjectFromRequest(() => Services.ContentService.GetById(id));

            if (foundContent == null)
                HandleContentNotFound(id);

            Services.ContentService.UnPublish(foundContent);
            var content = Mapper.Map<IContent, ContentItemDisplay>(foundContent);

            content.AddSuccessNotification(ui.Text("content", "unPublish"), ui.Text("speechBubbles", "contentUnpublished"));

            return content;
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
                        Request.CreateValidationErrorResponse(ui.Text("moveOrCopy", "notAllowedAtRoot", Security.CurrentUser)));
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
                        Request.CreateValidationErrorResponse(ui.Text("moveOrCopy", "notAllowedByContentType", Security.CurrentUser)));
                }

                // Check on paths
                if ((string.Format(",{0},", parent.Path)).IndexOf(string.Format(",{0},", toMove.Id), StringComparison.Ordinal) > -1)
                {
                    throw new HttpResponseException(
                        Request.CreateValidationErrorResponse(ui.Text("moveOrCopy", "notAllowedByPath", Security.CurrentUser)));
                }
            }

            return toMove;
        }

        private void ShowMessageForPublishStatus(PublishStatus status, ContentItemDisplay display)
        {
            switch (status.StatusType)
            {
                case PublishStatusType.Success:
                case PublishStatusType.SuccessAlreadyPublished:
                    display.AddSuccessNotification(
                        ui.Text("speechBubbles", "editContentPublishedHeader", UmbracoUser),
                        ui.Text("speechBubbles", "editContentPublishedText", UmbracoUser));
                    break;
                case PublishStatusType.FailedPathNotPublished:
                    display.AddWarningNotification(
                        ui.Text("publish"),
                        ui.Text("publish", "contentPublishedFailedByParent",
                                string.Format("{0} ({1})", status.ContentItem.Name, status.ContentItem.Id),
                                UmbracoUser).Trim());
                    break;
                case PublishStatusType.FailedCancelledByEvent:
                    display.AddWarningNotification(
                        ui.Text("publish"),
                        ui.Text("speechBubbles", "contentPublishedFailedByEvent"));
                    break;                
                case PublishStatusType.FailedAwaitingRelease:
                    display.AddWarningNotification(
                        ui.Text("publish"),
                        ui.Text("publish", "contentPublishedFailedAwaitingRelease",
                                new[]
                                    {
                                        string.Format("{0} ({1})", status.ContentItem.Name, status.ContentItem.Id)
                                    },
                                UmbracoUser).Trim());
                    break;
                case PublishStatusType.FailedHasExpired:
                    //TODO: We should add proper error messaging for this!
                case PublishStatusType.FailedIsTrashed:
                    //TODO: We should add proper error messaging for this!
                case PublishStatusType.FailedContentInvalid:
                    display.AddWarningNotification(
                        ui.Text("publish"),
                        ui.Text("publish", "contentPublishedFailedInvalid",
                                new[]
                                    {
                                        string.Format("{0} ({1})", status.ContentItem.Name, status.ContentItem.Id),
                                        string.Join(",", status.InvalidProperties.Select(x => x.Alias))
                                    },
                                UmbracoUser).Trim());
                    break;
                default:
                    throw new IndexOutOfRangeException();
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
        /// <param name="nodeId">The content to lookup, if the contentItem is not specified</param>
        /// <param name="permissionsToCheck"></param>
        /// <param name="contentItem">Specifies the already resolved content item to check against</param>
        /// <returns></returns>
        internal static bool CheckPermissions(
            IDictionary<string, object> storage,
            IUser user,
            IUserService userService,
            IContentService contentService,
            int nodeId,
            char[] permissionsToCheck = null,
            IContent contentItem = null)
        {
           
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
                                    ? UserExtensions.HasPathAccess(
                                        Constants.System.Root.ToInvariantString(),
                                        user.StartContentId,
                                        Constants.System.RecycleBinContent)
                                    : (nodeId == Constants.System.RecycleBinContent)
                                          ? UserExtensions.HasPathAccess(
                                              Constants.System.RecycleBinContent.ToInvariantString(),
                                              user.StartContentId,
                                              Constants.System.RecycleBinContent)
                                          : user.HasPathAccess(contentItem);

            if (hasPathAccess == false)
            {
                return false;
            }

            if (permissionsToCheck == null || permissionsToCheck.Any() == false)
            {
                return true;
            }

            var permission = userService.GetPermissions(user, nodeId).FirstOrDefault();

            var allowed = true;
            foreach (var p in permissionsToCheck)
            {
                if (permission == null || permission.AssignedPermissions.Contains(p.ToString(CultureInfo.InvariantCulture)) == false)
                {
                    allowed = false;
                }
            }
            return allowed;
        }

    }
}