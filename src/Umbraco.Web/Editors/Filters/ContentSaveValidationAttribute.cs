using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Actions;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Security;

namespace Umbraco.Web.Editors.Filters
{
    /// <summary>
    /// Validates the incoming <see cref="ContentItemSave"/> model along with if the user is allowed to perform the operation
    /// </summary>
    internal sealed class ContentSaveValidationAttribute : ActionFilterAttribute
    {
        private readonly ILogger _logger;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly ILocalizedTextService _textService;
        private readonly IContentService _contentService;
        private readonly IUserService _userService;
        private readonly IEntityService _entityService;
        private readonly AppCaches _appCaches;

        public ContentSaveValidationAttribute(): this(Current.Logger, Current.UmbracoContextAccessor, Current.Services.TextService, Current.Services.ContentService, Current.Services.UserService, Current.Services.EntityService, Current.AppCaches)
        { }

        public ContentSaveValidationAttribute(ILogger logger, IUmbracoContextAccessor umbracoContextAccessor, ILocalizedTextService textService, IContentService contentService, IUserService userService, IEntityService entityService, AppCaches appCaches)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
            _textService = textService ?? throw new ArgumentNullException(nameof(textService));
            _contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
            _appCaches = appCaches;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var model = (ContentItemSave)actionContext.ActionArguments["contentItem"];
            var contentItemValidator = new ContentSaveModelValidator(_logger, _umbracoContextAccessor, _textService);

            if (!ValidateAtLeastOneVariantIsBeingSaved(model, actionContext)) return;
            if (!contentItemValidator.ValidateExistingContent(model, actionContext)) return;
            if (!ValidateUserAccess(model, actionContext, _umbracoContextAccessor.UmbracoContext.Security)) return;
            
            //validate for each variant that is being updated
            foreach (var variant in model.Variants.Where(x => x.Save))
            {
                if (contentItemValidator.ValidateProperties(model, variant, actionContext))
                    contentItemValidator.ValidatePropertiesData(model, variant, variant.PropertyCollectionDto, actionContext.ModelState);
            }
        }

        /// <summary>
        /// If there are no variants tagged for Saving, then this is an invalid request
        /// </summary>
        /// <param name="contentItem"></param>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        private bool ValidateAtLeastOneVariantIsBeingSaved(ContentItemSave contentItem, HttpActionContext actionContext)
        {
            if (!contentItem.Variants.Any(x => x.Save))
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, "No variants flagged for saving");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if the user has access to post a content item based on whether it's being created or saved.
        /// </summary>
        /// <param name="actionContext"></param>
        /// <param name="contentItem"></param>
        /// <param name="webSecurity"></param>
        private bool ValidateUserAccess(ContentItemSave contentItem, HttpActionContext actionContext, WebSecurity webSecurity)
        {  

            //We now need to validate that the user is allowed to be doing what they are doing.
            //Based on the action we need to check different permissions.
            //Then if it is new, we need to lookup those permissions on the parent!

            var permissionToCheck = new List<char>();
            IContent contentToCheck = null;
            int contentIdToCheck;
            switch (contentItem.Action)
            {
                case ContentSaveAction.Save:
                    permissionToCheck.Add(ActionUpdate.ActionLetter);
                    contentToCheck = contentItem.PersistedContent;
                    contentIdToCheck = contentToCheck.Id;
                    break;
                case ContentSaveAction.Publish:
                case ContentSaveAction.PublishWithDescendants:
                case ContentSaveAction.PublishWithDescendantsForce:
                    permissionToCheck.Add(ActionPublish.ActionLetter);
                    contentToCheck = contentItem.PersistedContent;
                    contentIdToCheck = contentToCheck.Id;
                    break;
                case ContentSaveAction.SendPublish:
                    permissionToCheck.Add(ActionToPublish.ActionLetter);
                    contentToCheck = contentItem.PersistedContent;
                    contentIdToCheck = contentToCheck.Id;
                    break;
                case ContentSaveAction.Schedule:
                    permissionToCheck.Add(ActionUpdate.ActionLetter);
                    permissionToCheck.Add(ActionToPublish.ActionLetter);
                    contentToCheck = contentItem.PersistedContent;
                    contentIdToCheck = contentToCheck.Id;
                    break;
                case ContentSaveAction.SaveNew:
                    //Save new requires ActionNew

                    permissionToCheck.Add(ActionNew.ActionLetter);

                    if (contentItem.ParentId != Constants.System.Root)
                    {
                        contentToCheck = _contentService.GetById(contentItem.ParentId);
                        contentIdToCheck = contentToCheck.Id;
                    }
                    else
                    {
                        contentIdToCheck = contentItem.ParentId;
                    }
                    break;
                case ContentSaveAction.SendPublishNew:
                    //Send new requires both ActionToPublish AND ActionNew

                    permissionToCheck.Add(ActionNew.ActionLetter);
                    permissionToCheck.Add(ActionToPublish.ActionLetter);
                    if (contentItem.ParentId != Constants.System.Root)
                    {
                        contentToCheck = _contentService.GetById(contentItem.ParentId);
                        contentIdToCheck = contentToCheck.Id;
                    }
                    else
                    {
                        contentIdToCheck = contentItem.ParentId;
                    }
                    break;
                case ContentSaveAction.PublishNew:
                case ContentSaveAction.PublishWithDescendantsNew:
                case ContentSaveAction.PublishWithDescendantsForceNew:
                    //Publish new requires both ActionNew AND ActionPublish
                    // TODO: Shouldn't publish also require ActionUpdate since it will definitely perform an update to publish but maybe that's just implied

                    permissionToCheck.Add(ActionNew.ActionLetter);
                    permissionToCheck.Add(ActionPublish.ActionLetter);

                    if (contentItem.ParentId != Constants.System.Root)
                    {
                        contentToCheck = _contentService.GetById(contentItem.ParentId);
                        contentIdToCheck = contentToCheck.Id;
                    }
                    else
                    {
                        contentIdToCheck = contentItem.ParentId;
                    }
                    break;
                case ContentSaveAction.ScheduleNew:
                    
                    permissionToCheck.Add(ActionNew.ActionLetter);
                    permissionToCheck.Add(ActionUpdate.ActionLetter);
                    permissionToCheck.Add(ActionPublish.ActionLetter);

                    if (contentItem.ParentId != Constants.System.Root)
                    {
                        contentToCheck = _contentService.GetById(contentItem.ParentId);
                        contentIdToCheck = contentToCheck.Id;
                    }
                    else
                    {
                        contentIdToCheck = contentItem.ParentId;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            ContentPermissionsHelper.ContentAccess accessResult;
            if (contentToCheck != null)
            {
                //store the content item in request cache so it can be resolved in the controller without re-looking it up
                actionContext.Request.Properties[typeof(IContent).ToString()] = contentItem;

                accessResult = ContentPermissionsHelper.CheckPermissions(
                    contentToCheck, webSecurity.CurrentUser,
                    _userService, _entityService, _appCaches, permissionToCheck.ToArray());
            }
            else
            {
                accessResult = ContentPermissionsHelper.CheckPermissions(
                       contentIdToCheck, webSecurity.CurrentUser,
                       _userService, _contentService, _entityService, _appCaches,
                       out contentToCheck,
                       permissionToCheck.ToArray());
                if (contentToCheck != null)
                {
                    //store the content item in request cache so it can be resolved in the controller without re-looking it up
                    actionContext.Request.Properties[typeof(IContent).ToString()] = contentToCheck;
                }
            }

            if (accessResult == ContentPermissionsHelper.ContentAccess.NotFound)
                throw new HttpResponseException(HttpStatusCode.NotFound);

            return accessResult == ContentPermissionsHelper.ContentAccess.Granted;
        }
    }
}
