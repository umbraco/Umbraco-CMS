using Umbraco.Web.Routing;

namespace Umbraco.Tests.TestHelpers.Stubs
{
	internal class FakeLastChanceFinder : IContentLastChanceFinder
	{
		public bool TryFindContent(PublishedContentRequest frequest)
		{
			return false;
		}
	}
}