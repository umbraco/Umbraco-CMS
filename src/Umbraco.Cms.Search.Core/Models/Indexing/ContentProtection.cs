namespace Umbraco.Cms.Search.Core.Models.Indexing;

/// <summary>
/// Represents the public-access protection on a content item, as the member and/or member group IDs allowed to see it.
/// </summary>
/// <param name="AccessIds">The member and member group IDs granted access to the protected content.</param>
public record ContentProtection(IEnumerable<Guid> AccessIds)
{
}
