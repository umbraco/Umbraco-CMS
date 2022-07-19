using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Security;

public class ConfigureSecurityStampOptions : IConfigureOptions<SecurityStampValidatorOptions>
{
    /// <summary>
    ///     Configures security stamp options and ensures any custom claims
    ///     set on the identity are persisted to the new identity when it's refreshed.
    /// </summary>
    /// <param name="options"></param>
    public static void ConfigureOptions(SecurityStampValidatorOptions options)
    {
        options.ValidationInterval = TimeSpan.FromMinutes(30);

        // When refreshing the principal, ensure custom claims that
        // might have been set with an external identity continue
        // to flow through to this new one.
        options.OnRefreshingPrincipal = refreshingPrincipal =>
        {
            ClaimsIdentity newIdentity = refreshingPrincipal.NewPrincipal.Identities.First();
            ClaimsIdentity currentIdentity = refreshingPrincipal.CurrentPrincipal.Identities.First();

            // Since this is refreshing an existing principal, we want to merge all claims.
            newIdentity.MergeAllClaims(currentIdentity);

            return Task.CompletedTask;
        };
    }

    public void Configure(SecurityStampValidatorOptions options)
        => ConfigureOptions(options);
}
