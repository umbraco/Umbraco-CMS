namespace Umbraco.Cms.Core.Models.ContentEditing;

public sealed class DomainUpdateResult
{
    public IEnumerable<IDomain> Domains { get; init; } = Enumerable.Empty<IDomain>();

    public IEnumerable<IDomain>? ConflictingDomains { get; init; }
}
