using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Filters;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Editors.Filters
{
    /// <summary>
    /// This sets the IsCurrentUser property on any outgoing <see cref="UserDisplay"/> model or any collection of <see cref="UserDisplay"/> models
    /// </summary>
    internal class IsCurrentUserModelFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Response == null) return;

            var user = Current.UmbracoContext.Security.CurrentUser;
            if (user == null) return;

            var objectContent = actionExecutedContext.Response.Content as ObjectContent;
            if (objectContent != null)
            {
                var model = objectContent.Value as UserBasic;
                if (model != null && model.Id is int userId)
                {
                    model.IsCurrentUser = userId == user.Id;
                }
                else
                {
                    var collection = objectContent.Value as IEnumerable<UserBasic>;
                    if (collection != null)
                    {
                        foreach (var userBasic in collection)
                        {
                            if (userBasic.Id is int uid)
                            {
                                userBasic.IsCurrentUser = uid == user.Id;
                            }
                        }
                    }
                    else
                    {
                        var paged = objectContent.Value as UsersController.PagedUserResult;
                        if (paged != null && paged.Items != null)
                        {
                            foreach (var userBasic in paged.Items)
                            {
                                if (userBasic.Id is int uid)
                                {
                                    userBasic.IsCurrentUser = uid == user.Id;
                                }
                            }
                        }
                    }
                }
            }

            base.OnActionExecuted(actionExecutedContext);
        }
    }
}
