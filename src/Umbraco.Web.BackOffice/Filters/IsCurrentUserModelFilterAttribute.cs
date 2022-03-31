using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.BackOffice.Controllers;

namespace Umbraco.Cms.Web.BackOffice.Filters
{
    internal class IsCurrentUserModelFilterAttribute : TypeFilterAttribute
    {
        public IsCurrentUserModelFilterAttribute() : base(typeof(IsCurrentUserModelFilter))
        {
        }

        private class IsCurrentUserModelFilter : IActionFilter
        {
            private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;

            public IsCurrentUserModelFilter(IBackOfficeSecurityAccessor backofficeSecurityAccessor)
            {
                _backofficeSecurityAccessor = backofficeSecurityAccessor;
            }


            public void OnActionExecuted(ActionExecutedContext context)
            {
                if (context.Result == null) return;

                var user = _backofficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
                if (user == null) return;

                var objectContent = context.Result as ObjectResult;
                if (objectContent != null)
                {
                    var model = objectContent.Value as UserBasic;
                    if (model != null)
                    {
                        model.IsCurrentUser = (int?) model.Id == user.Id;
                    }
                    else
                    {
                        var collection = objectContent.Value as IEnumerable<UserBasic>;
                        if (collection != null)
                        {
                            foreach (var userBasic in collection)
                            {
                                userBasic.IsCurrentUser = (int?) userBasic.Id == user.Id;
                            }
                        }
                        else
                        {
                            var paged = objectContent.Value as UsersController.PagedUserResult;
                            if (paged != null && paged.Items != null)
                            {
                                foreach (var userBasic in paged.Items)
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
}
