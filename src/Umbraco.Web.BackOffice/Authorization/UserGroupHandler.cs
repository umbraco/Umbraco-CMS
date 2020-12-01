using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.BackOffice.Controllers;

namespace Umbraco.Web.BackOffice.Authorization
{
    /// <summary>
    /// Authorizes that the current user has access to the user group Id in the request
    /// </summary>
    public class UserGroupHandler : MustSatisfyRequirementAuthorizationHandler<UserGroupRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAcessor;
        private readonly IUserService _userService;
        private readonly IContentService _contentService;
        private readonly IMediaService _mediaService;
        private readonly IEntityService _entityService;
        private readonly IBackOfficeSecurityAccessor _backofficeSecurityAccessor;

        public UserGroupHandler(IHttpContextAccessor httpContextAcessor,
                IUserService userService,
                IContentService contentService,
                IMediaService mediaService,
                IEntityService entityService,
                IBackOfficeSecurityAccessor backofficeSecurityAccessor)
        {
            _httpContextAcessor = httpContextAcessor;
            _userService = userService;
            _contentService = contentService;
            _mediaService = mediaService;
            _entityService = entityService;
            _backofficeSecurityAccessor = backofficeSecurityAccessor;
        }

        protected override Task<bool> IsAuthorized(AuthorizationHandlerContext context, UserGroupRequirement requirement)
        {
            var currentUser = _backofficeSecurityAccessor.BackOfficeSecurity.CurrentUser;

            var queryString = _httpContextAcessor.HttpContext?.Request.Query;
            if (queryString == null)
            {
                // must succeed this requirement since we cannot process it
                return Task.FromResult(true);
            }   

            var ids = queryString.Where(x => x.Key == requirement.QueryStringName).ToArray();
            if (ids.Length == 0)
            {
                // must succeed this requirement since we cannot process it
                return Task.FromResult(true);
            }

            var intIds = ids.Select(x => x.Value.TryConvertTo<int>()).Where(x => x.Success).Select(x => x.Result).ToArray();
            var authHelper = new UserGroupEditorAuthorizationHelper(
                _userService,
                _contentService,
                _mediaService,
                _entityService);

            var isAuth = authHelper.AuthorizeGroupAccess(currentUser, intIds);

            return Task.FromResult(isAuth.Success);
        }

    }
}
