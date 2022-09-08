using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.Authorization;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Web.BackOffice.Filters;

/// <summary>
///     Validates the incoming <see cref="ContentItemSave" /> model along with if the user is allowed to perform the
///     operation
/// </summary>
internal sealed class ContentSaveValidationAttribute : TypeFilterAttribute
{
    public ContentSaveValidationAttribute() : base(typeof(ContentSaveValidationFilter)) =>
        Order = -3000; // More important than ModelStateInvalidFilter.FilterOrder


    private sealed class ContentSaveValidationFilter : IAsyncActionFilter
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentService _contentService;
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IPropertyValidationService _propertyValidationService;


        public ContentSaveValidationFilter(
            ILoggerFactory loggerFactory,
            IContentService contentService,
            IPropertyValidationService propertyValidationService,
            IAuthorizationService authorizationService,
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
            ILocalizationService localizationService)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));
            _propertyValidationService = propertyValidationService ??
                                         throw new ArgumentNullException(nameof(propertyValidationService));
            _authorizationService = authorizationService;
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
            _localizationService = localizationService;
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
            var model = (ContentItemSave?)context.ActionArguments["contentItem"];
            var contentItemValidator =
                new ContentSaveModelValidator(_loggerFactory.CreateLogger<ContentSaveModelValidator>(), _propertyValidationService);

            if (context.ModelState.ContainsKey("contentItem"))
            {
                // if the entire model is marked as error, remove it, we handle everything separately
                context.ModelState.Remove("contentItem");
            }

            if (!ValidateAtLeastOneVariantIsBeingSaved(model, context))
            {
                return;
            }

            if (!contentItemValidator.ValidateExistingContent(model, context))
            {
                return;
            }

            if (!await ValidateUserAccessAsync(model, context))
            {
                return;
            }

            if (model is not null)
            {
                //validate for each variant that is being updated
                foreach (ContentVariantSave variant in model.Variants.Where(x => x.Save))
                {
                    if (contentItemValidator.ValidateProperties(model, variant, context))
                    {
                        contentItemValidator.ValidatePropertiesData(model, variant, variant.PropertyCollectionDto, context.ModelState);
                    }
                }
            }
        }


        /// <summary>
        ///     If there are no variants tagged for Saving, then this is an invalid request
        /// </summary>
        /// <param name="contentItem"></param>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        private bool ValidateAtLeastOneVariantIsBeingSaved(
            ContentItemSave? contentItem,
            ActionExecutingContext actionContext)
        {
            if (!contentItem?.Variants.Any(x => x.Save) ?? true)
            {
                actionContext.Result = new NotFoundObjectResult(new { Message = "No variants flagged for saving" });
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
            ContentItemSave? contentItem,
            ActionExecutingContext actionContext)
        {
            // We now need to validate that the user is allowed to be doing what they are doing.
            // Based on the action we need to check different permissions.
            // Then if it is new, we need to lookup those permissions on the parent!

            var permissionToCheck = new List<char>();
            IContent? contentToCheck = null;
            int contentIdToCheck;

            // First check if user has Access to that language
            IUser? currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
            bool hasAccess = false;
            if (currentUser is null)
            {
                return false;
            }

            foreach (IReadOnlyUserGroup group in currentUser.Groups)
            {
                IEnumerable<ILanguage> languages = _localizationService.GetAllLanguages().Where(x => group.AllowedLanguages.Contains(x.Id));
                if (group.AllowedLanguages.Count() is 0 ||
                    languages.Select(x => x.IsoCode).Intersect(contentItem?.Variants.Where(x => x.Save || x.Publish).Select(x => x.Culture) ?? Enumerable.Empty<string>()).Count() is not 0)
                {
                    hasAccess = true;
                }
            }

            if (!hasAccess && contentItem?.Variants.First().Culture is not null)
            {
                actionContext.ModelState.AddModelError(Constants.ModelStateErrorKeys.PermissionError, "User does not have access to save language");
                return false;
            }

            switch (contentItem?.Action)
            {
                case ContentSaveAction.Save:
                    permissionToCheck.Add(ActionUpdate.ActionLetter);
                    contentToCheck = contentItem.PersistedContent;
                    contentIdToCheck = contentToCheck?.Id ?? default;
                    break;
                case ContentSaveAction.Publish:
                case ContentSaveAction.PublishWithDescendants:
                case ContentSaveAction.PublishWithDescendantsForce:
                    permissionToCheck.Add(ActionPublish.ActionLetter);
                    contentToCheck = contentItem.PersistedContent;
                    contentIdToCheck = contentToCheck?.Id ?? default;
                    break;
                case ContentSaveAction.SendPublish:
                    permissionToCheck.Add(ActionToPublish.ActionLetter);
                    contentToCheck = contentItem.PersistedContent;
                    contentIdToCheck = contentToCheck?.Id ?? default;
                    break;
                case ContentSaveAction.Schedule:
                    permissionToCheck.Add(ActionUpdate.ActionLetter);
                    permissionToCheck.Add(ActionToPublish.ActionLetter);
                    contentToCheck = contentItem.PersistedContent;
                    contentIdToCheck = contentToCheck?.Id ?? default;
                    break;
                case ContentSaveAction.SaveNew:
                    //Save new requires ActionNew

                    permissionToCheck.Add(ActionNew.ActionLetter);

                    if (contentItem.ParentId != Constants.System.Root)
                    {
                        contentToCheck = _contentService.GetById(contentItem.ParentId);
                        contentIdToCheck = contentToCheck?.Id ?? default;
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
                        contentIdToCheck = contentToCheck?.Id ?? default;
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
                        contentIdToCheck = contentToCheck?.Id ?? default;
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
                        contentIdToCheck = contentToCheck?.Id ?? default;
                    }
                    else
                    {
                        contentIdToCheck = contentItem.ParentId;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            ContentPermissionsResource resource = contentToCheck == null
                ? new ContentPermissionsResource(contentToCheck, contentIdToCheck, permissionToCheck)
                : new ContentPermissionsResource(contentToCheck, permissionToCheck);

            AuthorizationResult authorizationResult = await _authorizationService.AuthorizeAsync(
                actionContext.HttpContext.User,
                resource,
                AuthorizationPolicies.ContentPermissionByResource);

            if (!authorizationResult.Succeeded)
            {
                return false;
            }

            return true;
        }
    }
}
