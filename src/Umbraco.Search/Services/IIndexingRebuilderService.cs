namespace Umbraco.Search.Services;
public interface IIndexingRebuilderService
{
    bool CanRebuild(string indexName);
    bool TryRebuild(string index);

    bool IsRebuilding(string indexName);
}
