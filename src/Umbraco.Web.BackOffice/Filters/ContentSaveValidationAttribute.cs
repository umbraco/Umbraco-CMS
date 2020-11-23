using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Actions;
using Umbraco.Web.BackOffice.Authorization;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Security;

namespace Umbraco.Web.BackOffice.Filters
{
    /// <summary>
    ///     Validates the incoming <see cref="ContentItemSave" /> model along with if the user is allowed to perform the
    ///     operation
    /// </summary>
    internal sealed class ContentSaveValidationAttribute : TypeFilterAttribute
    {
        public ContentSaveValidationAttribute() : base(typeof(ContentSaveValidationFilter))
        {
            Order = -3000; // More important than ModelStateInvalidFilter.FilterOrder
        }


        private sealed class ContentSaveValidationFilter : IAsyncActionFilter
        {
            private readonly IContentService _contentService;
            private readonly IEntityService _entityService;
            private readonly IPropertyValidationService _propertyValidationService;
            private readonly ContentPermissions _contentPermissions;
            private readonly IAuthorizationService _authorizationService;
            private readonly ILoggerFactory _loggerFactory;
            private readonly ILocalizedTextService _textService;
            private readonly IUserService _userService;
            private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;


            public ContentSaveValidationFilter(
                ILoggerFactory loggerFactory,
                IBackOfficeSecurityAccessor backofficeSecurityAccessor,
                ILocalizedTextService textService,
                IContentService contentService,
                IUserService userService,
                IEntityService entityService,
                IPropertyValidationService propertyValidationService,
                ContentPermissions contentPermissions,
                IAuthorizationService authorizationService)
            {
                _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
                _backofficeSecurityAccessor = backofficeSecurityAccessor ?? throw new ArgumentNullException(nameof(backofficeSecurityAccessor));
                _textService = textService ?? throw new ArgumentNullException(nameof(textService));
                _contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));
                _userService = userService ?? throw new ArgumentNullException(nameof(userService));
                _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
                _propertyValidationService = propertyValidationService ?? throw new ArgumentNullException(nameof(propertyValidationService));
                _contentPermissions = contentPermissions;
                _authorizationService = authorizationService;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                // on executing...
                await OnActionExecutingAsync(context);

                await next(); //need to pass the execution to next

                // on executed...
            }

            private async Task OnActionExecutingAsync(ActionExecutingContext context)
            {
                var model = (ContentItemSave) context.ActionArguments["contentItem"];
                var contentItemValidator = new ContentSaveModelValidator(_loggerFactory.CreateLogger<ContentSaveModelValidator>(), _backofficeSecurityAccessor.BackOfficeSecurity, _textService, _propertyValidationService);

                if (!ValidateAtLeastOneVariantIsBeingSaved(model, context)) return;
                if (!contentItemValidator.ValidateExistingContent(model, context)) return;
                if (!await ValidateUserAccessAsync(model, context, _backofficeSecurityAccessor.BackOfficeSecurity)) return;

                //validate for each variant that is being updated
                foreach (var variant in model.Variants.Where(x => x.Save))
                {
                    if (contentItemValidator.ValidateProperties(model, variant, context))
                        contentItemValidator.ValidatePropertiesData(model, variant, variant.PropertyCollectionDto,
                            context.ModelState);
                }
            }


            /// <summary>
            ///     If there are no variants tagged for Saving, then this is an invalid request
            /// </summary>
            /// <param name="contentItem"></param>
            /// <param name="actionContext"></param>
            /// <returns></returns>
            private bool ValidateAtLeastOneVariantIsBeingSaved(
                ContentItemSave contentItem,
                ActionExecutingContext actionContext)
            {
                if (!contentItem.Variants.Any(x => x.Save))
                {
                    actionContext.Result = new NotFoundObjectResult(new {Message = "No variants flagged for saving"});
                    return false;
                }

                return true;
            }

            /// <summary>
            ///     Checks if the user has access to post a content item based on whether it's being created or saved.
            /// </summary>
            /// <param name="actionContext"></param>
            /// <param name="contentItem"></param>
            /// <param name="backofficeSecurity"></param>
            private async Task<bool> ValidateUserAccessAsync(
                ContentItemSave contentItem,
                ActionExecutingContext actionContext,
                IBackOfficeSecurity backofficeSecurity)
            {
                // We now need to validate that the user is allowed to be doing what they are doing.
                // Based on the action we need to check different permissions.
                // Then if it is new, we need to lookup those permissions on the parent!

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


                var requirement = contentToCheck == null
                    ? new ContentPermissionsResourceRequirement(contentIdToCheck, permissionToCheck)
                    : new ContentPermissionsResourceRequirement(permissionToCheck);

                var authorizationResult = await _authorizationService.AuthorizeAsync(actionContext.HttpContext.User, contentToCheck, requirement);
                if (!authorizationResult.Succeeded)
                {
                    actionContext.Result = new ForbidResult();
                    return false;
                }

                return true;
            }

            
        }
    }
}
