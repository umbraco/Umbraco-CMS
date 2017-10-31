namespace Umbraco.Web.PublishedCache
{
    /// <summary>
    /// Provides access to the "current" <see cref="IPublishedShapshot"/>.
    /// </summary>
    public interface IPublishedSnapshotAccessor
    {
        IPublishedShapshot PublishedSnapshot { get; set; }
    }
}
