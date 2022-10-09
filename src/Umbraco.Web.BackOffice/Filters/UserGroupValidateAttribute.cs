using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.BackOffice.ActionResults;
using Umbraco.Cms.Web.Common.ActionsResults;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Filters;

internal sealed class UserGroupValidateAttribute : TypeFilterAttribute
{
    public UserGroupValidateAttribute() : base(typeof(UserGroupValidateFilter))
    {
    }

    private class UserGroupValidateFilter : IActionFilter
    {
        private readonly IShortStringHelper _shortStringHelper;
        private readonly IUserService _userService;

        public UserGroupValidateFilter(
            IUserService userService,
            IShortStringHelper shortStringHelper)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _shortStringHelper = shortStringHelper ?? throw new ArgumentNullException(nameof(shortStringHelper));
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var userGroupSave = (UserGroupSave?)context.ActionArguments["userGroupSave"];

            if (userGroupSave is not null)
            {
                userGroupSave.Name = userGroupSave.Name?.CleanForXss('[', ']', '(', ')', ':');
                userGroupSave.Alias = userGroupSave.Alias.CleanForXss('[', ']', '(', ')', ':');
            }

            //Validate the usergroup exists or create one if required
            IUserGroup? persisted;
            switch (userGroupSave?.Action)
            {
                case ContentSaveAction.Save:
                    persisted = _userService.GetUserGroupById(Convert.ToInt32(userGroupSave.Id));
                    if (persisted == null)
                    {
                        var message = $"User group with id: {userGroupSave.Id} was not found";
                        context.Result = new UmbracoErrorResult(HttpStatusCode.NotFound, message);
                        return;
                    }

                    if (persisted.Alias != userGroupSave.Alias && persisted.IsSystemUserGroup())
                    {
                        var message = $"User group with alias: {persisted.Alias} cannot be changed";
                        context.Result = new UmbracoErrorResult(HttpStatusCode.BadRequest, message);
                        return;
                    }

                    break;
                case ContentSaveAction.SaveNew:
                    persisted = new UserGroup(_shortStringHelper);
                    break;
                default:
                    context.Result =
                        new UmbracoErrorResult(HttpStatusCode.NotFound, new ArgumentOutOfRangeException());
                    return;
            }

            //now assign the persisted entity to the model so we can use it in the action
            userGroupSave.PersistedUserGroup = persisted;

            IUserGroup? existing = _userService.GetUserGroupByAlias(userGroupSave.Alias);
            if (existing != null && existing.Id != userGroupSave.PersistedUserGroup.Id)
            {
                context.ModelState.AddModelError("Alias", "A user group with this alias already exists");
            }

            // TODO: Validate the name is unique?

            if (context.ModelState.IsValid == false)
            {
                //if it is not valid, do not continue and return the model state
                context.Result = new ValidationErrorResult(context.ModelState);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
