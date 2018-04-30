using Umbraco.Web.PublishedCache;

namespace Umbraco.Tests.TestHelpers.Stubs
{
    public class TestPublishedSnapshotAccessor : IPublishedSnapshotAccessor
    {
        public IPublishedSnapshot PublishedSnapshot { get; set; }
    }
}
