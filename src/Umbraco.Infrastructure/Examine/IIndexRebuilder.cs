namespace Umbraco.Cms.Infrastructure.Examine;

public interface IIndexRebuilder
{
    bool CanRebuild(string indexName);

    void RebuildIndex(string indexName, TimeSpan? delay = null, bool useBackgroundThread = true);

    void RebuildIndexes(bool onlyEmptyIndexes, TimeSpan? delay = null, bool useBackgroundThread = true);
}
