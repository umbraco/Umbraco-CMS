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

    /// <summary>Gets a value indicating whether this version is the published one.</summary>
    bool Published { get; }
}
