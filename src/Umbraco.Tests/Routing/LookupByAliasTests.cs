using NUnit.Framework;
using Umbraco.Web.Routing;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.template;

namespace Umbraco.Tests.Routing
{
	[TestFixture]
	public class LookupByAliasTests : BaseRoutingTest
	{
		[TestCase("/this/is/my/alias", 1046)]
		[TestCase("/anotheralias", 1046)]
		[TestCase("/page2/alias", 1173)]
		[TestCase("/2ndpagealias", 1173)]
		[TestCase("/only/one/alias", 1174)]
		[TestCase("/ONLY/one/Alias", 1174)]
		public void Lookup_By_Url_Alias(string urlAsString, int nodeMatch)
		{
			var template = Template.MakeNew("test", new User(0));
			var routingContext = GetRoutingContext(urlAsString, template);
			var url = routingContext.UmbracoContext.UmbracoUrl; //very important to use the cleaned up umbraco url
			var docRequest = new DocumentRequest(url, routingContext);
			var lookup = new LookupByAlias();
			Umbraco.Core.Configuration.GlobalSettings.HttpContext = routingContext.UmbracoContext.HttpContext;
			
			var result = lookup.TrySetDocument(docRequest);

			Assert.IsTrue(result);
			Assert.AreEqual(docRequest.DocumentId, nodeMatch);
		}
	}
}