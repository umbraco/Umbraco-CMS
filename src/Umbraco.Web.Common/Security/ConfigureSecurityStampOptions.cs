using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Security;

public class ConfigureSecurityStampOptions : IConfigureOptions<SecurityStampValidatorOptions>
{
    private readonly SecuritySettings _securitySettings;

    public ConfigureSecurityStampOptions()
        : this(StaticServiceProvider.Instance.GetRequiredService<IOptions<SecuritySettings>>())
    {
    }

    public ConfigureSecurityStampOptions(IOptions<SecuritySettings> securitySettings)
        => _securitySettings = securitySettings.Value;

    [Obsolete("Use the overload accepting SecuritySettings instead. Scheduled for removal in v14.")]
    public static void ConfigureOptions(SecurityStampValidatorOptions options)
        => ConfigureOptions(options, StaticServiceProvider.Instance.GetRequiredService<SecuritySettings>());

    /// <summary>
    ///     Configures security stamp options and ensures any custom claims
    ///     set on the identity are persisted to the new identity when it's refreshed.
    /// </summary>
    /// <param name="options">Options for <see cref="ISecurityStampValidator"/>.</param>
    /// <param name="securitySettings">The <see cref="SecuritySettings" /> options.</param>
    public static void ConfigureOptions(SecurityStampValidatorOptions options, SecuritySettings securitySettings)
    {
        // Adjust the security stamp validation interval to a shorter duration
        // when concurrent logins are not allowed and the duration has the default interval value
        // (currently defaults to 30 minutes), ensuring quicker re-validation.
        if (securitySettings.AllowConcurrentLogins is false && options.ValidationInterval == TimeSpan.FromMinutes(30))
        {
            options.ValidationInterval = TimeSpan.FromSeconds(30);
        }

        // When refreshing the principal, ensure custom claims that
        // might have been set with an external identity continue
        // to flow through to this new one.
        options.OnRefreshingPrincipal = refreshingPrincipal =>
        {
            ClaimsIdentity? newIdentity = refreshingPrincipal.NewPrincipal?.Identities.First();
            ClaimsIdentity? currentIdentity = refreshingPrincipal.CurrentPrincipal?.Identities.First();

            if (currentIdentity is not null)
            {
                // Since this is refreshing an existing principal, we want to merge all claims.
                newIdentity?.MergeAllClaims(currentIdentity);
            }

            return Task.CompletedTask;
        };
    }

    /// <inheritdoc />
    public void Configure(SecurityStampValidatorOptions options)
        => ConfigureOptions(options, _securitySettings);
}
