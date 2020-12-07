using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using Umbraco.Core;

namespace Umbraco.Web.BackOffice.Security
{
    /// <summary>
    /// Custom <see cref="AuthenticationBuilder"/> used to associate external logins with umbraco external login options
    /// </summary>
    public class BackOfficeAuthenticationBuilder : AuthenticationBuilder
    {
        private readonly BackOfficeExternalLoginProviderOptions _loginProviderOptions;

        public BackOfficeAuthenticationBuilder(IServiceCollection services, BackOfficeExternalLoginProviderOptions loginProviderOptions)
            : base(services)
        {
            _loginProviderOptions = loginProviderOptions;
        }

        public string SchemeForBackOffice(string scheme)
        {
            return Constants.Security.BackOfficeExternalAuthenticationTypePrefix + scheme;
        }

        /// <summary>
        /// Overridden to track the final authenticationScheme being registered for the external login
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <typeparam name="THandler"></typeparam>
        /// <param name="authenticationScheme"></param>
        /// <param name="displayName"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public override AuthenticationBuilder AddRemoteScheme<TOptions, THandler>(string authenticationScheme, string displayName, Action<TOptions> configureOptions)
        {            
            // Validate that the prefix is set
            if (!authenticationScheme.StartsWith(Constants.Security.BackOfficeExternalAuthenticationTypePrefix))
            {
                throw new InvalidOperationException($"The {nameof(authenticationScheme)} is not prefixed with {Constants.Security.BackOfficeExternalAuthenticationTypePrefix}. The scheme must be created with a call to the method {nameof(SchemeForBackOffice)}");
            }

            // add our login provider to the container along with a custom options configuration
            Services.AddSingleton(x => new BackOfficeExternalLoginProvider(displayName, authenticationScheme, _loginProviderOptions));
            Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<TOptions>, EnsureBackOfficeScheme<TOptions>>());

            return base.AddRemoteScheme<TOptions, THandler>(authenticationScheme, displayName, configureOptions);
        }

        // TODO: We could override and throw NotImplementedException for other methods?

        // Ensures that the sign in scheme is always the Umbraco back office external type
        private class EnsureBackOfficeScheme<TOptions> : IPostConfigureOptions<TOptions> where TOptions : RemoteAuthenticationOptions
        {
            public void PostConfigure(string name, TOptions options)
            {
                options.SignInScheme = Constants.Security.BackOfficeExternalAuthenticationType;
            }
        }
    }

}
