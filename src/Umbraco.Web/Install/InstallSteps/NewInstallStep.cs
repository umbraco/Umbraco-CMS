using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Security;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Services;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.InstallSteps
{
    /// <summary>
    /// This is the first UI step for a brand new install
    /// </summary>
    /// <remarks>
    /// By default this will show the user view which is the most basic information to configure a new install, but if an install get's interupted because of an
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

        public NewInstallStep(HttpContextBase http, IUserService userService, DatabaseBuilder databaseBuilder, IGlobalSettings globalSettings)
        {
            _http = http;
            _userService = userService;
            _databaseBuilder = databaseBuilder;
            _globalSettings = globalSettings;
        }

        //TODO: Change all logic in this step to use ASP.NET Identity NOT MembershipProviders
        private MembershipProvider CurrentProvider
        {
            get
            {
                var provider = Core.Security.MembershipProviderExtensions.GetUsersMembershipProvider();
                return provider;
            }
        }

        public override InstallSetupResult Execute(UserModel user)
        {
            var admin = _userService.GetUserById(Constants.Security.SuperUserId);
            if (admin == null)
            {
                throw new InvalidOperationException("Could not find the super user!");
            }

            var membershipUser = CurrentProvider.GetUser(Constants.Security.SuperUserId, true);
            if (membershipUser == null)
            {
                throw new InvalidOperationException($"No user found in membership provider with id of {Constants.Security.SuperUserId}.");
            }

            try
            {
                var success = membershipUser.ChangePassword("default", user.Password.Trim());
                if (success == false)
                {
                    throw new FormatException("Password must be at least " + CurrentProvider.MinRequiredPasswordLength + " characters long and contain at least " + CurrentProvider.MinRequiredNonAlphanumericCharacters + " symbols");
                }
            }
            catch (Exception)
            {
                throw new FormatException("Password must be at least " + CurrentProvider.MinRequiredPasswordLength + " characters long and contain at least " + CurrentProvider.MinRequiredNonAlphanumericCharacters + " symbols");
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
                var provider = Core.Security.MembershipProviderExtensions.GetUsersMembershipProvider();

                return new
                {
                    minCharLength = provider.MinRequiredPasswordLength,
                    minNonAlphaNumericLength = provider.MinRequiredNonAlphanumericCharacters
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
            var databaseSettings = ConfigurationManager.ConnectionStrings[Constants.System.UmbracoConnectionName];

            //if there's already a version then there should def be a user but in some cases someone may have
            // left a version number in there but cleared out their db conn string, in that case, it's really a new install.
            if (_globalSettings.ConfigurationStatus.IsNullOrWhiteSpace() == false && databaseSettings != null) return false;

            if (_databaseBuilder.IsConnectionStringConfigured(databaseSettings) && _databaseBuilder.IsDatabaseConfigured)
                return _databaseBuilder.HasSomeNonDefaultUser() == false;

            // In this one case when it's a brand new install and nothing has been configured, make sure the
            // back office cookie is cleared so there's no old cookies lying around causing problems
            _http.ExpireCookie(Current.Config.Umbraco().Security.AuthCookieName);

                return true;
        }
    }
}
