using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Editors.Filters
{
    /// <summary>
    /// Validates the incoming <see cref="MediaItemSave"/> model
    /// </summary>
    internal class MediaItemSaveValidationAttribute : ActionFilterAttribute
    {
        private readonly ILogger _logger;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IMediaService _mediaService;
        private readonly IEntityService _entityService;

        public MediaItemSaveValidationAttribute() : this(Current.Logger, Current.UmbracoContextAccessor, Current.Services.MediaService, Current.Services.EntityService)
        {
        }

        public MediaItemSaveValidationAttribute(ILogger logger, IUmbracoContextAccessor umbracoContextAccessor, IMediaService mediaService, IEntityService entityService)
        {
            _logger = logger;
            _umbracoContextAccessor = umbracoContextAccessor;
            _mediaService = mediaService;
            _entityService = entityService;
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var model = (MediaItemSave)actionContext.ActionArguments["contentItem"];
            var contentItemValidator = new ContentModelValidator<IMedia, MediaItemSave, IContentProperties<ContentPropertyBasic>>(_logger, _umbracoContextAccessor);

            if (ValidateUserAccess(model, actionContext))
            {
                //now do each validation step
                if (contentItemValidator.ValidateExistingContent(model, actionContext))
                    if (contentItemValidator.ValidateProperties(model, model, actionContext))
                        contentItemValidator.ValidatePropertiesData(model, model, model.PropertyCollectionDto, actionContext.ModelState);
            }
        }

        /// <summary>
        /// Checks if the user has access to post a content item based on whether it's being created or saved.
        /// </summary>
        /// <param name="mediaItem"></param>
        /// <param name="actionContext"></param>
        private bool ValidateUserAccess(MediaItemSave mediaItem, HttpActionContext actionContext)
        {
            //We now need to validate that the user is allowed to be doing what they are doing.
            //Then if it is new, we need to lookup those permissions on the parent.
            IMedia contentToCheck;
            int contentIdToCheck;
            switch (mediaItem.Action)
            {
                case ContentSaveAction.Save:
                    contentToCheck = mediaItem.PersistedContent;
                    contentIdToCheck = contentToCheck.Id;
                    break;
                case ContentSaveAction.SaveNew:
                    contentToCheck = _mediaService.GetById(mediaItem.ParentId);

                    if (mediaItem.ParentId != Constants.System.Root)
                    {
                        contentToCheck = _mediaService.GetById(mediaItem.ParentId);
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
                    return false;
            }

            if (MediaController.CheckPermissions(
                    actionContext.Request.Properties,
                    _umbracoContextAccessor.UmbracoContext.Security.CurrentUser,
                    _mediaService, _entityService,
                    contentIdToCheck, contentToCheck) == false)
            {
                actionContext.Response = actionContext.Request.CreateUserNoAccessResponse();
                return false;
            }

            return true;
        }
    }
}
