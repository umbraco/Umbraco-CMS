using Umbraco.Web.Routing;

namespace Umbraco.Tests.Stubs
{
	internal class FakeLastChanceLookup : IDocumentLastChanceLookup
	{
		public bool TrySetDocument(DocumentRequest docRequest)
		{
			return false;
		}
	}
}