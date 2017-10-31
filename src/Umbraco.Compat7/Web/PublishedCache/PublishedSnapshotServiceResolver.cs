using WebCurrent = Umbraco.Web.Composing.Current;

// ReSharper disable once CheckNamespace
namespace Umbraco.Web.PublishedCache
{
    public class PublishedSnapshotServiceResolver
    {
        private PublishedSnapshotServiceResolver()
        { }

        public static PublishedSnapshotServiceResolver Current { get; } = new PublishedSnapshotServiceResolver();

        public IPublishedSnapshotService Service => WebCurrent.PublishedSnapshotService;
    }
}
