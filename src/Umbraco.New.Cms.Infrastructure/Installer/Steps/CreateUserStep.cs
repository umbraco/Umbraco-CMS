using System.Collections.Specialized;
using System.Data.Common;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Extensions;
using Umbraco.New.Cms.Core.Installer;
using Umbraco.New.Cms.Core.Models.Installer;
using Constants = Umbraco.Cms.Core.Constants;
using HttpResponseMessage = System.Net.Http.HttpResponseMessage;

namespace Umbraco.New.Cms.Infrastructure.Installer.Steps;

public class CreateUserStep : IInstallStep
{
    private readonly IUserService _userService;
    private readonly DatabaseBuilder _databaseBuilder;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SecuritySettings _securitySettings;
    private readonly IOptionsMonitor<ConnectionStrings> _connectionStrings;
    private readonly ICookieManager _cookieManager;
    private readonly IBackOfficeUserManager _userManager;
    private readonly IDbProviderFactoryCreator _dbProviderFactoryCreator;
    private readonly IMetricsConsentService _metricsConsentService;

    public CreateUserStep(
        IUserService userService,
        DatabaseBuilder databaseBuilder,
        IHttpClientFactory httpClientFactory,
        IOptions<SecuritySettings> securitySettings,
        IOptionsMonitor<ConnectionStrings> connectionStrings,
        ICookieManager cookieManager,
        IBackOfficeUserManager userManager,
        IDbProviderFactoryCreator dbProviderFactoryCreator,
        IMetricsConsentService metricsConsentService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _databaseBuilder = databaseBuilder ?? throw new ArgumentNullException(nameof(databaseBuilder));
        _httpClientFactory = httpClientFactory;
        _securitySettings = securitySettings.Value ?? throw new ArgumentNullException(nameof(securitySettings));
        _connectionStrings = connectionStrings;
        _cookieManager = cookieManager;
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _dbProviderFactoryCreator = dbProviderFactoryCreator ?? throw new ArgumentNullException(nameof(dbProviderFactoryCreator));
        _metricsConsentService = metricsConsentService;
    }

    public async Task ExecuteAsync(InstallData model)
    {
            IUser? admin = _userService.GetUserById(Constants.Security.SuperUserId);
            if (admin == null)
            {
                throw new InvalidOperationException("Could not find the super user!");
            }

            UserInstallData user = model.User;
            admin.Email = user.Email.Trim();
            admin.Name = user.Name.Trim();
            admin.Username = user.Email.Trim();

            _userService.Save(admin);

            BackOfficeIdentityUser? membershipUser = await _userManager.FindByIdAsync(Constants.Security.SuperUserIdAsString);
            if (membershipUser == null)
            {
                throw new InvalidOperationException(
                    $"No user found in membership provider with id of {Constants.Security.SuperUserIdAsString}.");
            }

            //To change the password here we actually need to reset it since we don't have an old one to use to change
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(membershipUser);
            if (string.IsNullOrWhiteSpace(resetToken))
            {
                throw new InvalidOperationException("Could not reset password: unable to generate internal reset token");
            }

            IdentityResult resetResult = await _userManager.ChangePasswordWithResetAsync(membershipUser.Id, resetToken, user.Password.Trim());
            if (!resetResult.Succeeded)
            {
                throw new InvalidOperationException("Could not reset password: " + string.Join(", ", resetResult.Errors.ToErrorMessage()));
            }

            _metricsConsentService.SetConsentLevel(model.TelemetryLevel);

            if (model.User.SubscribeToNewsletter)
            {
                var values = new NameValueCollection { { "name", admin.Name }, { "email", admin.Email } };
                var content = new StringContent(JsonConvert.SerializeObject(values), Encoding.UTF8, "application/json");

                HttpClient httpClient = _httpClientFactory.CreateClient();

                try
                {
                    HttpResponseMessage response = httpClient.PostAsync("https://shop.umbraco.com/base/Ecom/SubmitEmail/installer.aspx", content).Result;
                }
                catch { /* fail in silence */ }
            }
    }

    /// <inheritdoc/>
    public Task<bool> RequiresExecutionAsync(InstallData model)
    {
        InstallState installState = GetInstallState();
        if (installState.HasFlag(InstallState.Unknown))
        {
            // In this one case when it's a brand new install and nothing has been configured, make sure the
            // back office cookie is cleared so there's no old cookies lying around causing problems
            _cookieManager.ExpireCookie(_securitySettings.AuthCookieName);
        }

        var shouldRun = installState.HasFlag(InstallState.Unknown) || !installState.HasFlag(InstallState.HasNonDefaultUser);
        return Task.FromResult(shouldRun);
    }

    private InstallState GetInstallState()
    {
        InstallState installState = InstallState.Unknown;

        if (_databaseBuilder.IsDatabaseConfigured)
        {
            installState = (installState | InstallState.HasConnectionString) & ~InstallState.Unknown;
        }

        ConnectionStrings? umbracoConnectionString = _connectionStrings.CurrentValue;

        var isConnectionStringConfigured = umbracoConnectionString.IsConnectionStringConfigured();
        if (isConnectionStringConfigured)
        {
            installState = (installState | InstallState.ConnectionStringConfigured) & ~InstallState.Unknown;
        }

        DbProviderFactory? factory = _dbProviderFactoryCreator.CreateFactory(umbracoConnectionString.ProviderName);
        var isConnectionAvailable = isConnectionStringConfigured && DbConnectionExtensions.IsConnectionAvailable(umbracoConnectionString.ConnectionString, factory);
        if (isConnectionAvailable)
        {
            installState = (installState | InstallState.CanConnect) & ~InstallState.Unknown;
        }

        var isUmbracoInstalled = isConnectionAvailable && _databaseBuilder.IsUmbracoInstalled();
        if (isUmbracoInstalled)
        {
            installState = (installState | InstallState.UmbracoInstalled) & ~InstallState.Unknown;
        }

        var hasSomeNonDefaultUser = isUmbracoInstalled && _databaseBuilder.HasSomeNonDefaultUser();
        if (hasSomeNonDefaultUser)
        {
            installState = (installState | InstallState.HasNonDefaultUser) & ~InstallState.Unknown;
        }

        return installState;
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
