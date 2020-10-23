using System;
using System.Collections.Generic;
using System.Text;

namespace Umbraco.Web.Common.Security
{
    // TODO: We need to implement this and extend it to support the back office external login options
    // basically migrate things from AuthenticationManagerExtensions & AuthenticationOptionsExtensions
    // and use this to get the back office external login infos
    public interface IBackOfficeExternalLoginProviders
    {
        ExternalSignInAutoLinkOptions Get(string authenticationType);

        IEnumerable<BackOfficeExternalLoginProvider> GetBackOfficeProviders();

        /// <summary>
        /// Returns the authentication type for the last registered external login (oauth) provider that specifies an auto-login redirect option
        /// </summary>
        /// <param name="manager"></param>
        /// <returns></returns>
        string GetAutoLoginProvider();

        bool HasDenyLocalLogin();
    }

    // TODO: we'll need to register these somehow
    public class BackOfficeExternalLoginProvider
    {
        public string Name { get; set; }
        public string AuthenticationType { get; set; }

        // TODO: I believe this should be replaced with just a reference to BackOfficeExternalLoginProviderOptions
        public IReadOnlyDictionary<string, object> Properties { get; set; }
    }

}
