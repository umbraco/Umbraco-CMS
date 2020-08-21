using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Core.Logging;
using Umbraco.Core.Mapping;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;
using IUser = Umbraco.Core.Models.Membership.IUser;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// The API controller used for editing content
    /// </summary>
    [PluginController(Constants.Web.Mvc.BackOfficeApiArea)]
    [ValidationFilter]
    [AngularJsonOnlyConfiguration]
    [IsBackOffice]
    public class AuthenticationController : UmbracoApiController
    {
        private BackOfficeOwinUserManager _userManager;
        private BackOfficeSignInManager _signInManager;
        private readonly UserPasswordConfigurationSettings _passwordConfiguration;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IRuntimeState _runtimeState;
        private readonly SecuritySettings _securitySettings;
        private readonly IRequestAccessor _requestAccessor;
        private readonly IEmailSender _emailSender;

        public AuthenticationController(
            IOptionsSnapshot<UserPasswordConfigurationSettings> passwordConfiguration,
            IOptionsSnapshot<GlobalSettings> globalSettings,
            IHostingEnvironment hostingEnvironment,
            IUmbracoContextAccessor umbracoContextAccessor,
            ISqlContext sqlContext,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger logger,
            IRuntimeState runtimeState,
            UmbracoMapper umbracoMapper,
            IOptionsSnapshot<SecuritySettings> securitySettings,
            IPublishedUrlProvider publishedUrlProvider,
            IRequestAccessor requestAccessor,
            IEmailSender emailSender)
            : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, logger, runtimeState, umbracoMapper, publishedUrlProvider)
        {
            _passwordConfiguration = passwordConfiguration.Value ?? throw new ArgumentNullException(nameof(passwordConfiguration));
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
            _runtimeState = runtimeState ?? throw new ArgumentNullException(nameof(runtimeState));
            _securitySettings = securitySettings.Value ?? throw new ArgumentNullException(nameof(securitySettings));
            _requestAccessor = requestAccessor ?? throw new ArgumentNullException(nameof(securitySettings));
            _emailSender = emailSender;
        }

        protected BackOfficeOwinUserManager UserManager => _userManager
                                                           ?? (_userManager = TryGetOwinContext().Result.GetBackOfficeUserManager());

        protected BackOfficeSignInManager SignInManager => _signInManager
            ?? (_signInManager = TryGetOwinContext().Result.GetBackOfficeSignInManager());


        [WebApi.UmbracoAuthorize]
        [ValidateAngularAntiForgeryToken]
        public async Task<HttpResponseMessage> PostUnLinkLogin(UnLinkLoginModel unlinkLoginModel)
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null) throw new InvalidOperationException("Could not find user");

            var result = await UserManager.RemoveLoginAsync(
                user,
                unlinkLoginModel.LoginProvider,
                unlinkLoginModel.ProviderKey);

            if (result.Succeeded)
            {
                await SignInManager.SignInAsync(user, isPersistent: true, rememberBrowser: false);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                AddModelErrors(result);
                return Request.CreateValidationErrorResponse(ModelState);
            }
        }


        // TODO: This should be on the CurrentUserController?
        [WebApi.UmbracoAuthorize]
        [ValidateAngularAntiForgeryToken]
        public async Task<Dictionary<string, string>> GetCurrentUserLinkedLogins()
        {
            var identityUser = await UserManager.FindByIdAsync(Security.GetUserId().ResultOr(0).ToString());
            return identityUser.Logins.ToDictionary(x => x.LoginProvider, x => x.ProviderKey);
        }

        /// <summary>
        /// Used to retrieve the 2FA providers for code submission
        /// </summary>
        /// <returns></returns>
        [SetAngularAntiForgeryTokens]
        public async Task<IEnumerable<string>> Get2FAProviders()
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (string.IsNullOrWhiteSpace(userId))
            {
                Logger.Warn<AuthenticationController>("Get2FAProviders :: No verified user found, returning 404");
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var user = await UserManager.FindByIdAsync(userId);
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(user);

            return userFactors;
        }

        [SetAngularAntiForgeryTokens]
        public async Task<IHttpActionResult> PostSend2FACode([FromBody]string provider)
        {
            if (provider.IsNullOrWhiteSpace())
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (string.IsNullOrWhiteSpace(userId))
            {
                Logger.Warn<AuthenticationController>("Get2FAProviders :: No verified user found, returning 404");
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            // Generate the token and send it
            if (await SignInManager.SendTwoFactorCodeAsync(provider) == false)
            {
                return BadRequest("Invalid code");
            }
            return Ok();
        }

        [SetAngularAntiForgeryTokens]
        public async Task<HttpResponseMessage> PostVerify2FACode(Verify2FACodeModel model)
        {
            if (ModelState.IsValid == false)
            {
                return Request.CreateValidationErrorResponse(ModelState);
            }

            var userName = await SignInManager.GetVerifiedUserNameAsync();
            if (userName == null)
            {
                Logger.Warn<AuthenticationController>("Get2FAProviders :: No verified user found, returning 404");
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: true, rememberBrowser: false);
            var owinContext = TryGetOwinContext().Result;

            var user = Services.UserService.GetByUsername(userName);
            if (result.Succeeded)
            {
                return SetPrincipalAndReturnUserDetail(user, owinContext.Request.User);
            }

            if (result.IsLockedOut)
            {
                UserManager.RaiseAccountLockedEvent(User, user.Id);
                return Request.CreateValidationErrorResponse("User is locked out");
            }

            return Request.CreateValidationErrorResponse("Invalid code");
        }

        // NOTE: This has been migrated to netcore, but in netcore we don't explicitly set the principal in this method, that's done in ConfigureUmbracoBackOfficeCookieOptions so don't worry about that
        private HttpResponseMessage SetPrincipalAndReturnUserDetail(IUser user, IPrincipal principal)
        {
            throw new NotImplementedException();
        }

        private void AddModelErrors(IdentityResult result, string prefix = "")
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(prefix, error.Description);
            }
        }
    }
}
