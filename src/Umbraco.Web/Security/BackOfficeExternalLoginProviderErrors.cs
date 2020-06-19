using System.Collections.Generic;

namespace Umbraco.Web.Security
{
    public class BackOfficeExternalLoginProviderErrors
    {
        public BackOfficeExternalLoginProviderErrors(string authenticationType, IEnumerable<string> errors)
        {
            AuthenticationType = authenticationType;
            Errors = errors;
        }

        public string AuthenticationType { get; }
        public IEnumerable<string> Errors { get; }
    }
}
