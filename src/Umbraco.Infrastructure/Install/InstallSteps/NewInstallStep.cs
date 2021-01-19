using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Security;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Extensions;
using Umbraco.Web.Install.Models;

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
    public class NewInstallStep : InstallSetupStep<UserModel>
    {
        private readonly IUserService _userService;
        private readonly DatabaseBuilder _databaseBuilder;
        private static HttpClient _httpClient;
        private readonly UserPasswordConfigurationSettings _passwordConfiguration;
        private readonly SecuritySettings _securitySettings;
        private readonly ConnectionStrings _connectionStrings;
        private readonly ICookieManager _cookieManager;
        private readonly IBackOfficeUserManager _userManager;
        private readonly IDbProviderFactoryCreator _dbProviderFactoryCreator;

        public NewInstallStep(
            IUserService userService,
            DatabaseBuilder databaseBuilder,
            IOptions<UserPasswordConfigurationSettings> passwordConfiguration,
            IOptions<SecuritySettings> securitySettings,
            IOptions<ConnectionStrings> connectionStrings,
            ICookieManager cookieManager,
            IBackOfficeUserManager userManager,
            IDbProviderFactoryCreator dbProviderFactoryCreator)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _databaseBuilder = databaseBuilder ?? throw new ArgumentNullException(nameof(databaseBuilder));
            _passwordConfiguration = passwordConfiguration.Value ?? throw new ArgumentNullException(nameof(passwordConfiguration));
            _securitySettings = securitySettings.Value ?? throw new ArgumentNullException(nameof(securitySettings));
            _connectionStrings = connectionStrings.Value ?? throw new ArgumentNullException(nameof(connectionStrings));
            _cookieManager = cookieManager;
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _dbProviderFactoryCreator = dbProviderFactoryCreator ?? throw new ArgumentNullException(nameof(dbProviderFactoryCreator));
        }

        public override async Task<InstallSetupResult> ExecuteAsync(UserModel user)
        {
            var admin = _userService.GetUserById(Constants.Security.SuperUserId);
            if (admin == null)
            {
                throw new InvalidOperationException("Could not find the super user!");
            }
            admin.Email = user.Email.Trim();
            admin.Name = user.Name.Trim();
            admin.Username = user.Email.Trim();

            _userService.Save(admin);

            var membershipUser = await _userManager.FindByIdAsync(Constants.Security.SuperUserId.ToString());
            if (membershipUser == null)
            {
                throw new InvalidOperationException(
                    $"No user found in membership provider with id of {Constants.Security.SuperUserId}.");
            }

            //To change the password here we actually need to reset it since we don't have an old one to use to change
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(membershipUser);
            if (string.IsNullOrWhiteSpace(resetToken))
                throw new InvalidOperationException("Could not reset password: unable to generate internal reset token");

            var resetResult = await _userManager.ChangePasswordWithResetAsync(membershipUser.Id, resetToken, user.Password.Trim());
            if (!resetResult.Succeeded)
                throw new InvalidOperationException("Could not reset password: " + string.Join(", ", resetResult.Errors.ToErrorMessage()));

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
                    minNonAlphaNumericLength = _passwordConfiguration.RequireNonLetterOrDigit ? 1 : 0,
                    quickInstallAvailable = DatabaseConfigureStep.IsSqlCeAvailable()
                };
            }
        }

        public override string View
        {
            get
            {
                return ShowView()
                    // the user UI
                    ? "user"
                    // continue install UI
                    : "continueinstall";
            }
        }

        private InstallState GetInstallState()
        {
            var installState = InstallState.Unknown;

            var databaseSettings = _connectionStrings.UmbracoConnectionString;

            var hasConnString = databaseSettings != null && _databaseBuilder.IsDatabaseConfigured;
            if (hasConnString)
            {
                installState = (installState | InstallState.HasConnectionString) & ~InstallState.Unknown;
            }

            var connStringConfigured = databaseSettings.IsConnectionStringConfigured();
            if (connStringConfigured)
            {
                installState = (installState | InstallState.ConnectionStringConfigured) & ~InstallState.Unknown;
            }

            var factory = _dbProviderFactoryCreator.CreateFactory(databaseSettings.ProviderName);
            var canConnect = connStringConfigured && DbConnectionExtensions.IsConnectionAvailable(databaseSettings.ConnectionString, factory);
            if (canConnect)
            {
                installState = (installState | InstallState.CanConnect) & ~InstallState.Unknown;
            }

            var umbracoInstalled = canConnect ? _databaseBuilder.IsUmbracoInstalled() : false;
            if (umbracoInstalled)
            {
                installState = (installState | InstallState.UmbracoInstalled) & ~InstallState.Unknown;
            }

            var hasNonDefaultUser = umbracoInstalled ? _databaseBuilder.HasSomeNonDefaultUser() : false;
            if (hasNonDefaultUser)
            {
                installState = (installState | InstallState.HasNonDefaultUser) & ~InstallState.Unknown;
            }

            return installState;
        }

        private bool ShowView()
        {
            var installState = GetInstallState();

            return installState.HasFlag(InstallState.Unknown)
                || !installState.HasFlag(InstallState.UmbracoInstalled);
        }

        public override bool RequiresExecution(UserModel model)
        {
            var installState = GetInstallState();

            if (installState.HasFlag(InstallState.Unknown))
            {
                // In this one case when it's a brand new install and nothing has been configured, make sure the
                // back office cookie is cleared so there's no old cookies lying around causing problems
                _cookieManager.ExpireCookie(_securitySettings.AuthCookieName);
            }

            return installState.HasFlag(InstallState.Unknown)
                || !installState.HasFlag(InstallState.HasNonDefaultUser);
        }

        [Flags]
        private enum InstallState : short
        {
            // This is an easy way to avoid 0 enum assignment and not worry about
            // manual calcs. https://www.codeproject.com/Articles/396851/Ending-the-Great-Debate-on-Enum-Flags
            Unknown = 1,
            HasVersion = 1 << 1,
            HasConnectionString = 1 << 2,
            ConnectionStringConfigured = 1 << 3,
            CanConnect = 1 << 4,
            UmbracoInstalled = 1 << 5,
            HasNonDefaultUser = 1 << 6
        }
    }
}
