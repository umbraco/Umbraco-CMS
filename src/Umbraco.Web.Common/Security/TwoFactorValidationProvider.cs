using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Security;

public class TwoFactorBackOfficeValidationProvider<TTwoFactorSetupGenerator> : TwoFactorValidationProvider<BackOfficeIdentityUser,
        TTwoFactorSetupGenerator>
    where TTwoFactorSetupGenerator : ITwoFactorProvider
{
    public TwoFactorBackOfficeValidationProvider(
        IDataProtectionProvider dataProtectionProvider,
        IOptions<DataProtectionTokenProviderOptions> options,
        ILogger<TwoFactorBackOfficeValidationProvider<TTwoFactorSetupGenerator>> logger,
        ITwoFactorLoginService twoFactorLoginService,
        TTwoFactorSetupGenerator generator)
        : base(dataProtectionProvider, options, logger, twoFactorLoginService, generator)
    {
    }
}

public class TwoFactorMemberValidationProvider<TTwoFactorSetupGenerator> : TwoFactorValidationProvider<MemberIdentityUser,
        TTwoFactorSetupGenerator>
    where TTwoFactorSetupGenerator : ITwoFactorProvider
{
    public TwoFactorMemberValidationProvider(
        IDataProtectionProvider dataProtectionProvider,
        IOptions<DataProtectionTokenProviderOptions> options,
        ILogger<TwoFactorMemberValidationProvider<TTwoFactorSetupGenerator>> logger,
        ITwoFactorLoginService twoFactorLoginService,
        TTwoFactorSetupGenerator generator)
        : base(dataProtectionProvider, options, logger, twoFactorLoginService, generator)
    {
    }
}

public class TwoFactorValidationProvider<TUmbracoIdentityUser, TTwoFactorSetupGenerator>
    : DataProtectorTokenProvider<TUmbracoIdentityUser>
    where TUmbracoIdentityUser : UmbracoIdentityUser
    where TTwoFactorSetupGenerator : ITwoFactorProvider
{
    private readonly TTwoFactorSetupGenerator _generator;
    private readonly ITwoFactorLoginService _twoFactorLoginService;

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

    public override Task<bool> CanGenerateTwoFactorTokenAsync(
        UserManager<TUmbracoIdentityUser> manager,
        TUmbracoIdentityUser user) => Task.FromResult(_generator is not null);

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
