using System.Data.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Installer;
using Umbraco.Cms.Core.Models.Installer;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;
using HttpResponseMessage = System.Net.Http.HttpResponseMessage;

namespace Umbraco.Cms.Infrastructure.Installer.Steps;

/// <summary>
/// Represents the installer step responsible for creating the initial user during the Umbraco installation process.
/// </summary>
public class CreateUserStep : StepBase, IInstallStep
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
    private readonly IJsonSerializer _jsonSerializer;
    private readonly ILogger<CreateUserStep> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateUserStep"/> class, which represents the installer step responsible for creating a user during the installation process.
    /// </summary>
    /// <param name="userService">Service for managing users.</param>
    /// <param name="databaseBuilder">Builds and manages the database schema.</param>
    /// <param name="httpClientFactory">Factory for creating HTTP client instances.</param>
    /// <param name="securitySettings">The security settings configuration options.</param>
    /// <param name="connectionStrings">Monitors the application's connection strings.</param>
    /// <param name="cookieManager">Manages HTTP cookies.</param>
    /// <param name="userManager">Manages back office user accounts.</param>
    /// <param name="dbProviderFactoryCreator">Creates database provider factories.</param>
    /// <param name="metricsConsentService">Handles user consent for metrics collection.</param>
    /// <param name="jsonSerializer">Serializes and deserializes JSON data.</param>
    [Obsolete("Please use the constructor that takes all parameters. Scheduled for removal in Umbraco 19.")]
    public CreateUserStep(
        IUserService userService,
        DatabaseBuilder databaseBuilder,
        IHttpClientFactory httpClientFactory,
        IOptions<SecuritySettings> securitySettings,
        IOptionsMonitor<ConnectionStrings> connectionStrings,
        ICookieManager cookieManager,
        IBackOfficeUserManager userManager,
        IDbProviderFactoryCreator dbProviderFactoryCreator,
        IMetricsConsentService metricsConsentService,
        IJsonSerializer jsonSerializer)
        : this(
            userService,
            databaseBuilder,
            httpClientFactory,
            securitySettings,
            connectionStrings,
            cookieManager,
            userManager,
            dbProviderFactoryCreator,
            metricsConsentService,
            jsonSerializer,
            StaticServiceProvider.Instance.GetRequiredService<ILogger<CreateUserStep>>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateUserStep"/> class, which handles the creation of a user during the installation process.
    /// </summary>
    /// <param name="userService">Service for managing user accounts and operations.</param>
    /// <param name="databaseBuilder">Responsible for building and initializing the database schema.</param>
    /// <param name="httpClientFactory">Factory for creating <see cref="System.Net.Http.HttpClient"/> instances.</param>
    /// <param name="securitySettings">Provides access to security-related configuration settings.</param>
    /// <param name="connectionStrings">Monitors and provides database connection string options.</param>
    /// <param name="cookieManager">Manages HTTP cookies for authentication and session management.</param>
    /// <param name="userManager">Manages back office user authentication and identity operations.</param>
    /// <param name="dbProviderFactoryCreator">Creates database provider factories for database connections.</param>
    /// <param name="metricsConsentService">Handles user consent for metrics and telemetry collection.</param>
    /// <param name="jsonSerializer">Serializes and deserializes objects to and from JSON format.</param>
    /// <param name="logger">Logs diagnostic and operational information for the <see cref="CreateUserStep"/>.</param>
    public CreateUserStep(
        IUserService userService,
        DatabaseBuilder databaseBuilder,
        IHttpClientFactory httpClientFactory,
        IOptions<SecuritySettings> securitySettings,
        IOptionsMonitor<ConnectionStrings> connectionStrings,
        ICookieManager cookieManager,
        IBackOfficeUserManager userManager,
        IDbProviderFactoryCreator dbProviderFactoryCreator,
        IMetricsConsentService metricsConsentService,
        IJsonSerializer jsonSerializer,
        ILogger<CreateUserStep> logger)
    {
        _userService = userService;
        _databaseBuilder = databaseBuilder;
        _httpClientFactory = httpClientFactory;
        _securitySettings = securitySettings.Value;
        _connectionStrings = connectionStrings;
        _cookieManager = cookieManager;
        _userManager = userManager;
        _dbProviderFactoryCreator = dbProviderFactoryCreator;
        _metricsConsentService = metricsConsentService;
        _jsonSerializer = jsonSerializer;
        _logger = logger;
    }

    /// <summary>
    /// Asynchronously creates or updates the super user during installation using the provided installation data.
    /// <para>
    /// This includes setting the user's email, name, username, and password, as well as configuring telemetry consent.
    /// If requested, the method also attempts to subscribe the user to the Umbraco newsletter. Any failures in newsletter subscription are logged but do not block installation.
    /// </para>
    /// </summary>
    /// <param name="model">The installation data containing user information and telemetry preferences.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an <see cref="Attempt{InstallationResult}"/> indicating whether the operation succeeded or failed, with error details if applicable.
    /// </returns>
    public async Task<Attempt<InstallationResult>> ExecuteAsync(InstallData model)
    {
        IUser? admin = await _userService.GetAsync(Constants.Security.SuperUserKey);
        if (admin is null)
        {
            return FailWithMessage("Could not find the super user");
        }

        UserInstallData user = model.User;
        admin.Email = user.Email.Trim();
        admin.Name = user.Name.Trim();
        admin.Username = user.Email.Trim();

        _userService.Save(admin);

        BackOfficeIdentityUser? membershipUser = await _userManager.FindByIdAsync(Constants.Security.SuperUserIdAsString);
        if (membershipUser == null)
        {
            return FailWithMessage(
                $"No user found in membership provider with id of {Constants.Security.SuperUserIdAsString}.");
        }

        // To change the password here we actually need to reset it since we don't have an old one to use to change
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(membershipUser);
        if (string.IsNullOrWhiteSpace(resetToken))
        {
            return FailWithMessage("Could not reset password: unable to generate internal reset token");
        }

        IdentityResult resetResult = await _userManager.ChangePasswordWithResetAsync(membershipUser.Id, resetToken, user.Password.Trim());
        if (!resetResult.Succeeded)
        {
            return FailWithMessage("Could not reset password: " + string.Join(", ", resetResult.Errors.ToErrorMessage()));
        }

        await _metricsConsentService.SetConsentLevelAsync(model.TelemetryLevel);

        if (model.User.SubscribeToNewsletter)
        {
            const string EmailCollectorUrl = "https://emailcollector.umbraco.io/api/EmailProxy";

            var emailModel = new EmailModel
            {
                Name = admin.Name,
                Email = admin.Email,
                UserGroup = [Constants.Security.AdminGroupAlias],
            };

            HttpClient httpClient = _httpClientFactory.CreateClient();
            using var content = new StringContent(_jsonSerializer.Serialize(emailModel), System.Text.Encoding.UTF8, "application/json");
            try
            {
                // Set a reasonable timeout of 5 seconds for web request to save subscriber.
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                HttpResponseMessage response = await httpClient.PostAsync(EmailCollectorUrl, content, cts.Token);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully subscribed the user created on installation to the Umbraco newsletter.");
                }
                else
                {
                    _logger.LogWarning("Failed to subscribe the user created on installation to the Umbraco newsletter. Status code: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                // Log and move on if a failure occurs, we don't want to block installation for this.
                _logger.LogError(ex, "Exception occurred while trying to subscribe the user created on installation to the Umbraco newsletter.");
            }

        }

        return Success();
    }

    /// <summary>
    /// Model used to subscribe to the newsletter. Aligns with EmailModel defined in Umbraco.EmailMarketing.
    /// </summary>
    private class EmailModel
    {
        /// <summary>
        /// Gets or sets the name of the user to be included in the email model.
        /// </summary>
        public required string Name { get; init; }

        /// <summary>Gets or sets the email address of the user.</summary>
        public required string Email { get; init; }

        /// <summary>
        /// Gets or sets the collection of user group names associated with the user being created.
        /// </summary>
        public required List<string> UserGroup { get; init; }
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
