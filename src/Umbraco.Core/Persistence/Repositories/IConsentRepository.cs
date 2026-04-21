using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="IConsent" /> entities.
/// </summary>
public interface IConsentRepository
{
    /// <summary>
    ///     Clears the current flag.
    /// </summary>
    Task ClearCurrentAsync(string source, string context, string action);

    /// <summary>
    ///     Looks up consent entries matching the specified filters.
    /// </summary>
    /// <param name="source">The optional source filter.</param>
    /// <param name="context">The optional context filter.</param>
    /// <param name="action">The optional action filter.</param>
    /// <param name="sourceStartsWith">Whether <paramref name="source"/> is a starts-with pattern.</param>
    /// <param name="contextStartsWith">Whether <paramref name="context"/> is a starts-with pattern.</param>
    /// <param name="actionStartsWith">Whether <paramref name="action"/> is a starts-with pattern.</param>
    /// <param name="includeHistory">Whether to include historical (non-current) consent entries.</param>
    /// <returns>The matching consent entries, grouped with history.</returns>
    Task<IEnumerable<IConsent>> LookupAsync(
        string? source = null,
        string? context = null,
        string? action = null,
        bool sourceStartsWith = false,
        bool contextStartsWith = false,
        bool actionStartsWith = false,
        bool includeHistory = false);

    Task SaveAsync(IConsent consent, CancellationToken cancellationToken);
}
