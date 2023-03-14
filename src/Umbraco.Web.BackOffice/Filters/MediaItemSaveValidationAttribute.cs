using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Authorization;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Web.BackOffice.Filters;

/// <summary>
///     Validates the incoming <see cref="MediaItemSave" /> model
/// </summary>
internal class MediaItemSaveValidationAttribute : TypeFilterAttribute
{
    public MediaItemSaveValidationAttribute() : base(typeof(MediaItemSaveValidationFilter))
    {
    }

    private sealed class MediaItemSaveValidationFilter : IAsyncActionFilter
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IMediaService _mediaService;
        private readonly IPropertyValidationService _propertyValidationService;

        public MediaItemSaveValidationFilter(
            ILoggerFactory loggerFactory,
            IMediaService mediaService,
            IPropertyValidationService propertyValidationService,
            IAuthorizationService authorizationService)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
            _propertyValidationService = propertyValidationService ??
                                         throw new ArgumentNullException(nameof(propertyValidationService));
            _authorizationService =
                authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // on executing...
            await OnActionExecutingAsync(context);

            if (context.Result == null)
            {
                //need to pass the execution to next if a result was not set
                await next();
            }

            // on executed...
        }

        private async Task OnActionExecutingAsync(ActionExecutingContext context)
        {
            var model = (MediaItemSave?)context.ActionArguments["contentItem"];
            var contentItemValidator =
                new MediaSaveModelValidator(_loggerFactory.CreateLogger<MediaSaveModelValidator>(), _propertyValidationService);

            if (await ValidateUserAccessAsync(model, context))
            {
                //now do each validation step
                if (contentItemValidator.ValidateExistingContent(model, context))
                {
                    if (contentItemValidator.ValidateProperties(model, model, context))
                    {
                        contentItemValidator.ValidatePropertiesData(model, model, model?.PropertyCollectionDto, context.ModelState);
                    }
                }
            }
        }

        /// <summary>
        ///     Checks if the user has access to post a content item based on whether it's being created or saved.
        /// </summary>
        /// <param name="mediaItem"></param>
        /// <param name="actionContext"></param>
        private async Task<bool> ValidateUserAccessAsync(MediaItemSave? mediaItem, ActionExecutingContext actionContext)
        {
            //We now need to validate that the user is allowed to be doing what they are doing.
            //Then if it is new, we need to lookup those permissions on the parent.
            IMedia? contentToCheck;
            int contentIdToCheck;
            switch (mediaItem?.Action)
            {
                case ContentSaveAction.Save:
                    contentToCheck = mediaItem.PersistedContent;
                    contentIdToCheck = contentToCheck?.Id ?? default;
                    break;
                case ContentSaveAction.SaveNew:
                    contentToCheck = _mediaService.GetById(mediaItem.ParentId);

                    if (mediaItem.ParentId != Constants.System.Root)
                    {
                        contentToCheck = _mediaService.GetById(mediaItem.ParentId);
                        contentIdToCheck = contentToCheck?.Id ?? default;
                    }
                    else
                    {
                        contentIdToCheck = mediaItem.ParentId;
                    }

                    break;
                default:
                    //we don't support this for media
                    actionContext.Result = new NotFoundResult();
                    return false;
            }

            MediaPermissionsResource resource = contentToCheck == null
                ? new MediaPermissionsResource(contentIdToCheck)
                : new MediaPermissionsResource(contentToCheck);

            AuthorizationResult authorizationResult = await _authorizationService.AuthorizeAsync(
                actionContext.HttpContext.User,
                resource,
                AuthorizationPolicies.MediaPermissionByResource);

            if (!authorizationResult.Succeeded)
            {
                actionContext.Result = new ForbidResult();
                return false;
            }

            return true;
        }
    }
}
