using Umbraco.Web.PublishedCache;

namespace Umbraco.Tests.Testing.Objects.Accessors
{
    public class TestPublishedSnapshotAccessor : IPublishedSnapshotAccessor
    {
        public IPublishedSnapshot PublishedSnapshot { get; set; }
    }
}
