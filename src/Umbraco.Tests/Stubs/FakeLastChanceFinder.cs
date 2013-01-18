using Umbraco.Web.Routing;

namespace Umbraco.Tests.Stubs
{
	internal class FakeLastChanceFinder : IContentFinder
	{
		public bool TryFindDocument(PublishedContentRequest docRequest)
		{
			return false;
		}
	}
}