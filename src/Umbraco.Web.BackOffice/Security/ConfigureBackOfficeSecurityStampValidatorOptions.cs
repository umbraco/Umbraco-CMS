using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Security
{
    /// <summary>
    /// Configures the back office security stamp options
    /// </summary>
    public class ConfigureBackOfficeSecurityStampValidatorOptions : IConfigureOptions<BackOfficeSecurityStampValidatorOptions>
    {
        public void Configure(BackOfficeSecurityStampValidatorOptions options)
        {
            options.ValidationInterval = TimeSpan.FromMinutes(3);

            // When refreshing the principal, ensure custom claims that
            // might have been set with an external identity continue
            // to flow through to this new one.
            options.OnRefreshingPrincipal = refreshingPrincipal =>
            {
                ClaimsIdentity newIdentity = refreshingPrincipal.NewPrincipal.Identities.First();
                ClaimsIdentity currentIdentity = refreshingPrincipal.CurrentPrincipal.Identities.First();

                newIdentity.MergeClaimsFromBackOfficeIdentity(currentIdentity);

                return Task.CompletedTask;
            };
        }
    }


}
