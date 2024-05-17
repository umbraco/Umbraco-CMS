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

    public async Task EnterPreviewAsync(IUser user)
    {
        Attempt<string?> attempt = await _previewTokenGenerator.GenerateTokenAsync(user.Key);

        if (attempt.Success)
        {
            _cookieManager.SetCookieValue(Constants.Web.PreviewCookieName, attempt.Result!, true);
        }
        }


    public Task EndPreviewAsync()
    {
         _cookieManager.ExpireCookie(Constants.Web.PreviewCookieName);
         return Task.CompletedTask;
    }

    public async Task<ClaimsIdentity?> TryGetPreviewClaimsIdentityAsync()
    {
        var cookieValue = _cookieManager.GetCookieValue(Constants.Web.PreviewCookieName);
        if (string.IsNullOrWhiteSpace(cookieValue))
        {
            return null;
        }

        Attempt<Guid?> attempt = await _previewTokenGenerator.VerifyAsync(cookieValue);

        if (attempt.Success is false)
        {
            return null;
        }

        Guid? userKey = attempt.Result;
        if (userKey is null)
        {
            return null;
        }

        IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
        ICoreBackOfficeSignInManager coreBackOfficeSignInManager = serviceScope.ServiceProvider.GetRequiredService<ICoreBackOfficeSignInManager>();

        ClaimsPrincipal? principal = await coreBackOfficeSignInManager.CreateUserPrincipalAsync(userKey.Value);
        ClaimsIdentity? backOfficeIdentity = principal?.GetUmbracoIdentity();

        return backOfficeIdentity;
    }
}
