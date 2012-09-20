using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Routing;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.template;

namespace Umbraco.Tests.Routing
{
	[TestFixture]
	public class LookupByIdTests : BaseRoutingTest
	{
		/// <summary>
		/// We don't need a db for this test, will run faster without one
		/// </summary>
		protected override bool RequiresDbSetup
		{
			get { return false; }
		}

		[TestCase("/1046", 1046)]
		[TestCase("/1046.aspx", 1046)]		
		public void Lookup_By_Id(string urlAsString, int nodeMatch)
		{
			var routingContext = GetRoutingContext(urlAsString);
			var url = routingContext.UmbracoContext.UmbracoUrl; //very important to use the cleaned up umbraco url
			var docRequest = new DocumentRequest(url, routingContext);
			var lookup = new LookupByIdPath();
		

			var result = lookup.TrySetDocument(docRequest);

			Assert.IsTrue(result);
			Assert.AreEqual(docRequest.DocumentId, nodeMatch);
		}
	}
}