using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using Umbraco.Core;
using Umbraco.Web.Composing;
using Umbraco.Web.Editors;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// if the users being edited is an admin then we must ensure that the current user is also an admin
    /// </summary>
    /// <remarks>
    /// This will authorize against one or multiple ids
    /// </remarks>
    public sealed class AdminUsersAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly string _parameterName;

        public AdminUsersAuthorizeAttribute(string parameterName)
        {
            _parameterName = parameterName;
        }

        public AdminUsersAuthorizeAttribute() : this("id")
        {            
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            int[] userIds;
            if (actionContext.ActionArguments.TryGetValue(_parameterName, out var userId))
            {
                var intUserId = userId.TryConvertTo<int>();
                if (intUserId)
                    userIds = new[] {intUserId.Result};
                else return base.IsAuthorized(actionContext);
            }
            else
            {
                var queryString = actionContext.Request.GetQueryNameValuePairs();
                var ids = queryString.Where(x => x.Key == _parameterName).ToArray();
                if (ids.Length == 0)
                    return base.IsAuthorized(actionContext);
                userIds = ids.Select(x => x.Value.TryConvertTo<int>()).Where(x => x.Success).Select(x => x.Result).ToArray();
            }

            if (userIds.Length == 0) return base.IsAuthorized(actionContext);

            var users = Current.Services.UserService.GetUsersById(userIds);
            var authHelper = new UserEditorAuthorizationHelper(Current.Services.ContentService, Current.Services.MediaService, Current.Services.UserService, Current.Services.EntityService, Current.AppCaches);
            return users.All(user => authHelper.IsAuthorized(Current.UmbracoContext.Security.CurrentUser, user, null, null, null) != false);
        }
    }
}
