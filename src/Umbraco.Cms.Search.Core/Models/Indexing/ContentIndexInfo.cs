using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Services;

namespace Umbraco.Cms.Search.Core.Models.Indexing;

public record ContentIndexInfo(string IndexAlias, IEnumerable<UmbracoObjectTypes> ContainedObjectTypes, IIndexer Indexer);
