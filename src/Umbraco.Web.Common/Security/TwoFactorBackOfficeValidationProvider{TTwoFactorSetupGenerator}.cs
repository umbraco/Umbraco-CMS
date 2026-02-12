using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Security;

/// <summary>
/// A two-factor validation provider for back office users.
/// </summary>
/// <typeparam name="TTwoFactorSetupGenerator">The type of the two-factor setup generator.</typeparam>
public class TwoFactorBackOfficeValidationProvider<TTwoFactorSetupGenerator> : TwoFactorValidationProvider<BackOfficeIdentityUser,
        TTwoFactorSetupGenerator>
    where TTwoFactorSetupGenerator : ITwoFactorProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TwoFactorBackOfficeValidationProvider{TTwoFactorSetupGenerator}" /> class.
    /// </summary>
    /// <param name="dataProtectionProvider">The data protection provider.</param>
    /// <param name="options">The token provider options.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="twoFactorLoginService">The two-factor login service.</param>
    /// <param name="generator">The two-factor setup generator.</param>
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
