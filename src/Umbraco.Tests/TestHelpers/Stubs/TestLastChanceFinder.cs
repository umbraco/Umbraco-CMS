using Umbraco.Cms.Core.Routing;

namespace Umbraco.Tests.TestHelpers.Stubs
{
    internal class TestLastChanceFinder : IContentLastChanceFinder
    {
        public bool TryFindContent(IPublishedRequestBuilder frequest) => false;
    }
}
