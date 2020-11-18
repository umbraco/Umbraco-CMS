using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Core;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Editors;
using Umbraco.Web.Security;

namespace Umbraco.Web.BackOffice.Filters
{
    /// <summary>
    /// if the users being edited is an admin then we must ensure that the current user is also an admin
    /// </summary>
    /// <remarks>
    /// This will authorize against one or multiple ids
    /// </remarks>
    public sealed class AdminUsersAuthorizeAttribute : TypeFilterAttribute
    {

        public AdminUsersAuthorizeAttribute(string parameterName): base(typeof(AdminUsersAuthorizeFilter))
        {
            Arguments = new object[] { parameterName };
        }

        public AdminUsersAuthorizeAttribute() : this("id")
        {
        }

        private class AdminUsersAuthorizeFilter : IAuthorizationFilter
        {
            private readonly string _parameterName;
            private readonly IRequestAccessor _requestAccessor;
            private readonly IUserService _userService;
            private readonly IContentService _contentService;
            private readonly IMediaService _mediaService;
            private readonly IEntityService _entityService;
            private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;

            public AdminUsersAuthorizeFilter(
                IRequestAccessor requestAccessor,
                IUserService userService,
                IContentService contentService,
                IMediaService mediaService,
                IEntityService entityService,
                IBackOfficeSecurityAccessor backofficeSecurityAccessor,
                string parameterName)
            {
                _requestAccessor = requestAccessor;
                _userService = userService;
                _contentService = contentService;
                _mediaService = mediaService;
                _entityService = entityService;
                _backofficeSecurityAccessor = backofficeSecurityAccessor;
                _parameterName = parameterName;
            }

            public void OnAuthorization(AuthorizationFilterContext context)
            {
                if (!IsAuthorized(context))
                {
                    context.Result = new ForbidResult();
                }
            }

            private bool IsAuthorized(AuthorizationFilterContext actionContext)
            {
                int[] userIds;

                if (int.TryParse(_requestAccessor.GetRequestValue(_parameterName), out var userId))
                {
                    var intUserId = userId.TryConvertTo<int>();
                    if (intUserId)
                        userIds = new[] { intUserId.Result };
                    else return true;
                }
                else
                {
                    var queryString = actionContext.HttpContext.Request.Query;
                    var ids = queryString.Where(x => x.Key == _parameterName).ToArray();
                    if (ids.Length == 0)
                        return true;
                    userIds = ids.Select(x => x.Value.TryConvertTo<int>()).Where(x => x.Success).Select(x => x.Result).ToArray();
                }

                if (userIds.Length == 0) return true;

                var users = _userService.GetUsersById(userIds);
                var authHelper = new UserEditorAuthorizationHelper(_contentService, _mediaService, _userService, _entityService);
                return users.All(user => authHelper.IsAuthorized(_backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser, user, null, null, null) != false);
            }
        }


    }
}
