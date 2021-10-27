﻿using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Web.Security
{
    public class BackOfficeExternalLoginProviderErrors
    {
        // required for deserialization
        public BackOfficeExternalLoginProviderErrors()
        {
        }

        public BackOfficeExternalLoginProviderErrors(string authenticationType, IEnumerable<string> errors)
        {
            AuthenticationType = authenticationType;
            Errors = errors ?? Enumerable.Empty<string>();
        }

        public string AuthenticationType { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }
}
