namespace Umbraco.Web.PublishedCache
{
    /// <summary>
    /// Provides access to the "current" <see cref="IPublishedSnapshot"/>.
    /// </summary>
    public interface IPublishedSnapshotAccessor
    {
        IPublishedSnapshot PublishedSnapshot { get; set; }
    }
}
