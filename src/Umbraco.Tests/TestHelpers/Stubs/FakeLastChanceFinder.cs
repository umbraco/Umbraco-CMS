using Umbraco.Web.Routing;

namespace Umbraco.Tests.TestHelpers.Stubs
{
	internal class FakeLastChanceFinder : IContentFinder
	{
		public bool TryFindContent(PublishedContentRequest docRequest)
		{
			return false;
		}
	}
}