using System;

namespace Umbraco.Core.Models.Identity
{
    /// <inheritdoc />
    public class ExternalLogin : IExternalLogin
    {
        public ExternalLogin(string loginProvider, string providerKey, string userData = null)
        {
            LoginProvider = loginProvider ?? throw new ArgumentNullException(nameof(loginProvider));
            ProviderKey = providerKey ?? throw new ArgumentNullException(nameof(providerKey));
            UserData = userData;
        }

        /// <inheritdoc />
        public string LoginProvider { get; }

        /// <inheritdoc />
        public string ProviderKey { get;  }

        /// <inheritdoc />
        public string UserData { get; }
    }
}
