using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Security;
using Umbraco.Web.WebApi;
using Umbraco.Web._Legacy.Actions;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// Checks if the user has access to post a content item based on whether it's being created or saved.
    /// </summary>
    internal sealed class ContentPostValidateAttribute : ActionFilterAttribute
    {
        private readonly IContentService _contentService;
        private readonly WebSecurity _security;
        private readonly IUserService _userService;
        private readonly IEntityService _entityService;

        public ContentPostValidateAttribute()
        { }

        // fixme wtf is this?
        public ContentPostValidateAttribute(IContentService contentService, IUserService userService, IEntityService entityService, WebSecurity security)
        {
            _contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
            _security = security ?? throw new ArgumentNullException(nameof(security));
        }

        // fixme all these should be injected properties

        private IContentService ContentService
            => _contentService ?? Current.Services.ContentService;

        private WebSecurity Security
            => _security ?? UmbracoContext.Current.Security;

        private IUserService UserService
            => _userService ?? Current.Services.UserService;

        private IEntityService EntityService
            => _entityService ?? Current.Services.EntityService;

        public override bool AllowMultiple
            => true;

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var contentItem = (ContentItemSave)actionContext.ActionArguments["contentItem"];

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
                        contentToCheck = ContentService.GetById(contentItem.ParentId);
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
                        contentToCheck = ContentService.GetById(contentItem.ParentId);
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
                        contentToCheck = ContentService.GetById(contentItem.ParentId);
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
                Security.CurrentUser,
                UserService, ContentService, EntityService,
                contentIdToCheck, permissionToCheck.ToArray(), contentToCheck) == false)
            {
                actionContext.Response = actionContext.Request.CreateUserNoAccessResponse();
            }
        }
    }
}
