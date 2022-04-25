using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Security
{
    public class BackOfficeSecurity : IBackOfficeSecurity
    {
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly object _currentUserLock = new object();
        private IUser? _currentUser;

        public BackOfficeSecurity(
            IUserService userService,
            IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <inheritdoc />
        public IUser? CurrentUser
        {
            get
            {

                //only load it once per instance! (but make sure groups are loaded)
                if (_currentUser == null)
                {
                    lock (_currentUserLock)
                    {
                        //Check again
                        if (_currentUser == null)
                        {
                            Attempt<int?> id = GetUserId();
                            if (id.Success && id.Result is not null)
                            {
                                _currentUser = id.Success ? _userService.GetUserById(id.Result.Value) : null;
                            }

                        }
                    }
                }

                return _currentUser;
            }
        }

        /// <inheritdoc />
        public Attempt<int?> GetUserId()
        {
            ClaimsIdentity? identity = _httpContextAccessor.HttpContext?.GetCurrentIdentity();
            return identity == null ? Attempt.Fail<int?>() : Attempt.Succeed(identity.GetId());
        }

        /// <inheritdoc />
        public bool IsAuthenticated()
        {
            HttpContext? httpContext = _httpContextAccessor.HttpContext;
            return httpContext?.User != null && (httpContext.User.Identity?.IsAuthenticated ?? false) && httpContext.GetCurrentIdentity() != null;
        }

        /// <inheritdoc />
        public bool UserHasSectionAccess(string section, IUser user) => user.HasSectionAccess(section);
    }
}
