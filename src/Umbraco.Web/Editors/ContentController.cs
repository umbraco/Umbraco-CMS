using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
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
            var foundContent = ((ContentService) Services.ContentService).GetByIds(ids);
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
            var foundContent = GetEntityFromRequest(() => Services.ContentService.GetById(id));            
            if (foundContent == null)
            {
                HandleContentNotFound(id);
            }
            
            var content = Mapper.Map<IContent, ContentItemDisplay>(foundContent);
            return content;

//            content.Tabs.ElementAt(0).Properties.
        }

        /// <summary>
        /// Gets an empty content item for the 
        /// </summary>
        /// <param name="contentTypeAlias"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public ContentItemDisplay GetEmpty(string contentTypeAlias, int parentId)
        {
            var contentType = Services.ContentTypeService.GetContentType(contentTypeAlias);
            if (contentType == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var emptyContent = new Content("", parentId, contentType);
            return Mapper.Map<IContent, ContentItemDisplay>(emptyContent);
        }
        
        /// <summary>
        /// Gets the children for the content id passed in
        /// </summary>
        /// <returns></returns>        
        [FilterAllowedOutgoingContent(typeof(IEnumerable<ContentItemBasic<ContentPropertyBasic, IContent>>), "Items")]
        public PagedResult<ContentItemBasic<ContentPropertyBasic, IContent>> GetChildren(
            int id, 
            int pageNumber = 0, 
            int pageSize = 0, 
            string orderBy = "SortOrder", 
            Direction orderDirection = Direction.Ascending, 
            string filter = "")
        {
            //TODO: Not sure how to handle 'filter' just yet!

            //TODO: This will be horribly inefficient for paging! This is because our datasource/repository 
            // doesn't support paging at the SQL level... and it'll be pretty interesting to try to make that work.

            var foundContent = Services.ContentService.GetById(id);
            if (foundContent == null)
            {
                HandleContentNotFound(id);
            }
            
            var totalChildren = ((ContentService) Services.ContentService).CountChildren(id);
            var result = foundContent.Children()
                                     .Select(Mapper.Map<IContent, ContentItemBasic<ContentPropertyBasic, IContent>>)
                                     .AsQueryable();

            var orderedResult = orderDirection == Direction.Ascending 
                ? result.OrderBy(orderBy) 
                : result.OrderByDescending(orderBy);
            
            var pagedResult = new PagedResult<ContentItemBasic<ContentPropertyBasic, IContent>>(
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
        /// Saves content
        /// </summary>
        /// <returns></returns>
        [FileUploadCleanupFilter]
        //[ContentPostValidate]
        public ContentItemDisplay PostSave(
            [ModelBinder(typeof(ContentItemBinder))]
                ContentItemSave<IContent> contentItem)
        {            
            //If we've reached here it means:
            // * Our model has been bound
            // * and validated
            // * any file attachments have been saved to their temporary location for us to use
            // * we have a reference to the DTO object and the persisted object
            // * Permissions are valid
            
            UpdateName(contentItem);
            
            //TODO: We need to support 'send to publish'

            //TODO: We'll need to save the new template, publishat, etc... values here
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
                    && (contentItem.Action == ContentSaveAction.SaveNew || contentItem.Action == ContentSaveAction.PublishNew))
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
                Services.ContentService.Save(contentItem.PersistedContent, (int)Security.CurrentUser.Id);
            }
            else
            {
                //publish the item and check if it worked, if not we will show a diff msg below
                publishStatus = ((ContentService)Services.ContentService).SaveAndPublishInternal(contentItem.PersistedContent, (int)Security.CurrentUser.Id);
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
                case ContentSaveAction.Publish:
                case ContentSaveAction.PublishNew:
                    ShowMessageForStatus(publishStatus.Result, display);
                    break;
            }

            return display;
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
        public HttpResponseMessage DeleteById(int id)
        {
            //TODO: We need to check if the user is allowed to do this!

            var foundContent = Services.ContentService.GetById(id);
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
        [EnsureUserPermissionForContent(Constants.System.RecycleBinContent)]
        public HttpResponseMessage EmptyRecycleBin()
        {            
            //TODO: We need to check if the user is allowed access to the recycle bin!

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
            //TODO: We need to check if the user is allowed to sort here!

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
                sortedContent.AddRange(((ContentService) Services.ContentService).GetByIds(sorted.IdSortOrder));

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

        private void ShowMessageForStatus(PublishStatus status, ContentItemDisplay display)
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
                case PublishStatusType.FailedHasExpired:
                case PublishStatusType.FailedAwaitingRelease:
                case PublishStatusType.FailedIsTrashed:
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
        /// <param name="permissionToCheck"></param>
        /// <param name="contentItem">Specifies the already resolved content item to check against, setting this ignores the nodeId</param>
        /// <returns></returns>
        internal static bool CheckPermissions(
            IDictionary<string, object> storage,
            IUser user,
            IUserService userService,
            IContentService contentService,
            int nodeId,
            char permissionToCheck,
            IContent contentItem = null)
        {
            if (contentItem == null)
            {
                contentItem = contentService.GetById(nodeId);
            }

            if (contentItem == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            //put the content item into storage so it can be retreived 
            // in the controller (saves a lookup)
            storage[typeof(IContent).ToString()] = contentItem;

            var hasPathAccess = user.HasPathAccess(contentItem);

            if (hasPathAccess == false)
            {
                return false;
            }

            var permission = userService.GetPermissions(user, nodeId).FirstOrDefault();
            if (permission == null || permission.AssignedPermissions.Contains(permissionToCheck.ToString(CultureInfo.InvariantCulture)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}