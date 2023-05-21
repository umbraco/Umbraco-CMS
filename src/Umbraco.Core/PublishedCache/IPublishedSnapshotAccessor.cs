namespace Umbraco.Cms.Core.PublishedCache;

/// <summary>
///     Provides access to a TryGetPublishedSnapshot bool method that will return true if the "current"
///     <see cref="IPublishedSnapshot" /> is not null.
/// </summary>
public interface IPublishedSnapshotAccessor
{
    bool TryGetPublishedSnapshot(out IPublishedSnapshot? publishedSnapshot);
}
