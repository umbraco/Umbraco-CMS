using System;
using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Cms.Web.BackOffice.Security
{
    /// <summary>
    /// Used to add back office login providers
    /// </summary>
    public class BackOfficeExternalLoginsBuilder
    {
        public BackOfficeExternalLoginsBuilder(IServiceCollection services)
        {
            _services = services;
        }

        private readonly IServiceCollection _services;

        /// <summary>
        /// Add a back office login provider with options
        /// </summary>
        /// <param name="loginProviderOptions"></param>
        /// <param name="build"></param>
        /// <returns></returns>
        public BackOfficeExternalLoginsBuilder AddBackOfficeLogin(
            Action<BackOfficeAuthenticationBuilder> build,
            Action<BackOfficeExternalLoginProviderOptions>? loginProviderOptions = null)
        {
            build(new BackOfficeAuthenticationBuilder(_services, loginProviderOptions));
            return this;
        }
    }

}
