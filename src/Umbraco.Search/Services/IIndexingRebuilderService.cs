namespace Umbraco.New.Cms.Infrastructure.Services;

public interface IIndexingRebuilderService
{
    bool CanRebuild(string indexName);
    bool TryRebuild(string index, string indexName);

    bool IsRebuilding(string indexName);
}
