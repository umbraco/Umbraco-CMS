namespace Umbraco.Cms.Core.Models.ContentEditing;

/// <summary>
///     Represents the result of a domain update operation.
/// </summary>
public sealed class DomainUpdateResult
{
    /// <summary>
    ///     Gets the collection of domains that were successfully updated.
    /// </summary>
    public IEnumerable<IDomain> Domains { get; init; } = Enumerable.Empty<IDomain>();

    /// <summary>
    ///     Gets the collection of domains that conflicted with existing domains, or <c>null</c> if there were no conflicts.
    /// </summary>
    public IEnumerable<IDomain>? ConflictingDomains { get; init; }
}
