using Umbraco.Web.Routing;

namespace Umbraco.Tests.TestHelpers.Stubs
{
    internal class TestLastChanceFinder : IContentLastChanceFinder
    {
        public bool TryFindContent(PublishedRequest frequest)
        {
            return false;
        }
    }
}
