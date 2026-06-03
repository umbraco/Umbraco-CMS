namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

/// <summary>
///     Defines the contract for a type-specific content version row
///     (e.g. <c>umbracoDocumentVersion</c>, <c>umbracoMediaVersion</c>)
///     whose primary key is shared with <see cref="ContentVersionDto" />.
/// </summary>
internal interface IContentVersionDto
{
    /// <summary>Gets the primary key, shared with <see cref="ContentVersionDto.Id" />.</summary>
    int Id { get; }

    /// <summary>
    /// Gets the shared content version row containing version metadata (name, date, writer).
    /// Not a database column — populated by the repository after query.
    /// </summary>
    ContentVersionDto ContentVersionDto { get; }
}
