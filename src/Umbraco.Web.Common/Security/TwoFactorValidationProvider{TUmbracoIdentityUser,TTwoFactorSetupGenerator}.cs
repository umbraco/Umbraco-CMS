using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Security;

/// <summary>
/// Base class for two-factor validation providers that validate tokens using an <see cref="ITwoFactorProvider" />.
/// </summary>
/// <typeparam name="TUmbracoIdentityUser">The type of the Umbraco identity user.</typeparam>
/// <typeparam name="TTwoFactorSetupGenerator">The type of the two-factor setup generator.</typeparam>
public class TwoFactorValidationProvider<TUmbracoIdentityUser, TTwoFactorSetupGenerator>
    : DataProtectorTokenProvider<TUmbracoIdentityUser>
    where TUmbracoIdentityUser : UmbracoIdentityUser
    where TTwoFactorSetupGenerator : ITwoFactorProvider
{
    private readonly TTwoFactorSetupGenerator _generator;
    private readonly ITwoFactorLoginService _twoFactorLoginService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TwoFactorValidationProvider{TUmbracoIdentityUser, TTwoFactorSetupGenerator}" /> class.
    /// </summary>
    /// <param name="dataProtectionProvider">The data protection provider.</param>
    /// <param name="options">The token provider options.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="twoFactorLoginService">The two-factor login service.</param>
    /// <param name="generator">The two-factor setup generator.</param>
    protected TwoFactorValidationProvider(
        IDataProtectionProvider dataProtectionProvider,
        IOptions<DataProtectionTokenProviderOptions> options,
        ILogger<TwoFactorValidationProvider<TUmbracoIdentityUser, TTwoFactorSetupGenerator>> logger,
        ITwoFactorLoginService twoFactorLoginService,
        TTwoFactorSetupGenerator generator)
        : base(dataProtectionProvider, options, logger)
    {
        _twoFactorLoginService = twoFactorLoginService;
        _generator = generator;
    }

    /// <inheritdoc />
    public override Task<bool> CanGenerateTwoFactorTokenAsync(
        UserManager<TUmbracoIdentityUser> manager,
        TUmbracoIdentityUser user) => Task.FromResult(_generator is not null);

    /// <inheritdoc />
    public override async Task<bool> ValidateAsync(string purpose, string token, UserManager<TUmbracoIdentityUser> manager, TUmbracoIdentityUser user)
    {
        var secret =
            await _twoFactorLoginService.GetSecretForUserAndProviderAsync(GetUserKey(user), _generator.ProviderName);

        if (secret is null)
        {
            return false;
        }

        var validToken = _generator.ValidateTwoFactorPIN(secret, token);

        return validToken;
    }

    /// <summary>
    /// Gets the unique key for the specified user.
    /// </summary>
    /// <param name="user">The user to get the key for.</param>
    /// <returns>The unique key for the user.</returns>
    /// <exception cref="NotSupportedException">Thrown when the user type is not supported.</exception>
    protected Guid GetUserKey(TUmbracoIdentityUser user)
    {
        switch (user)
        {
            case MemberIdentityUser memberIdentityUser:
                return memberIdentityUser.Key;
            case BackOfficeIdentityUser backOfficeIdentityUser:
                return backOfficeIdentityUser.Key;
            default:
                throw new NotSupportedException(
                    "Current we only support MemberIdentityUser and BackOfficeIdentityUser");
        }
    }
}
