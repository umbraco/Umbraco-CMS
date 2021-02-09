namespace Umbraco.Cms.Core.PublishedCache
{
    /// <summary>
    /// Provides access to the "current" <see cref="IPublishedSnapshot"/>.
    /// </summary>
    public interface IPublishedSnapshotAccessor
    {
        IPublishedSnapshot PublishedSnapshot { get; set; }
    }
}
