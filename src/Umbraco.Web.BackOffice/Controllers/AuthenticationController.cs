using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.BackOffice;
using Umbraco.Core.Configuration;
using Umbraco.Core.Mapping;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Extensions;
using Umbraco.Web.BackOffice.Security;
using Umbraco.Web.Common.Attributes;
using Umbraco.Web.Common.Controllers;
using Umbraco.Web.Common.Exceptions;
using Umbraco.Web.Common.Filters;
using Umbraco.Web.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Security;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.BackOffice.Controllers
{
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]  // TODO: Maybe this could be applied with our Application Model conventions
    //[ValidationFilter] // TODO: I don't actually think this is required with our custom Application Model conventions applied
    [TypeFilter(typeof(AngularJsonOnlyConfigurationAttribute))] // TODO: This could be applied with our Application Model conventions
    [IsBackOffice] // TODO: This could be applied with our Application Model conventions
    public class AuthenticationController : UmbracoApiControllerBase
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly BackOfficeUserManager _userManager;
        private readonly BackOfficeSignInManager _signInManager;
        private readonly IUserService _userService;
        private readonly UmbracoMapper _umbracoMapper;
        private readonly IGlobalSettings _globalSettings;

        // TODO: We need to import the logic from Umbraco.Web.Editors.AuthenticationController

        public AuthenticationController(
            IUmbracoContextAccessor umbracoContextAccessor,
            BackOfficeUserManager backOfficeUserManager,
            BackOfficeSignInManager signInManager,
            IUserService userService,
            UmbracoMapper umbracoMapper,
            IGlobalSettings globalSettings)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
            _userManager = backOfficeUserManager;
            _signInManager = signInManager;
            _userService = userService;
            _umbracoMapper = umbracoMapper;
            _globalSettings = globalSettings;
        }

        /// <summary>
        /// Checks if the current user's cookie is valid and if so returns OK or a 400 (BadRequest)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public bool IsAuthenticated()
        {
            var umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();
            var attempt = umbracoContext.Security.AuthorizeRequest();
            if (attempt == ValidateRequestAttempt.Success)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Logs a user in
        /// </summary>
        /// <returns></returns>
        [TypeFilter(typeof(SetAngularAntiForgeryTokens))]
        public async Task<UserDetail> PostLogin(LoginModel loginModel)
        {
            // Sign the user in with username/password, this also gives a chance for developers to
            // custom verify the credentials and auto-link user accounts with a custom IBackOfficePasswordChecker
            var result = await _signInManager.PasswordSignInAsync(
                loginModel.Username, loginModel.Password, isPersistent: true, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                // get the user
                var user = _userService.GetByUsername(loginModel.Username);
                _userManager.RaiseLoginSuccessEvent(User, user.Id);

                // TODO: This is not correct, no user will be set :/ 
                return SetPrincipalAndReturnUserDetail(user, HttpContext.User);
            }

            if (result.RequiresTwoFactor)
            {
                throw new NotImplementedException("Implement MFA/2FA, we need to have some IOptions or similar to configure this");

                //var twofactorOptions = UserManager as IUmbracoBackOfficeTwoFactorOptions;
                //if (twofactorOptions == null)
                //{
                //    throw new HttpResponseException(
                //        Request.CreateErrorResponse(
                //            HttpStatusCode.BadRequest,
                //            "UserManager does not implement " + typeof(IUmbracoBackOfficeTwoFactorOptions)));
                //}

                //var twofactorView = twofactorOptions.GetTwoFactorView(
                //    owinContext,
                //    UmbracoContext,
                //    loginModel.Username);

                //if (twofactorView.IsNullOrWhiteSpace())
                //{
                //    throw new HttpResponseException(
                //        Request.CreateErrorResponse(
                //            HttpStatusCode.BadRequest,
                //            typeof(IUmbracoBackOfficeTwoFactorOptions) + ".GetTwoFactorView returned an empty string"));
                //}

                //var attemptedUser = Services.UserService.GetByUsername(loginModel.Username);

                //// create a with information to display a custom two factor send code view
                //var verifyResponse = Request.CreateResponse(HttpStatusCode.PaymentRequired, new
                //{
                //    twoFactorView = twofactorView,
                //    userId = attemptedUser.Id
                //});

                //_userManager.RaiseLoginRequiresVerificationEvent(User, attemptedUser.Id);

                //return verifyResponse;
            }

            // return BadRequest (400), we don't want to return a 401 because that get's intercepted
            // by our angular helper because it thinks that we need to re-perform the request once we are
            // authorized and we don't want to return a 403 because angular will show a warning message indicating
            // that the user doesn't have access to perform this function, we just want to return a normal invalid message.
            throw new HttpResponseException(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// This is used when the user is auth'd successfully and we need to return an OK with user details along with setting the current Principal in the request
        /// </summary>
        /// <param name="user"></param>
        /// <param name="principal"></param>
        /// <returns></returns>
        private UserDetail SetPrincipalAndReturnUserDetail(IUser user, ClaimsPrincipal principal)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (principal == null) throw new ArgumentNullException(nameof(principal));

            var userDetail = _umbracoMapper.Map<UserDetail>(user);
            // update the userDetail and set their remaining seconds
            userDetail.SecondsUntilTimeout = TimeSpan.FromMinutes(_globalSettings.TimeOutInMinutes).TotalSeconds;

            // ensure the user is set for the current request
            HttpContext.SetPrincipalForRequest(principal);

            return userDetail;
        }
    }
}
