using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Security;

public class ForgotPasswordUriProvider : IForgotPasswordUriProvider
{
    private readonly LinkGenerator _linkGenerator;
    private readonly ICoreBackOfficeUserManager _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly WebRoutingSettings _webRoutingSettings;

    public ForgotPasswordUriProvider(
        LinkGenerator linkGenerator,
        ICoreBackOfficeUserManager userManager,
        IHttpContextAccessor httpContextAccessor,
        IOptions<WebRoutingSettings> webRoutingSettings)
    {
        _linkGenerator = linkGenerator;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _webRoutingSettings = webRoutingSettings.Value;
    }

    public async Task<Attempt<Uri, UserOperationStatus>> CreateForgotPasswordUriAsync(IUser user)
    {
        Attempt<string, UserOperationStatus> tokenAttempt = await _userManager.GeneratePasswordResetTokenAsync(user);

        if (tokenAttempt.Success is false)
        {
            return Attempt.FailWithStatus(tokenAttempt.Status, new Uri(string.Empty));
        }

        string forgotPasswordToken = $"{user.Key}{WebUtility.UrlEncode("|")}{tokenAttempt.Result.ToUrlBase64()}";

        // FIXME: This will need to change.
        string? action = _linkGenerator.GetPathByAction(
            nameof(BackOfficeController.ValidatePasswordResetCode),
            ControllerExtensions.GetControllerName<BackOfficeController>(),
            new { area = Constants.Web.Mvc.BackOfficeArea, invite = forgotPasswordToken });

        Uri applicationUri = _httpContextAccessor
            .GetRequiredHttpContext()
            .Request
            .GetApplicationUri(_webRoutingSettings);

        var inviteUri = new Uri(applicationUri, action);
        return Attempt.SucceedWithStatus(UserOperationStatus.Success, inviteUri);
    }
}
