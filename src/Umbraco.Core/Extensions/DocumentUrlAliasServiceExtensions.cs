using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IDocumentUrlAliasService"/>.
/// </summary>
internal static class DocumentUrlAliasServiceExtensions
{
    /// <summary>
    /// Normalizes a URL alias by trimming whitespace, removing leading/trailing slashes, and converting to lowercase.
    /// </summary>
    /// <param name="service">The <see cref="IDocumentUrlAliasService"/>.</param>
    /// <param name="alias">The alias to normalize.</param>
    /// <returns>The normalized alias.</returns>
    public static string NormalizeAlias(this IDocumentUrlAliasService service, string alias) =>
        alias
            .Trim()
            .TrimStart('/')
            .TrimEnd('/')
            .ToLowerInvariant();
}
