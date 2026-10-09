using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Search.Querying;

/// <summary>
/// Represents a single search hit.
/// </summary>
/// <param name="Id">The key of the matched item.</param>
/// <param name="ObjectType">The entity type of the matched item.</param>
public record Document(Guid Id, UmbracoObjectTypes ObjectType)
{
}
