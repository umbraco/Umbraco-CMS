using Microsoft.AspNetCore.Identity;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     No-op lookup normalizer to maintain compatibility with ASP.NET Identity 2
/// </summary>
public class NoopLookupNormalizer : ILookupNormalizer
{
    /// <summary>
    /// Returns the input name without any normalization.
    /// </summary>
    /// <param name="name">The name to normalize.</param>
    /// <returns>The original name unchanged.</returns>
    public string? NormalizeName(string? name) => name;

    /// <summary>
    /// Returns the specified email address without performing any normalization.
    /// </summary>
    /// <param name="email">The email address to return.</param>
    /// <returns>The original, unmodified email address.</returns>
    public string? NormalizeEmail(string? email) => email;
}
