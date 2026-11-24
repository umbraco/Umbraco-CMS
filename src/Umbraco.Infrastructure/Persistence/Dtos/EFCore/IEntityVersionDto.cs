namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

/// <summary>
///     Defines the contract for a content kind's version row (e.g. <c>umbracoMediaVersion</c>), whose primary key
///     is shared with <see cref="ContentVersionDto" />.
/// </summary>
internal interface IEntityVersionDto
{
    /// <summary>Gets the primary key, shared with <see cref="ContentVersionDto.Id" />.</summary>
    int Id { get; }
}
