using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using Umbraco.Core;
using Umbraco.Web.Editors;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// if the user being edited is an admin then we must ensure that the current user is also an admin
    /// </summary>
    public sealed class AdminUsersAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            if (actionContext.ActionArguments.TryGetValue("id", out var userId) == false)
            {
                var queryString = actionContext.Request.GetQueryNameValuePairs();
                var ids = queryString.Where(x => x.Key == "id").ToArray();
                if (ids.Length == 0)
                    return base.IsAuthorized(actionContext);
                userId = ids[0].Value;
            }

            if (userId == null) return base.IsAuthorized(actionContext);
            var intUserId = userId.TryConvertTo<int>();
            if (intUserId.Success == false)
                return base.IsAuthorized(actionContext);

            var user = ApplicationContext.Current.Services.UserService.GetUserById(intUserId.Result);
            if (user == null)
                return base.IsAuthorized(actionContext);

            //Perform authorization here to see if the current user can actually save this user with the info being requested
            var authHelper = new UserEditorAuthorizationHelper(ApplicationContext.Current.Services.ContentService, ApplicationContext.Current.Services.MediaService, ApplicationContext.Current.Services.UserService, ApplicationContext.Current.Services.EntityService);
            var canSaveUser = authHelper.IsAuthorized(UmbracoContext.Current.Security.CurrentUser, user, null, null, null);
            return canSaveUser;
        }
    }
}
