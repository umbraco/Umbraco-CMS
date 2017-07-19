using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Security;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// Checks if the user has access to post a content item based on whether it's being created or saved.
    /// </summary>
    internal sealed class MediaPostValidateAttribute : ActionFilterAttribute
    {
        private readonly IMediaService _mediaService;
        private readonly IEntityService _entityService;
        private readonly WebSecurity _security;

        public MediaPostValidateAttribute()
        {
        }

        public MediaPostValidateAttribute(IMediaService mediaService, IEntityService entityService, WebSecurity security)
        {
            if (mediaService == null) throw new ArgumentNullException("mediaService");
            if (security == null) throw new ArgumentNullException("security");
            _mediaService = mediaService;
            _entityService = entityService;
            _security = security;
        }

        private IMediaService MediaService
        {
            get { return _mediaService ?? ApplicationContext.Current.Services.MediaService; }
        }

        private IEntityService EntityService
        {
            get { return _entityService ?? ApplicationContext.Current.Services.EntityService; }
        }

        private WebSecurity Security
        {
            get { return _security ?? UmbracoContext.Current.Security; }
        }
        
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var mediaItem = (MediaItemSave)actionContext.ActionArguments["contentItem"];

            //We now need to validate that the user is allowed to be doing what they are doing.
            //Then if it is new, we need to lookup those permissions on the parent.
            IMedia contentToCheck = null;
            int contentIdToCheck;
            switch (mediaItem.Action)
            {
                case ContentSaveAction.Save:
                    contentToCheck = mediaItem.PersistedContent;
                    contentIdToCheck = contentToCheck.Id;
                    break;                
                case ContentSaveAction.SaveNew:
                    contentToCheck = MediaService.GetById(mediaItem.ParentId);

                    if (mediaItem.ParentId != Constants.System.Root)
                    {
                        contentToCheck = MediaService.GetById(mediaItem.ParentId);
                        contentIdToCheck = contentToCheck.Id;
                    }
                    else
                    {
                        contentIdToCheck = mediaItem.ParentId;
                    }

                    break;
                default:
                    //we don't support this for media
                    actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.NotFound);
                    return;
            }

            if (MediaController.CheckPermissions(
                actionContext.Request.Properties,
                Security.CurrentUser,
                MediaService,
                EntityService,
                contentIdToCheck,
                contentToCheck) == false)
            {
                throw new HttpResponseException(actionContext.Request.CreateUserNoAccessResponse());
            }
        }
    }
}