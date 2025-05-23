using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Security;

public class InviteUriProvider : IInviteUriProvider
{
    private readonly ICoreBackOfficeUserManager _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHostingEnvironment _hostingEnvironment;

    public InviteUriProvider(
        ICoreBackOfficeUserManager userManager,
        IHttpContextAccessor httpContextAccessor,
        IHostingEnvironment hostingEnvironment)
    {

        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _hostingEnvironment = hostingEnvironment;
    }

    public async Task<Attempt<Uri, UserOperationStatus>> CreateInviteUriAsync(IUser invitee)
    {
        Attempt<string, UserOperationStatus> tokenAttempt = await _userManager.GenerateEmailConfirmationTokenAsync(invitee);

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
            new ("flow", "invite-user"),
            new ("userId", invitee.Key.ToString()),
            new ("inviteCode", tokenAttempt.Result.ToUrlBase64()),
        }).ToUriComponent();

        return Attempt.SucceedWithStatus(UserOperationStatus.Success, uriBuilder.Uri);
    }
}
