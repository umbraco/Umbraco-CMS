using System;
using System.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Extensions;
using Umbraco.Web.Security;

namespace Umbraco.Web.Common.Security
{

    public class BackofficeSecurity : IBackOfficeSecurity
    {
        private readonly IUserService _userService;
        private readonly GlobalSettings _globalSettings;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BackofficeSecurity(
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
        public ValidateRequestAttempt AuthorizeRequest(bool throwExceptions = false)
        {
            // check for secure connection
            if (_globalSettings.UseHttps && !_httpContextAccessor.GetRequiredHttpContext().Request.IsHttps)
            {
                if (throwExceptions) throw new SecurityException("This installation requires a secure connection (via SSL). Please update the URL to include https://");
                return ValidateRequestAttempt.FailedNoSsl;
            }
            return ValidateCurrentUser(throwExceptions);
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

        /// <inheritdoc />
        public bool ValidateCurrentUser()
        {
            return ValidateCurrentUser(false, true) == ValidateRequestAttempt.Success;
        }

        /// <inheritdoc />
        public ValidateRequestAttempt ValidateCurrentUser(bool throwExceptions, bool requiresApproval = true)
        {
            //This will first check if the current user is already authenticated - which should be the case in nearly all circumstances
            // since the authentication happens in the Module, that authentication also checks the ticket expiry. We don't
            // need to check it a second time because that requires another decryption phase and nothing can tamper with it during the request.

            if (IsAuthenticated() == false)
            {
                //There is no user
                if (throwExceptions) throw new InvalidOperationException("The user has no umbraco contextid - try logging in");
                return ValidateRequestAttempt.FailedNoContextId;
            }

            var user = CurrentUser;

            // Check for console access
            if (user == null || (requiresApproval && user.IsApproved == false) || (user.IsLockedOut && RequestIsInUmbracoApplication(_httpContextAccessor, _globalSettings, _hostingEnvironment)))
            {
                if (throwExceptions) throw new ArgumentException("You have no privileges to the umbraco console. Please contact your administrator");
                return ValidateRequestAttempt.FailedNoPrivileges;
            }
            return ValidateRequestAttempt.Success;
        }

        private static bool RequestIsInUmbracoApplication(IHttpContextAccessor httpContextAccessor, GlobalSettings globalSettings, IHostingEnvironment hostingEnvironment)
        {
            return httpContextAccessor.GetRequiredHttpContext().Request.Path.ToString().IndexOf(hostingEnvironment.ToAbsolute(globalSettings.UmbracoPath), StringComparison.InvariantCultureIgnoreCase) > -1;
        }
    }
}
