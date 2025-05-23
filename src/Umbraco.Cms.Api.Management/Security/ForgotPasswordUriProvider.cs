using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Security;

public class ForgotPasswordUriProvider : IForgotPasswordUriProvider
{

    private readonly ICoreBackOfficeUserManager _userManager;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ForgotPasswordUriProvider(
        ICoreBackOfficeUserManager userManager,
        IHostingEnvironment hostingEnvironment,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _hostingEnvironment = hostingEnvironment;
        _httpContextAccessor = httpContextAccessor;
    }

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

        var uriBuilder = new UriBuilder(_hostingEnvironment.ApplicationMainUrl);
        uriBuilder.Path = BackOfficeLoginController.LoginPath;
        uriBuilder.Query = QueryString.Create(new KeyValuePair<string, string?>[]
        {
            new ("flow", "reset-password"),
            new ("userId", user.Key.ToString()),
            new ("resetCode", tokenAttempt.Result.ToUrlBase64()),
        }).ToUriComponent();

        return Attempt.SucceedWithStatus(UserOperationStatus.Success, uriBuilder.Uri);
    }
}
