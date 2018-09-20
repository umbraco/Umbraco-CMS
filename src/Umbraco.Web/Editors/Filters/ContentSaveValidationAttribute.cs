﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Security;
using Umbraco.Web.WebApi;
using Umbraco.Web._Legacy.Actions;

namespace Umbraco.Web.Editors.Filters
{
    /// <summary>
    /// Validates the incoming <see cref="ContentItemSave"/> model along with if the user is allowed to perform the operation
    /// </summary>
    internal sealed class ContentSaveValidationAttribute : ActionFilterAttribute
    {
        public ContentSaveValidationAttribute(): this(Current.Logger, Current.UmbracoContextAccessor, Current.Services.ContentService, Current.Services.UserService, Current.Services.EntityService, UmbracoContext.Current.Security)
        { }

        public ContentSaveValidationAttribute(ILogger logger, IUmbracoContextAccessor umbracoContextAccessor, IContentService contentService, IUserService userService, IEntityService entityService, WebSecurity security)
        {
            _logger = logger;
            _umbracoContextAccessor = umbracoContextAccessor;
            _contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
            _security = security ?? throw new ArgumentNullException(nameof(security));
        }

        private readonly ILogger _logger;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IContentService _contentService;
        private readonly WebSecurity _security;
        private readonly IUserService _userService;
        private readonly IEntityService _entityService;

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var model = (ContentItemSave)actionContext.ActionArguments["contentItem"];
            var contentItemValidator = new ContentItemValidationHelper<IContent, ContentItemSave>(_logger, _umbracoContextAccessor);

            if (!ValidateAtLeastOneVariantIsBeingSaved(model, actionContext)) return;
            if (!contentItemValidator.ValidateExistingContent(model, actionContext)) return;
            if (!ValidateUserAccess(model, actionContext)) return;
            
            //validate for each variant that is being updated
            foreach (var variant in model.Variants.Where(x => x.Save))
            {
                if (contentItemValidator.ValidateProperties(model, variant, actionContext))
                    contentItemValidator.ValidatePropertyData(model, variant, variant.PropertyCollectionDto, actionContext.ModelState);
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
        private bool ValidateUserAccess(ContentItemSave contentItem, HttpActionContext actionContext)
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
                    permissionToCheck.Add(ActionUpdate.Instance.Letter);
                    contentToCheck = contentItem.PersistedContent;
                    contentIdToCheck = contentToCheck.Id;
                    break;
                case ContentSaveAction.Publish:
                    permissionToCheck.Add(ActionPublish.Instance.Letter);
                    contentToCheck = contentItem.PersistedContent;
                    contentIdToCheck = contentToCheck.Id;
                    break;
                case ContentSaveAction.SendPublish:
                    permissionToCheck.Add(ActionToPublish.Instance.Letter);
                    contentToCheck = contentItem.PersistedContent;
                    contentIdToCheck = contentToCheck.Id;
                    break;
                case ContentSaveAction.SaveNew:
                    //Save new requires ActionNew

                    permissionToCheck.Add(ActionNew.Instance.Letter);

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

                    permissionToCheck.Add(ActionNew.Instance.Letter);
                    permissionToCheck.Add(ActionToPublish.Instance.Letter);
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
                    //Publish new requires both ActionNew AND ActionPublish
                    //TODO: Shoudn't publish also require ActionUpdate since it will definitely perform an update to publish but maybe that's just implied

                    permissionToCheck.Add(ActionNew.Instance.Letter);
                    permissionToCheck.Add(ActionPublish.Instance.Letter);

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

            if (ContentController.CheckPermissions(
                actionContext.Request.Properties,
                _security.CurrentUser,
                _userService, _contentService, _entityService,
                contentIdToCheck, permissionToCheck.ToArray(), contentToCheck) == false)
            {
                actionContext.Response = actionContext.Request.CreateUserNoAccessResponse();
                return false;
            }

            return true;
        }
    }
}
