namespace Umbraco.Web.PublishedCache
{
    public interface IPublishedMediaCache2 : IPublishedMediaCache, IPublishedCache2
    {
        // NOTE: this is here purely to avoid API breaking changes
    }

    public interface IPublishedMediaCache : IPublishedCache
    { }
}
