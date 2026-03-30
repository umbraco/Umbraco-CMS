using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Security;

/// <summary>
/// Provides URIs used in the forgot password process for user account recovery.
/// </summary>
public class ForgotPasswordUriProvider : IForgotPasswordUriProvider
{
    private readonly ICoreBackOfficeUserManager _userManager;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ForgotPasswordUriProvider> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ForgotPasswordUriProvider"/> class.
    /// </summary>
    /// <param name="userManager">An <see cref="ICoreBackOfficeUserManager"/> instance for managing back office users.</param>
    /// <param name="hostingEnvironment">An <see cref="IHostingEnvironment"/> instance representing the current hosting environment.</param>
    /// <param name="httpContextAccessor">An <see cref="IHttpContextAccessor"/> instance for accessing the current HTTP context.</param>
    /// <param name="logger">A logger instance for diagnostic messages.</param>
    public ForgotPasswordUriProvider(
        ICoreBackOfficeUserManager userManager,
        IHostingEnvironment hostingEnvironment,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ForgotPasswordUriProvider> logger)
    {
        _userManager = userManager;
        _hostingEnvironment = hostingEnvironment;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForgotPasswordUriProvider"/> class.
    /// </summary>
    /// <param name="userManager">An <see cref="ICoreBackOfficeUserManager"/> instance for managing back office users.</param>
    /// <param name="hostingEnvironment">An <see cref="IHostingEnvironment"/> instance representing the current hosting environment.</param>
    /// <param name="httpContextAccessor">An <see cref="IHttpContextAccessor"/> instance for accessing the current HTTP context.</param>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public ForgotPasswordUriProvider(
        ICoreBackOfficeUserManager userManager,
        IHostingEnvironment hostingEnvironment,
        IHttpContextAccessor httpContextAccessor)
        : this(
            userManager,
            hostingEnvironment,
            httpContextAccessor,
            StaticServiceProvider.Instance.GetRequiredService<ILogger<ForgotPasswordUriProvider>>())
    {
    }

    /// <inheritdoc/>
    public async Task<Attempt<Uri, UserOperationStatus>> CreateForgotPasswordUriAsync(IUser user)
    {
        Attempt<string, UserOperationStatus> tokenAttempt = await _userManager.GeneratePasswordResetTokenAsync(user);

        if (tokenAttempt.Success is false)
        {
            return Attempt.FailWithStatus(tokenAttempt.Status, new Uri(string.Empty));
        }

        HttpRequest? request = _httpContextAccessor.HttpContext?.Request;
        if (request is null)
        {
            throw new NotSupportedException("Needs a HttpContext");
        }

        var query = QueryString.Create(new KeyValuePair<string, string?>[]
        {
            new("flow", "reset-password"),
            new("userId", user.Key.ToString()),
            new("resetCode", tokenAttempt.Result.ToUrlBase64()),
        }).ToUriComponent();

        Uri? appUrl = _hostingEnvironment.ApplicationMainUrl;
        if (appUrl is null)
        {
            _logger.LogWarning(
                "ApplicationMainUrl is not configured. Password reset link will use a relative path. "
                + "Set Umbraco:CMS:WebRouting:UmbracoApplicationUrl in configuration.");

            return Attempt.SucceedWithStatus(
                UserOperationStatus.Success,
                new Uri(BackOfficeLoginController.LoginPath + query, UriKind.Relative));
        }

        var uriBuilder = new UriBuilder(appUrl)
        {
            Path = BackOfficeLoginController.LoginPath,
            Query = query,
        };

        return Attempt.SucceedWithStatus(UserOperationStatus.Success, uriBuilder.Uri);
    }
}
