using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Web.BackOffice.Security
{
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

}
