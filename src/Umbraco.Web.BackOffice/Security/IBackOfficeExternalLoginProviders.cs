using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.Builder;

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
            //Ensure the prefix is set
            if (!authenticationScheme.StartsWith(Constants.Security.BackOfficeExternalAuthenticationTypePrefix))
            {
                authenticationScheme = Constants.Security.BackOfficeExternalAuthenticationTypePrefix + authenticationScheme;
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
            BackOfficeExternalLoginProviderOptions loginProviderOptions,
            Action<BackOfficeAuthenticationBuilder> build)
        {
            build(new BackOfficeAuthenticationBuilder(_services, loginProviderOptions));
            return this;
        }
    }

    public static class AuthenticationBuilderExtensions
    {
        public static IUmbracoBuilder AddBackOfficeExternalLogins(this IUmbracoBuilder umbracoBuilder, Action<BackOfficeExternalLoginsBuilder> builder)
        {
            builder(new BackOfficeExternalLoginsBuilder(umbracoBuilder.Services));
            return umbracoBuilder;
        }
    }

    // TODO: We need to implement this and extend it to support the back office external login options
    // basically migrate things from AuthenticationManagerExtensions & AuthenticationOptionsExtensions
    // and use this to get the back office external login infos
    public interface IBackOfficeExternalLoginProviders
    {
        BackOfficeExternalLoginProvider Get(string authenticationType);

        IEnumerable<BackOfficeExternalLoginProvider> GetBackOfficeProviders();

        /// <summary>
        /// Returns the authentication type for the last registered external login (oauth) provider that specifies an auto-login redirect option
        /// </summary>
        /// <param name="manager"></param>
        /// <returns></returns>
        string GetAutoLoginProvider();

        /// <summary>
        /// Returns true if there is any external provider that has the Deny Local Login option configured
        /// </summary>
        /// <returns></returns>
        bool HasDenyLocalLogin();
    }

    public class BackOfficeExternalLoginProviders : IBackOfficeExternalLoginProviders
    {
        public BackOfficeExternalLoginProviders(IEnumerable<BackOfficeExternalLoginProvider> externalLogins)
        {
            _externalLogins = externalLogins;
        }

        private readonly IEnumerable<BackOfficeExternalLoginProvider> _externalLogins;

        public BackOfficeExternalLoginProvider Get(string authenticationType)
        {
            return _externalLogins.FirstOrDefault(x => x.AuthenticationType == authenticationType);
        }

        public string GetAutoLoginProvider()
        {
            var found = _externalLogins.Where(x => x.Options.AutoRedirectLoginToExternalProvider).ToList();
            return found.Count > 0 ? found[0].AuthenticationType : null;
        }

        public IEnumerable<BackOfficeExternalLoginProvider> GetBackOfficeProviders()
        {
            return _externalLogins;
        }

        public bool HasDenyLocalLogin()
        {
            var found = _externalLogins.Where(x => x.Options.DenyLocalLogin).ToList();
            return found.Count > 0;
        }
    }

    public class BackOfficeExternalLoginProvider : IEquatable<BackOfficeExternalLoginProvider>
    {
        public BackOfficeExternalLoginProvider(string name, string authenticationType, BackOfficeExternalLoginProviderOptions properties)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            AuthenticationType = authenticationType ?? throw new ArgumentNullException(nameof(authenticationType));
            Options = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public string Name { get; }
        public string AuthenticationType { get; }
        public BackOfficeExternalLoginProviderOptions Options { get; }

        public override bool Equals(object obj)
        {
            return Equals(obj as BackOfficeExternalLoginProvider);
        }

        public bool Equals(BackOfficeExternalLoginProvider other)
        {
            return other != null &&
                   Name == other.Name &&
                   AuthenticationType == other.AuthenticationType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, AuthenticationType);
        }
    }

}
