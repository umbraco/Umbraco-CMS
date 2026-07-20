using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Search.Core.Models.Configuration;

public record ContentIndexRegistration(
    string IndexAlias,
    Type Indexer,
    Type Searcher,
    Type ContentChangeStrategy,
    IEnumerable<UmbracoObjectTypes> ContainedObjectTypes,
    bool SameOriginOnly)
    : IndexRegistration(IndexAlias, Indexer, Searcher)
{
}
