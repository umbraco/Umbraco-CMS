using Microsoft.AspNetCore.Identity;

namespace Umbraco.Infrastructure.Security
{

    /// <summary>
    /// No-op lookup normalizer to maintain compatibility with ASP.NET Identity 2
    /// </summary>
    public class NoOpLookupNormalizer : ILookupNormalizer
    {
        // TODO: Do we need this?

        public string NormalizeName(string name) => name;

        public string NormalizeEmail(string email) => email;
    }
}
