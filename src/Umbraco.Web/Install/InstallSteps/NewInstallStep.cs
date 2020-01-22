using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Web.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Services;
using Umbraco.Web.Install.Models;
using Umbraco.Web.Models.Identity;
using Umbraco.Web.Security;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Web.Install.InstallSteps
{
    /// <summary>
    /// This is the first UI step for a brand new install
    /// </summary>
    /// <remarks>
    /// By default this will show the user view which is the most basic information to configure a new install, but if an install get's interrupted because of an
    /// error, etc... and the end-user refreshes the installer then we cannot show the user screen because they've already entered that information so instead we'll
    /// display a simple continue installation view.
    /// </remarks>
    [InstallSetupStep(InstallationType.NewInstall, "User", 20, "")]
    internal class NewInstallStep : InstallSetupStep<UserModel>
    {
        private readonly HttpContextBase _http;
        private readonly IUserService _userService;
        private readonly DatabaseBuilder _databaseBuilder;
        private static HttpClient _httpClient;
        private readonly IGlobalSettings _globalSettings;
        private readonly IUserPasswordConfiguration _passwordConfiguration;
        private readonly BackOfficeUserManager<BackOfficeIdentityUser> _userManager;
        private readonly IUmbracoSettingsSection _umbracoSettingsSection;
        private readonly IConnectionStrings _connectionStrings;

        public NewInstallStep(HttpContextBase http, IUserService userService, DatabaseBuilder databaseBuilder, IGlobalSettings globalSettings, IUserPasswordConfiguration passwordConfiguration, IUmbracoSettingsSection umbracoSettingsSection, IConnectionStrings connectionStrings)
        {
            _http = http;
            _userService = userService;
            _databaseBuilder = databaseBuilder;
            _globalSettings = globalSettings;
            _passwordConfiguration = passwordConfiguration ?? throw new ArgumentNullException(nameof(passwordConfiguration));
            _umbracoSettingsSection = umbracoSettingsSection ?? throw new ArgumentNullException(nameof(umbracoSettingsSection));
            _connectionStrings = connectionStrings ?? throw new ArgumentNullException(nameof(connectionStrings));
            _userManager = _http.GetOwinContext().GetBackOfficeUserManager();
        }

        public override async Task<InstallSetupResult> ExecuteAsync(UserModel user)
        {
            var admin = _userService.GetUserById(Constants.Security.SuperUserId);
            if (admin == null)
            {
                throw new InvalidOperationException("Could not find the super user!");
            }

            var membershipUser = await _userManager.FindByIdAsync(Constants.Security.SuperUserId);
            if (membershipUser == null)
            {
                throw new InvalidOperationException($"No user found in membership provider with id of {Constants.Security.SuperUserId}.");
            }

            //To change the password here we actually need to reset it since we don't have an old one to use to change
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(membershipUser.Id);
            var resetResult = await _userManager.ChangePasswordWithResetAsync(membershipUser.Id, resetToken, user.Password.Trim());
            if (!resetResult.Succeeded)
            {
                throw new InvalidOperationException("Could not reset password: " + string.Join(", ", resetResult.Errors));
            }

            admin.Email = user.Email.Trim();
            admin.Name = user.Name.Trim();
            admin.Username = user.Email.Trim();

            _userService.Save(admin);

            if (user.SubscribeToNewsLetter)
            {
                if (_httpClient == null)
                    _httpClient = new HttpClient();

                var values = new NameValueCollection { { "name", admin.Name }, { "email", admin.Email } };
                var content = new StringContent(JsonConvert.SerializeObject(values), Encoding.UTF8, "application/json");

                try
                {
                    var response = _httpClient.PostAsync("https://shop.umbraco.com/base/Ecom/SubmitEmail/installer.aspx", content).Result;
                }
                catch { /* fail in silence */ }
            }

            return null;
        }

        /// <summary>
        /// Return a custom view model for this step
        /// </summary>
        public override object ViewModel
        {
            get
            {
                return new
                {
                    minCharLength = _passwordConfiguration.RequiredLength,
                    minNonAlphaNumericLength = _passwordConfiguration.RequireNonLetterOrDigit ? 1 : 0
                };
            }
        }

        public override string View
        {
            get
            {
                return RequiresExecution(null)
              //the user UI
                ? "user"
              //the continue install UI
              : "continueinstall";
            }
        }

        public override bool RequiresExecution(UserModel model)
        {
            //now we have to check if this is really a new install, the db might be configured and might contain data
            var databaseSettings = _connectionStrings[Constants.System.UmbracoConnectionName];

            //if there's already a version then there should def be a user but in some cases someone may have
            // left a version number in there but cleared out their db conn string, in that case, it's really a new install.
            if (_globalSettings.ConfigurationStatus.IsNullOrWhiteSpace() == false && databaseSettings != null) return false;

            if (databaseSettings.IsConnectionStringConfigured() && _databaseBuilder.IsDatabaseConfigured)
                return _databaseBuilder.HasSomeNonDefaultUser() == false;

            // In this one case when it's a brand new install and nothing has been configured, make sure the
            // back office cookie is cleared so there's no old cookies lying around causing problems
            _http.ExpireCookie(_umbracoSettingsSection.Security.AuthCookieName);

                return true;
        }
    }
}
