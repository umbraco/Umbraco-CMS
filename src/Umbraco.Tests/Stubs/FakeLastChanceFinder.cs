using Umbraco.Web.Routing;

namespace Umbraco.Tests.Stubs
{
	internal class FakeLastChanceFinder : IContentFinder
	{
		public bool TryFindContent(PublishedContentRequest docRequest)
		{
			return false;
		}
	}
}