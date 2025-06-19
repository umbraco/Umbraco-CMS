namespace Umbraco.Cms.Infrastructure.Examine;

internal sealed class NoopIndexRebuilder : IIndexRebuilder
{
    public bool CanRebuild(string indexName) => false;

    public void RebuildIndex(string indexName, TimeSpan? delay = null, bool useBackgroundThread = true) {}

    public void RebuildIndexes(bool onlyEmptyIndexes, TimeSpan? delay = null, bool useBackgroundThread = true) {}
}
