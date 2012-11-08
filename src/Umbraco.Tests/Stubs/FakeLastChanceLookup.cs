using Umbraco.Web.Routing;

namespace Umbraco.Tests.Stubs
{
	internal class FakeLastChanceLookup : IDocumentLastChanceLookup
	{
		public bool TrySetDocument(PublishedContentRequest docRequest)
		{
			return false;
		}
	}
}