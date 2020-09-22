using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Security;

namespace Umbraco.Web.BackOffice.Filters
{
    /// <summary>
    ///     Validates the incoming <see cref="MediaItemSave" /> model
    /// </summary>
    internal class MediaItemSaveValidationAttribute : TypeFilterAttribute
    {
        public MediaItemSaveValidationAttribute() : base(typeof(MediaItemSaveValidationFilter))
        {
        }

        private sealed class MediaItemSaveValidationFilter : IActionFilter
        {
            private readonly IEntityService _entityService;
            private readonly IPropertyValidationService _propertyValidationService;


            private readonly IMediaService _mediaService;
            private readonly ILocalizedTextService _textService;
            private readonly ILoggerFactory _loggerFactory;
            private readonly IBackofficeSecurityAccessor _backofficeSecurityAccessor;

            public MediaItemSaveValidationFilter(
                ILoggerFactory loggerFactory,
                IBackofficeSecurityAccessor backofficeSecurityAccessor,
                ILocalizedTextService textService,
                IMediaService mediaService,
                IEntityService entityService,
                IPropertyValidationService propertyValidationService)
            {
                _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
                _backofficeSecurityAccessor = backofficeSecurityAccessor ?? throw new ArgumentNullException(nameof(backofficeSecurityAccessor));
                _textService = textService ?? throw new ArgumentNullException(nameof(textService));
                _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
                _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
                _propertyValidationService = propertyValidationService ?? throw new ArgumentNullException(nameof(propertyValidationService));
            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                var model = (MediaItemSave) context.ActionArguments["contentItem"];
                var contentItemValidator = new MediaSaveModelValidator(_loggerFactory.CreateLogger<MediaSaveModelValidator>(), _backofficeSecurityAccessor.BackofficeSecurity, _textService, _propertyValidationService);

                if (ValidateUserAccess(model, context))
                {
                    //now do each validation step
                    if (contentItemValidator.ValidateExistingContent(model, context))
                        if (contentItemValidator.ValidateProperties(model, model, context))
                            contentItemValidator.ValidatePropertiesData(model, model, model.PropertyCollectionDto,
                                context.ModelState);
                }
            }

            /// <summary>
            ///     Checks if the user has access to post a content item based on whether it's being created or saved.
            /// </summary>
            /// <param name="mediaItem"></param>
            /// <param name="actionContext"></param>
            private bool ValidateUserAccess(MediaItemSave mediaItem, ActionExecutingContext actionContext)
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
                        actionContext.Result = new NotFoundResult();
                        return false;
                }

                if (MediaController.CheckPermissions(
                    actionContext.HttpContext.Items,
                    _backofficeSecurityAccessor.BackofficeSecurity.CurrentUser,
                    _mediaService, _entityService,
                    contentIdToCheck, contentToCheck) == false)
                {
                    actionContext.Result = new ForbidResult();
                    return false;
                }

                return true;
            }

            public void OnActionExecuted(ActionExecutedContext context)
            {

            }
        }
    }
}
