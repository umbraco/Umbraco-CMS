using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Routing;
using umbraco.BusinessLogic;
using Umbraco.Core.Models;

namespace Umbraco.Tests.Routing
{
	[TestFixture]
	public class ContentFinderByNiceUrlAndTemplateTests : BaseRoutingTest
	{
		public override void Initialize()
		{
			base.Initialize();
			Umbraco.Core.Configuration.UmbracoSettings.UseLegacyXmlSchema = false;
		}

		[TestCase("/blah")]
		[TestCase("/default.aspx/blah")] //this one is actually rather important since this is the path that comes through when we are running in pre-IIS 7 for the root document '/' !
		[TestCase("/home/Sub1/blah")]
		[TestCase("/Home/Sub1/Blah")] //different cases
		[TestCase("/home/Sub1.aspx/blah")]
		public void Match_Document_By_Url_With_Template(string urlAsString)
		{
			var template = new Template("test");
            ApplicationContext.Services.FileService.SaveTemplate(template);
			var altTemplate = new Template("blah");
            ApplicationContext.Services.FileService.SaveTemplate(altTemplate);
			var routingContext = GetRoutingContext(urlAsString, template);
			var url = routingContext.UmbracoContext.CleanedUmbracoUrl; //very important to use the cleaned up umbraco url
			var docRequest = new PublishedContentRequest(url, routingContext);
			var lookup = new ContentFinderByNiceUrlAndTemplate();

			var result = lookup.TryFindDocument(docRequest);

			Assert.IsTrue(result);
			Assert.IsNotNull(docRequest.PublishedContent);
			Assert.IsNotNull(docRequest.TemplateAlias);
			Assert.AreEqual("blah".ToUpperInvariant(), docRequest.TemplateAlias.ToUpperInvariant());
		}
	}
}