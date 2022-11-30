using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     A service for handling lawful data processing requirements
/// </summary>
/// <remarks>
///     <para>
///         Consent can be given or revoked or changed via the <see cref="RegisterConsent" /> method, which
///         creates a new <see cref="IConsent" /> entity to track the consent. Revoking a consent is performed by
///         registering a revoked consent.
///     </para>
///     <para>A consent can be revoked, by registering a revoked consent, but cannot be deleted.</para>
///     <para>
///         Getter methods return the current state of a consent, i.e. the latest <see cref="IConsent" />
///         entity that was created.
///     </para>
/// </remarks>
public interface IConsentService : IService
{
    /// <summary>
    ///     Registers consent.
    /// </summary>
    /// <param name="source">The source, i.e. whoever is consenting.</param>
    /// <param name="context"></param>
    /// <param name="action"></param>
    /// <param name="state">The state of the consent.</param>
    /// <param name="comment">Additional free text.</param>
    /// <returns>The corresponding consent entity.</returns>
    IConsent RegisterConsent(string source, string context, string action, ConsentState state, string? comment = null);

    /// <summary>
    ///     Retrieves consents.
    /// </summary>
    /// <param name="source">The optional source.</param>
    /// <param name="context">The optional context.</param>
    /// <param name="action">The optional action.</param>
    /// <param name="sourceStartsWith">Determines whether <paramref name="source" /> is a start pattern.</param>
    /// <param name="contextStartsWith">Determines whether <paramref name="context" /> is a start pattern.</param>
    /// <param name="actionStartsWith">Determines whether <paramref name="action" /> is a start pattern.</param>
    /// <param name="includeHistory">Determines whether to include the history of consents.</param>
    /// <returns>Consents matching the parameters.</returns>
    IEnumerable<IConsent> LookupConsent(
        string? source = null,
        string? context = null,
        string? action = null,
        bool sourceStartsWith = false,
        bool contextStartsWith = false,
        bool actionStartsWith = false,
        bool includeHistory = false);
}
