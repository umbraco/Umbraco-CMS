using Microsoft.Extensions.Options;
using System;

namespace Umbraco.Web.BackOffice.Security
{
    /// <summary>
    /// Configures the back office security stamp options
    /// </summary>
    public class ConfigureBackOfficeSecurityStampValidatorOptions : IConfigureOptions<BackOfficeSecurityStampValidatorOptions>
    {
        public void Configure(BackOfficeSecurityStampValidatorOptions options)
        {
            options.ValidationInterval = TimeSpan.FromMinutes(30);
        }
    }


}
