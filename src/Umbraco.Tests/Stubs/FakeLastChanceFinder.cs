using Umbraco.Web.Routing;

namespace Umbraco.Tests.Stubs
{
	internal class FakeLastChanceFinder : IPublishedContentFinder
	{
		public bool TryFindDocument(PublishedContentRequest docRequest)
		{
			return false;
		}
	}
}