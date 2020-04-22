using System;
using Microsoft.AspNetCore.Identity;

namespace Umbraco.Web.Security
{
    /// <summary>
    /// No-op lookup normalizer to maintain compatibility with ASP.NET Identity 2
    /// </summary>
    public class NopLookupNormalizer : ILookupNormalizer
    {
        public string NormalizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            return name;
        }

        public string NormalizeEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentNullException(nameof(email));
            return email;
        }
    }
}
