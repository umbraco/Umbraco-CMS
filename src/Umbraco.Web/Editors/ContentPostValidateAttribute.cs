using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Security;
using umbraco.BusinessLogic.Actions;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// Checks if the user has access to post a content item based on whether it's being created or saved.
    /// </summary>
    internal sealed class MediaPostValidateAttribute : ActionFilterAttribute
    {
        private readonly IMediaService _mediaService;
        private readonly WebSecurity _security;

        public MediaPostValidateAttribute()
        {
        }

        public MediaPostValidateAttribute(IMediaService mediaService, WebSecurity security)
        {
            if (mediaService == null) throw new ArgumentNullException("mediaService");
            if (security == null) throw new ArgumentNullException("security");
            _mediaService = mediaService;
            _security = security;
        }

        private IMediaService MediaService
        {
            get { return _mediaService ?? ApplicationContext.Current.Services.MediaService; }
        }

        private WebSecurity Security
        {
            get { return _security ?? UmbracoContext.Current.Security; }
        }
        
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var mediaItem = (ContentItemSave<IMedia>)actionContext.ActionArguments["contentItem"];

            //We now need to validate that the user is allowed to be doing what they are doing.
            //Then if it is new, we need to lookup those permissions on the parent.
            IMedia contentToCheck;
            switch (mediaItem.Action)
            {
                case ContentSaveAction.Save:
                    contentToCheck = mediaItem.PersistedContent;
                    break;                
                case ContentSaveAction.SaveNew:
                    contentToCheck = MediaService.GetById(mediaItem.ParentId);
                    break;
                case ContentSaveAction.Publish:
                case ContentSaveAction.PublishNew:
                default:
                    //we don't support this for media
                    actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                    return;
            }

            if (MediaController.CheckPermissions(
                actionContext.Request.Properties,
                Security.CurrentUser,
                MediaService,
                contentToCheck.Id,
                contentToCheck) == false)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                return;
            }
        }
    }

    /// <summary>
    /// Checks if the user has access to post a content item based on whether it's being created or saved.
    /// </summary>
    internal sealed class ContentPostValidateAttribute : ActionFilterAttribute
    {
        private readonly IContentService _contentService;
        private readonly WebSecurity _security;
        private readonly IUserService _userService;

        public ContentPostValidateAttribute()
        {            
        }

        public ContentPostValidateAttribute(IContentService contentService, IUserService userService, WebSecurity security)
        {
            if (contentService == null) throw new ArgumentNullException("contentService");
            if (userService == null) throw new ArgumentNullException("userService");
            if (security == null) throw new ArgumentNullException("security");
            _contentService = contentService;
            _userService = userService;
            _security = security;
        }

        private IContentService ContentService
        {
            get { return _contentService ?? ApplicationContext.Current.Services.ContentService; }
        }

        private WebSecurity Security
        {
            get { return _security ?? UmbracoContext.Current.Security; }
        }

        private IUserService UserService
        {
            get { return _userService ?? ApplicationContext.Current.Services.UserService; }
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var contentItem = (ContentItemSave<IContent>)actionContext.ActionArguments["contentItem"];

            //We now need to validate that the user is allowed to be doing what they are doing.
            //Based on the action we need to check different permissions.
            //Then if it is new, we need to lookup those permissions on the parent!
            char permissionToCheck;
            IContent contentToCheck;
            switch (contentItem.Action)
            {
                case ContentSaveAction.Save:
                    permissionToCheck = ActionSave.Instance.Letter;
                    contentToCheck = contentItem.PersistedContent;
                    break;
                case ContentSaveAction.Publish:
                    permissionToCheck = ActionPublish.Instance.Letter;
                    contentToCheck = contentItem.PersistedContent;
                    break;
                case ContentSaveAction.PublishNew:
                case ContentSaveAction.SaveNew:
                default:
                    permissionToCheck = ActionNew.Instance.Letter;
                    contentToCheck = ContentService.GetById(contentItem.ParentId);
                    break;
            }

            if (ContentController.CheckPermissions(
                actionContext.Request.Properties,
                Security.CurrentUser,
                UserService,
                ContentService,
                contentToCheck.Id,
                permissionToCheck,
                contentToCheck) == false)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                return;
            }
        }
    }
}