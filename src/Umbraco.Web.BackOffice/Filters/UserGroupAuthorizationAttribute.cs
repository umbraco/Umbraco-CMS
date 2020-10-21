using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Core;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.BackOffice.Controllers;
using Umbraco.Web.Security;

namespace Umbraco.Web.BackOffice.Filters
{
    /// <summary>
    /// Authorizes that the current user has access to the user group Id in the request
    /// </summary>
    /// <remarks>
    /// This will authorize against one or multiple ids
    /// </remarks>
    public sealed class UserGroupAuthorizationAttribute : TypeFilterAttribute
    {

        public UserGroupAuthorizationAttribute(string parameterName): base(typeof(UserGroupAuthorizationFilter))
        {
            Arguments = new object[] { parameterName };
        }

        public UserGroupAuthorizationAttribute() : this("id")
        {
        }

        private class UserGroupAuthorizationFilter : IAuthorizationFilter
        {
            private readonly string _parameterName;
            private readonly IRequestAccessor _requestAccessor;
            private readonly IUserService _userService;
            private readonly IContentService _contentService;
            private readonly IMediaService _mediaService;
            private readonly IEntityService _entityService;
            private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;

            public UserGroupAuthorizationFilter(
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
                var currentUser = _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser;

                var queryString = actionContext.HttpContext.Request.Query;

                var ids = queryString.Where(x => x.Key == _parameterName).ToArray();
                if (ids.Length == 0)
                    return true;

                var intIds = ids.Select(x => x.Value.TryConvertTo<int>()).Where(x => x.Success).Select(x => x.Result).ToArray();
                var authHelper = new UserGroupEditorAuthorizationHelper(
                    _userService,
                    _contentService,
                    _mediaService,
                    _entityService);
                return authHelper.AuthorizeGroupAccess(currentUser, intIds);

            }
        }
    }
}
