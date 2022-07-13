using Microsoft.AspNetCore.Identity;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     No-op lookup normalizer to maintain compatibility with ASP.NET Identity 2
/// </summary>
public class NoopLookupNormalizer : ILookupNormalizer
{
    public string NormalizeName(string name) => name;

    public string NormalizeEmail(string email) => email;
}
