using System;
using System.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Web.Common.Security
{
    // TODO: This is only for the back office, does it need to be in common?

    public class BackOfficeSecurity : IBackOfficeSecurity
    {
        private readonly IUserService _userService;
        private readonly GlobalSettings _globalSettings;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BackOfficeSecurity(
            IUserService userService,
            IOptions<GlobalSettings> globalSettings,
            IHostingEnvironment hostingEnvironment,
            IHttpContextAccessor httpContextAccessor)
        {
            _userService = userService;
            _globalSettings = globalSettings.Value;
            _hostingEnvironment = hostingEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        private IUser _currentUser;

        /// <inheritdoc />
        public IUser CurrentUser
        {
            get
            {
                //only load it once per instance! (but make sure groups are loaded)
                if (_currentUser == null)
                {
                    var id = GetUserId();
                    _currentUser = id ? _userService.GetUserById(id.Result) : null;
                }

                return _currentUser;
            }
        }

        /// <inheritdoc />
        public Attempt<int> GetUserId()
        {
            var identity = _httpContextAccessor.HttpContext?.GetCurrentIdentity();
            return identity == null ? Attempt.Fail<int>() : Attempt.Succeed(identity.Id);
        }

        /// <inheritdoc />
        public bool IsAuthenticated()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            return httpContext?.User != null && httpContext.User.Identity.IsAuthenticated && httpContext.GetCurrentIdentity() != null;
        }

        /// <inheritdoc />
        public bool UserHasSectionAccess(string section, IUser user)
        {
            return user.HasSectionAccess(section);
        }

    }
}
