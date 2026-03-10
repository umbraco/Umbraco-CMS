using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Preview;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides functionality for managing content preview mode.
/// </summary>
/// <remarks>
///     Preview mode allows backoffice users to view unpublished content changes
///     as they would appear on the front-end website. This implementation uses
///     secure cookies with SameSite=None to support cross-site preview scenarios.
/// </remarks>
public class PreviewService : IPreviewService
{
    private readonly ICookieManager _cookieManager;
    private readonly IPreviewTokenGenerator _previewTokenGenerator;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IRequestCache _requestCache;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PreviewService" /> class.
    /// </summary>
    /// <param name="cookieManager">The cookie manager for handling preview cookies.</param>
    /// <param name="previewTokenGenerator">The generator for creating and validating preview tokens.</param>
    /// <param name="serviceScopeFactory">The factory for creating service scopes.</param>
    /// <param name="requestCache">The request cache for caching preview state per request.</param>
    public PreviewService(
        ICookieManager cookieManager,
        IPreviewTokenGenerator previewTokenGenerator,
        IServiceScopeFactory serviceScopeFactory,
        IRequestCache requestCache)
    {
        _cookieManager = cookieManager;
        _previewTokenGenerator = previewTokenGenerator;
        _serviceScopeFactory = serviceScopeFactory;
        _requestCache = requestCache;
    }

    /// <inheritdoc />
    public async Task<bool> TryEnterPreviewAsync(IUser user)
    {
        Attempt<string?> attempt = await _previewTokenGenerator.GenerateTokenAsync(user.Key);

        if (attempt.Success)
        {
            // Preview cookies must use SameSite=None and Secure=true to support cross-site scenarios
            // (e.g., when the backoffice is on a different domain/port than the frontend during development).
            // SameSite=None requires Secure=true per browser specifications.
            _cookieManager.SetCookieValue(Constants.Web.PreviewCookieName, attempt.Result!, httpOnly: true, secure: true, sameSiteMode: "None");
        }

        return attempt.Success;
    }

    /// <inheritdoc />
    public Task EndPreviewAsync()
    {
         _cookieManager.ExpireCookie(Constants.Web.PreviewCookieName);
         return Task.CompletedTask;
    }

    /// <inheritdoc />
    public bool IsInPreview() =>
        _requestCache.Get(
            "IsInPreview",
            () => TryGetPreviewClaimsIdentityAsync().GetAwaiter().GetResult().Success) as bool? ?? false;

    /// <inheritdoc />
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
