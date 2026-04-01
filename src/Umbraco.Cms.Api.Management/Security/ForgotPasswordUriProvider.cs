using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="ForgotPasswordUriProvider"/> class.
    /// </summary>
    /// <param name="userManager">An <see cref="ICoreBackOfficeUserManager"/> instance for managing back office users.</param>
    /// <param name="hostingEnvironment">An <see cref="IHostingEnvironment"/> instance representing the current hosting environment.</param>
    /// <param name="httpContextAccessor">An <see cref="IHttpContextAccessor"/> instance for accessing the current HTTP context.</param>
    public ForgotPasswordUriProvider(
        ICoreBackOfficeUserManager userManager,
        IHostingEnvironment hostingEnvironment,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _hostingEnvironment = hostingEnvironment;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc/>
    public async Task<Attempt<Uri, UserOperationStatus>> CreateForgotPasswordUriAsync(IUser user)
    {
        if (_httpContextAccessor.HttpContext is null)
        {
            throw new NotSupportedException("Needs a HttpContext");
        }

        Uri? appUrl = _hostingEnvironment.ApplicationMainUrl;
        if (appUrl is null)
        {
            return Attempt.FailWithStatus<Uri, UserOperationStatus>(UserOperationStatus.ApplicationUrlNotConfigured, default!);
        }

        Attempt<string, UserOperationStatus> tokenAttempt = await _userManager.GeneratePasswordResetTokenAsync(user);

        if (tokenAttempt.Success is false)
        {
            return Attempt.FailWithStatus<Uri, UserOperationStatus>(tokenAttempt.Status, default!);
        }

        var uriBuilder = new UriBuilder(appUrl)
        {
            Path = BackOfficeLoginController.LoginPath,
            Query = QueryString.Create(new KeyValuePair<string, string?>[]
            {
                new("flow", "reset-password"),
                new("userId", user.Key.ToString()),
                new("resetCode", tokenAttempt.Result.ToUrlBase64()),
            }).ToUriComponent(),
        };

        return Attempt.SucceedWithStatus(UserOperationStatus.Success, uriBuilder.Uri);
    }
}
