using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Net.Mail;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Models;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Mvc;
using Umbraco.Web.Security;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using IUser = Umbraco.Core.Models.Membership.IUser;
using Umbraco.Core.Mapping;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Hosting;
using Umbraco.Extensions;
using Umbraco.Web.Routing;
using Constants = Umbraco.Core.Constants;

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
        private readonly IUserPasswordConfiguration _passwordConfiguration;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IRuntimeState _runtimeState;
        private readonly SecuritySettings _securitySettings;
        private readonly IRequestAccessor _requestAccessor;
        private readonly IEmailSender _emailSender;

        public AuthenticationController(
            IUserPasswordConfiguration passwordConfiguration,
            GlobalSettings globalSettings,
            IHostingEnvironment hostingEnvironment,
            IUmbracoContextAccessor umbracoContextAccessor,
            ISqlContext sqlContext,
            ServiceContext services,
            AppCaches appCaches,
            IProfilingLogger profilingLogger ,
            ILogger<AuthenticationController> logger,
            IRuntimeState runtimeState,
            UmbracoMapper umbracoMapper,
            IOptions<SecuritySettings> securitySettings,
            IPublishedUrlProvider publishedUrlProvider,
            IRequestAccessor requestAccessor,
            IEmailSender emailSender)
            : base(globalSettings, umbracoContextAccessor, sqlContext, services, appCaches, profilingLogger , runtimeState, umbracoMapper, publishedUrlProvider)
        {
            _passwordConfiguration = passwordConfiguration ?? throw new ArgumentNullException(nameof(passwordConfiguration));
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
            _logger = logger;
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

        /// <summary>
        /// Used to retrieve the 2FA providers for code submission
        /// </summary>
        /// <returns></returns>
        // [SetAngularAntiForgeryTokens] //TODO reintroduce when migrated to netcore
        public async Task<IEnumerable<string>> Get2FAProviders()
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Get2FAProviders :: No verified user found, returning 404");
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            var user = await UserManager.FindByIdAsync(userId);
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(user);

            return userFactors;
        }

        // [SetAngularAntiForgeryTokens] //TODO reintroduce when migrated to netcore
        public async Task<IHttpActionResult> PostSend2FACode([FromBody]string provider)
        {
            if (provider.IsNullOrWhiteSpace())
                throw new HttpResponseException(HttpStatusCode.NotFound);

            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Get2FAProviders :: No verified user found, returning 404");
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            // Generate the token and send it
            if (await SignInManager.SendTwoFactorCodeAsync(provider) == false)
            {
                return BadRequest("Invalid code");
            }
            return Ok();
        }

        // [SetAngularAntiForgeryTokens] //TODO reintroduce when migrated to netcore
        public async Task<HttpResponseMessage> PostVerify2FACode(Verify2FACodeModel model)
        {
            if (ModelState.IsValid == false)
            {
                return Request.CreateValidationErrorResponse(ModelState);
            }

            var userName = await SignInManager.GetVerifiedUserNameAsync();
            if (userName == null)
            {
                _logger.LogWarning("Get2FAProviders :: No verified user found, returning 404");
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
