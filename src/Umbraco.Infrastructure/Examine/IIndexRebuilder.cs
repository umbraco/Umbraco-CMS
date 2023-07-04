namespace Umbraco.Cms.Infrastructure.Examine;
[Obsolete("This class will be removed in v14, please check documentation of specific search provider", true)]

public interface IIndexRebuilder
{
    bool CanRebuild(string indexName);

    void RebuildIndex(string indexName, TimeSpan? delay = null, bool useBackgroundThread = true);

    void RebuildIndexes(bool onlyEmptyIndexes, TimeSpan? delay = null, bool useBackgroundThread = true);
}
