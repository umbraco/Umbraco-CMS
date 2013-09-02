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
}