using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Umbraco.Core.Security
{

    /// <summary>
    /// A custom user identity for the Umbraco backoffice
    /// </summary>
    [Serializable]
    public class UmbracoBackOfficeIdentity : ClaimsIdentity
    {
    }
}
