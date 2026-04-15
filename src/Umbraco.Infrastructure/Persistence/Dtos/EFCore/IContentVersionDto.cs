namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

/// <summary>
///     Defines the contract for a type-specific content version row
///     (e.g. <c>umbracoDocumentVersion</c>, <c>umbracoMediaVersion</c>)
///     whose primary key is shared with <see cref="ContentVersionDto" />.
/// </summary>
/// <remarks>
///     TODO: Extend this interface with additional shared properties as the EF Core
///     repository layer is built out.
/// </remarks>
internal interface IContentVersionDto
{
    /// <summary>Gets the primary key, shared with <see cref="ContentVersionDto.Id" />.</summary>
    int Id { get; }
}
