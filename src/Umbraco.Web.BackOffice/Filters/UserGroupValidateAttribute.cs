using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.BackOffice.ActionResults;
using Umbraco.Extensions;
using Umbraco.Web.Common.ActionsResults;

namespace Umbraco.Cms.Web.BackOffice.Filters
{
    internal sealed class UserGroupValidateAttribute : TypeFilterAttribute
    {
        public UserGroupValidateAttribute() : base(typeof(UserGroupValidateFilter))
        {
        }

        private class UserGroupValidateFilter : IActionFilter
        {
            private readonly UmbracoMapper _umbracoMapper;
            private readonly IUserService _userService;

            public UserGroupValidateFilter(
                IUserService userService,
                UmbracoMapper umbracoMapper)
            {
                _userService = userService ?? throw new ArgumentNullException(nameof(userService));
                _umbracoMapper = umbracoMapper ?? throw new ArgumentNullException(nameof(umbracoMapper));
            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                var userGroupSave = (UserGroupSave) context.ActionArguments["userGroupSave"];

                userGroupSave.Name = userGroupSave.Name.CleanForXss('[', ']', '(', ')', ':');
                userGroupSave.Alias = userGroupSave.Alias.CleanForXss('[', ']', '(', ')', ':');

                //Validate the usergroup exists or create one if required
                IUserGroup persisted;
                switch (userGroupSave.Action)
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

                        //map the model to the persisted instance
                        _umbracoMapper.Map(userGroupSave, persisted);
                        break;
                    case ContentSaveAction.SaveNew:
                        //create the persisted model from mapping the saved model
                        persisted = _umbracoMapper.Map<IUserGroup>(userGroupSave);
                        ((UserGroup) persisted).ResetIdentity();
                        break;
                    default:
                        context.Result =
                            new UmbracoErrorResult(HttpStatusCode.NotFound, new ArgumentOutOfRangeException());
                        return;
                }

                //now assign the persisted entity to the model so we can use it in the action
                userGroupSave.PersistedUserGroup = persisted;

                var existing = _userService.GetUserGroupByAlias(userGroupSave.Alias);
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
}
