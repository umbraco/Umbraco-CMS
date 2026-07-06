namespace Umbraco.Cms.Search.Core.Models.Indexing;

public record ContentProtection(IEnumerable<Guid> AccessIds)
{
}
