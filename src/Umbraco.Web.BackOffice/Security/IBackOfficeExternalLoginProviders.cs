using Microsoft.AspNetCore.Authentication.OAuth;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Umbraco.Web.BackOffice.Security
{
    // TODO: This is only for the back office, does it need to be in common?

    // TODO: We need to implement this and extend it to support the back office external login options
    // basically migrate things from AuthenticationManagerExtensions & AuthenticationOptionsExtensions
    // and use this to get the back office external login infos
    public interface IBackOfficeExternalLoginProviders
    {
        /// <summary>
        /// Register a login provider for the back office
        /// </summary>
        /// <param name="provider"></param>
        void Register(BackOfficeExternalLoginProvider provider);

        BackOfficeExternalLoginProviderOptions Get(string authenticationType);

        IEnumerable<BackOfficeExternalLoginProvider> GetBackOfficeProviders();

        /// <summary>
        /// Returns the authentication type for the last registered external login (oauth) provider that specifies an auto-login redirect option
        /// </summary>
        /// <param name="manager"></param>
        /// <returns></returns>
        string GetAutoLoginProvider();

        bool HasDenyLocalLogin();
    }

    // TODO: This class is just a placeholder for later
    public class BackOfficeExternalLoginProviders : IBackOfficeExternalLoginProviders
    {
        private ConcurrentDictionary<string, BackOfficeExternalLoginProvider> _providers = new ConcurrentDictionary<string, BackOfficeExternalLoginProvider>();

        public void Register(BackOfficeExternalLoginProvider provider)
        {
            _providers.TryAdd(provider.AuthenticationType, provider);

            // TODO: we need to be able to set things like we were doing in ForUmbracoBackOffice.
            // Ok, most is done but we'll also need to take into account the callback path to ignore when we
            // do front-end routing
        }

        public BackOfficeExternalLoginProviderOptions Get(string authenticationType)
        {
            return _providers.TryGetValue(authenticationType, out var opt) ? opt.Options : null;
        }

        public string GetAutoLoginProvider()
        {
            var found = _providers.Where(x => x.Value.Options.AutoRedirectLoginToExternalProvider).ToList();
            return found.Count > 0 ? found[0].Key : null;
        }

        public IEnumerable<BackOfficeExternalLoginProvider> GetBackOfficeProviders()
        {
            return _providers.Values;
        }

        public bool HasDenyLocalLogin()
        {
            var found = _providers.Where(x => x.Value.Options.DenyLocalLogin).ToList();
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
