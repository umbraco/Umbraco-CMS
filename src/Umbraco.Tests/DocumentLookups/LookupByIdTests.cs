using NUnit.Framework;
using Umbraco.Web.Routing;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.template;

namespace Umbraco.Tests.DocumentLookups
{
	[TestFixture]
	public class LookupByIdTests : BaseRoutingTest
	{
		[TestCase("/1046", 1046)]
		[TestCase("/1046.aspx", 1046)]		
		public void Lookup_By_Id(string urlAsString, int nodeMatch)
		{
			var template = Template.MakeNew("test", new User(0));
			var routingContext = GetRoutingContext(urlAsString, template);
			var url = routingContext.UmbracoContext.UmbracoUrl; //very important to use the cleaned up umbraco url
			var docRequest = new DocumentRequest(url, routingContext);
			var lookup = new LookupById();
			Umbraco.Core.Configuration.GlobalSettings.HttpContext = routingContext.UmbracoContext.HttpContext;

			var result = lookup.TrySetDocument(docRequest);

			Assert.IsTrue(result);
			Assert.AreEqual(docRequest.NodeId, nodeMatch);
		}
	}
}