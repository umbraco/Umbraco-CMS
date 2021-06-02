using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Umbraco.Core;
using Umbraco.Core.Mapping;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Editors.Filters
{
    internal sealed class UserGroupValidateAttribute : ActionFilterAttribute
    {
        private readonly IUserService _userService;

        public UserGroupValidateAttribute()
        {
        }

        public UserGroupValidateAttribute(IUserService userService)
        {
            if (_userService == null) throw new ArgumentNullException(nameof(userService));
            _userService = userService;
        }

        private static UmbracoMapper Mapper => Current.Mapper;

        private IUserService UserService => _userService ?? Current.Services.UserService; // TODO: inject

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var userGroupSave = (UserGroupSave) actionContext.ActionArguments["userGroupSave"];

            userGroupSave.Name = userGroupSave.Name.CleanForXss('[', ']', '(', ')', ':');
            userGroupSave.Alias = userGroupSave.Alias.CleanForXss('[', ']', '(', ')', ':');

            //Validate the usergroup exists or create one if required
            IUserGroup persisted;
            switch (userGroupSave.Action)
            {
                case ContentSaveAction.Save:
                    persisted = UserService.GetUserGroupById(Convert.ToInt32(userGroupSave.Id));
                    if (persisted == null)
                    {
                        var message = $"User group with id: {userGroupSave.Id} was not found";
                        actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, message);
                        return;
                    }

                    if (persisted.Alias != userGroupSave.Alias && persisted.IsSystemUserGroup())
                    {
                        var message = $"User group with alias: {persisted.Alias} cannot be changed";
                        actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest, message);
                        return;
                    }

                    break;
                case ContentSaveAction.SaveNew:
                    persisted = new UserGroup();
                    break;
                default:
                    actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, new ArgumentOutOfRangeException());
                    return;
            }

            //now assign the persisted entity to the model so we can use it in the action
            userGroupSave.PersistedUserGroup = persisted;

            var existing = UserService.GetUserGroupByAlias(userGroupSave.Alias);
            if (existing != null && existing.Id != userGroupSave.PersistedUserGroup.Id)
            {
                actionContext.ModelState.AddModelError("Alias", "A user group with this alias already exists");
            }

            // TODO: Validate the name is unique?

            if (actionContext.ModelState.IsValid == false)
            {
                //if it is not valid, do not continue and return the model state
                actionContext.Response = actionContext.Request.CreateValidationErrorResponse(actionContext.ModelState);
            }
        }
    }
}
