using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Security;

/// <summary>
/// Provides functionality to generate invite URIs for user invitations.
/// </summary>
public class InviteUriProvider : IInviteUriProvider
{
    private readonly ICoreBackOfficeUserManager _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHostingEnvironment _hostingEnvironment;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Api.Management.Security.InviteUriProvider"/> class, used to generate invite URIs for user management.
    /// </summary>
    /// <param name="userManager">The user manager responsible for core back office user operations.</param>
    /// <param name="httpContextAccessor">Provides access to the current HTTP context.</param>
    /// <param name="hostingEnvironment">Provides information about the web hosting environment.</param>
    public InviteUriProvider(
        ICoreBackOfficeUserManager userManager,
        IHttpContextAccessor httpContextAccessor,
        IHostingEnvironment hostingEnvironment)
    {

        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _hostingEnvironment = hostingEnvironment;
    }

    /// <summary>
    /// Asynchronously creates an invitation URI for the specified user to be invited to the system.
    /// </summary>
    /// <param name="invitee">The user to invite.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an <see cref="Attempt{Uri, UserOperationStatus}"/> indicating success with the generated invitation URI, or failure with the corresponding status.
    /// </returns>
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
