using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.Models.Persistence;

public class IndexDocument
{
    public required Guid Key { get; init; }

    public required IndexField[] Fields { get; init; }

    public required bool Published { get; init; }
}
