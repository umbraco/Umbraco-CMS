namespace Umbraco.Cms.Core.PublishedCache
{
    /// <summary>
    /// Provides access to the "current" <see cref="IPublishedSnapshot"/>.
    /// </summary>
    public interface IPublishedSnapshotAccessor
    {
        bool TryGetPublishedSnapshot(out IPublishedSnapshot publishedSnapshot);
    }
}
