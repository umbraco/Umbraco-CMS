using System;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Used for authorizing scheduled tasks
    /// </summary>
    public sealed class AdminTokenAuthorizeAttribute : AuthorizeAttribute
    {
        // see note in HttpInstallAuthorizeAttribute
        private readonly IUserService _userService;
        private readonly IRuntimeState _runtimeState;
        private readonly ILogger _logger;

        private IUserService UserService => _userService ?? Current.Services.UserService;

        private IRuntimeState RuntimeState => _runtimeState ?? Current.RuntimeState;

        private ILogger Logger => _logger ?? Current.Logger;

        /// <summary>
        /// THIS SHOULD BE ONLY USED FOR UNIT TESTS
        /// </summary>
        /// <param name="userService"></param>
        /// <param name="runtimeState"></param>
        public AdminTokenAuthorizeAttribute(IUserService userService, IRuntimeState runtimeState)
        {
            if (userService == null) throw new ArgumentNullException(nameof(userService));
            if (runtimeState == null) throw new ArgumentNullException(nameof(runtimeState));
            _userService = userService;
            _runtimeState = runtimeState;
        }

        public AdminTokenAuthorizeAttribute()
        { }

        public const string AuthorizationType = "AToken";

        /// <summary>
        /// Used to return the full value that needs to go in the Authorization header
        /// </summary>
        /// <param name="userService"></param>
        /// <returns></returns>
        public static string GetAuthHeaderTokenVal(IUserService userService)
        {
            return $"{AuthorizationType} {GetAuthHeaderVal(userService)}";
        }

        public static AuthenticationHeaderValue GetAuthenticationHeaderValue(IUserService userService)
        {
            return new AuthenticationHeaderValue(AuthorizationType, GetAuthHeaderVal(userService));
        }

        private static string GetAuthHeaderVal(IUserService userService)
        {
            var admin = userService.GetUserById(Core.Constants.Security.SuperUserId);

            var token = $"{admin.Email}u____u{admin.Username}u____u{admin.RawPasswordValue}";

            var encrypted = token.EncryptWithMachineKey();
            var bytes = Encoding.UTF8.GetBytes(encrypted);
            var base64 = Convert.ToBase64String(bytes);
            return $"val=\"{base64}\"";
        }

        /// <summary>
        /// Ensures that the user must be in the Administrator or the Install role
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            // we need to that the app is configured and that a user is logged in
            if (RuntimeState.Level != RuntimeLevel.Run) return false;

            // need the auth header
            if (httpContext.Request.Headers["Authorization"] == null || httpContext.Request.Headers["Authorization"].IsNullOrWhiteSpace()) return false;

            var header = httpContext.Request.Headers["Authorization"];
            if (header.StartsWith("AToken ") == false) return false;

            var keyVal = Regex.Matches(header, "AToken val=(.*?)(?:$|\\s)");
            if (keyVal.Count != 1) return false;
            if (keyVal[0].Groups.Count != 2) return false;

            var admin = UserService.GetUserById(Core.Constants.Security.SuperUserId);
            if (admin == null) return false;

            try
            {
                //get the token value from the header
                var val = keyVal[0].Groups[1].Value.Trim("\"");
                //un-base 64 the string
                var bytes = Convert.FromBase64String(val);
                var encrypted = Encoding.UTF8.GetString(bytes);
                //decrypt the string
                var text = encrypted.DecryptWithMachineKey();

                //split - some users have not set an email, don't strip out empty entries
                var split = text.Split(new[] {"u____u"}, StringSplitOptions.None);
                if (split.Length != 3) return false;

                //compare
                return
                    split[0] == admin.Email
                    && split[1] == admin.Username
                    && split[2] == admin.RawPasswordValue;
            }
            catch (Exception ex)
            {
                Logger.Error<AdminTokenAuthorizeAttribute>(ex, "Failed to format passed in token value");
                return false;
            }
        }
    }
}
