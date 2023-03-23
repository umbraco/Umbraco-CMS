using Examine;

namespace Umbraco.New.Cms.Infrastructure.Services;

public interface IIndexingRebuilderService
{
    bool CanRebuild(string indexName);
    bool TryRebuild(IIndex index, string indexName);

    bool IsRebuilding(string indexName);
}
