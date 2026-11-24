namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

/// <summary>
///     Defines the contract for a publishable content kind's version row
///     (e.g. <c>umbracoDocumentVersion</c>, <c>umbracoElementVersion</c>): the shared version id plus the
///     published flag.
/// </summary>
internal interface IPublishableVersionDto : IEntityVersionDto
{
    /// <summary>Gets a value indicating whether this version is the published one.</summary>
    bool Published { get; }
}
