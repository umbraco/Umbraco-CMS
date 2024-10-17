namespace Umbraco.Cms.Core.PublishedCache;

public interface IDatabaseCacheRebuilder
{
    void Rebuild();

    void RebuildDatabaseCacheIfSerializerChanged();
}
