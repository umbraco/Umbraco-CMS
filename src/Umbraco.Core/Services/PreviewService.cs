using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Preview;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

public class PreviewService : IPreviewService
{
    private readonly ICookieManager _cookieManager;
    private readonly IPreviewTokenGenerator _previewTokenGenerator;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public PreviewService(
        ICookieManager cookieManager,
        IPreviewTokenGenerator previewTokenGenerator,
        IServiceScopeFactory serviceScopeFactory)
    {
        _cookieManager = cookieManager;
        _previewTokenGenerator = previewTokenGenerator;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<bool> TryEnterPreviewAsync(IUser user)
    {
        Attempt<string?> attempt = await _previewTokenGenerator.GenerateTokenAsync(user.Key);

        if (attempt.Success)
        {
            _cookieManager.SetCookieValue(Constants.Web.PreviewCookieName, attempt.Result!, true);
        }

        return attempt.Success;
    }

    public Task EndPreviewAsync()
    {
         _cookieManager.ExpireCookie(Constants.Web.PreviewCookieName);
         return Task.CompletedTask;
    }

    public async Task<Attempt<ClaimsIdentity>> TryGetPreviewClaimsIdentityAsync()
    {
        var cookieValue = _cookieManager.GetCookieValue(Constants.Web.PreviewCookieName);
        if (string.IsNullOrWhiteSpace(cookieValue))
        {
            return Attempt<ClaimsIdentity>.Fail();
        }

        Attempt<Guid?> userKeyAttempt = await _previewTokenGenerator.VerifyAsync(cookieValue);

        if (userKeyAttempt.Success is false)
        {
            return Attempt<ClaimsIdentity>.Fail();
        }

        IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
        ICoreBackOfficeSignInManager coreBackOfficeSignInManager = serviceScope.ServiceProvider.GetRequiredService<ICoreBackOfficeSignInManager>();

        ClaimsPrincipal? principal = await coreBackOfficeSignInManager.CreateUserPrincipalAsync(userKeyAttempt.Result!.Value);
        ClaimsIdentity? backOfficeIdentity = principal?.GetUmbracoIdentity();

        return Attempt<ClaimsIdentity>.Succeed(backOfficeIdentity);
    }
}
