using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.BackOffice.Controllers;

namespace Umbraco.Cms.Web.BackOffice.Filters;

internal class IsCurrentUserModelFilterAttribute : TypeFilterAttribute
{
    public IsCurrentUserModelFilterAttribute() : base(typeof(IsCurrentUserModelFilter))
    {
    }

    private class IsCurrentUserModelFilter : IActionFilter
    {
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;

        public IsCurrentUserModelFilter(IBackOfficeSecurityAccessor backofficeSecurityAccessor) =>
            _backofficeSecurityAccessor = backofficeSecurityAccessor;


        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Result == null)
            {
                return;
            }

            IUser? user = _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
            if (user == null)
            {
                return;
            }

            if (context.Result is ObjectResult objectContent)
            {
                if (objectContent.Value is UserBasic model)
                {
                    model.IsCurrentUser = (int?)model.Id == user.Id;
                }
                else
                {
                    if (objectContent.Value is IEnumerable<UserBasic> collection)
                    {
                        foreach (UserBasic userBasic in collection)
                        {
                            userBasic.IsCurrentUser = (int?)userBasic.Id == user.Id;
                        }
                    }
                    else
                    {
                        if (objectContent.Value is UsersController.PagedUserResult paged && paged.Items != null)
                        {
                            foreach (UserBasic userBasic in paged.Items)
                            {
                                userBasic.IsCurrentUser = (int?)userBasic.Id == user.Id;
                            }
                        }
                    }
                }
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
        }
    }
}
