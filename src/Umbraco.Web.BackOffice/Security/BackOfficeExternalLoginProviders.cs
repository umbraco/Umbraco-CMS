using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Cms.Web.BackOffice.Security
{
    /// <inheritdoc />
    public class BackOfficeExternalLoginProviders : IBackOfficeExternalLoginProviders
    {
        public BackOfficeExternalLoginProviders(IEnumerable<BackOfficeExternalLoginProvider> externalLogins)
        {
            _externalLogins = externalLogins;
        }

        private readonly IEnumerable<BackOfficeExternalLoginProvider> _externalLogins;

        /// <inheritdoc />
        public BackOfficeExternalLoginProvider Get(string authenticationType)
        {
            return _externalLogins.FirstOrDefault(x => x.AuthenticationType == authenticationType);
        }

        /// <inheritdoc />
        public string GetAutoLoginProvider()
        {
            var found = _externalLogins.Where(x => x.Options.AutoRedirectLoginToExternalProvider).ToList();
            return found.Count > 0 ? found[0].AuthenticationType : null;
        }

        /// <inheritdoc />
        public IEnumerable<BackOfficeExternalLoginProvider> GetBackOfficeProviders()
        {
            return _externalLogins;
        }

        /// <inheritdoc />
        public bool HasDenyLocalLogin()
        {
            var found = _externalLogins.Where(x => x.Options.DenyLocalLogin).ToList();
            return found.Count > 0;
        }
    }

}
