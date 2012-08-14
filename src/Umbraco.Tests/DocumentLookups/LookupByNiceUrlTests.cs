using System.Configuration;
using System.Web;
using NUnit.Framework;
using Umbraco.Web.Routing;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.template;

namespace Umbraco.Tests.DocumentLookups
{
	[TestFixture]
	public class LookupByNiceUrlTests : BaseRoutingTest
	{

		[TestCase("/")]
		[TestCase("/default.aspx")] //this one is actually rather important since this is the path that comes through when we are running in pre-IIS 7 for the root document '/' !
		[TestCase("/Sub1")]
		[TestCase("/sub1")]
		[TestCase("/sub1.aspx")]
		public void Match_Document_By_Url_Hide_Top_Level(string urlAsString)
		{
			var template = Template.MakeNew("test", new User(0));
			var routingContext = GetRoutingContext(urlAsString, template);
			var url = routingContext.UmbracoContext.UmbracoUrl; //very important to use the cleaned up umbraco url
			var docRequest = new DocumentRequest(url, routingContext);
			var lookup = new LookupByNiceUrl();
			Umbraco.Core.Configuration.GlobalSettings.HttpContext = routingContext.UmbracoContext.HttpContext;
			ConfigurationManager.AppSettings.Set("umbracoHideTopLevelNodeFromPath", "true");

			var result = lookup.TrySetDocument(docRequest);

			Assert.IsTrue(result);
		}

		[TestCase("/")]
		[TestCase("/default.aspx")] //this one is actually rather important since this is the path that comes through when we are running in pre-IIS 7 for the root document '/' !
		[TestCase("/home/Sub1")]
		[TestCase("/Home/Sub1")] //different cases
		[TestCase("/home/Sub1.aspx")]
		public void Match_Document_By_Url(string urlAsString)
		{
			var template = Template.MakeNew("test", new User(0));
			var routingContext = GetRoutingContext(urlAsString, template);
			var url = routingContext.UmbracoContext.UmbracoUrl;	//very important to use the cleaned up umbraco url		
			var docRequest = new DocumentRequest(url, routingContext);			
			var lookup = new LookupByNiceUrl();

			var result = lookup.TrySetDocument(docRequest);

			Assert.IsTrue(result);
		}

	}
}