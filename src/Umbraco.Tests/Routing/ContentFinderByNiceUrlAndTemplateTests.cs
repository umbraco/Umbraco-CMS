using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Routing;
using umbraco.BusinessLogic;
using Umbraco.Core.Models;

namespace Umbraco.Tests.Routing
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerFixture)]
	[TestFixture]
	public class ContentFinderByNiceUrlAndTemplateTests : BaseRoutingTest
	{
        Template CreateTemplate(string alias)
        {
            var template = new Template(alias, alias, alias);
            template.Content = ""; // else saving throws with a dirty internal error
            ApplicationContext.Services.FileService.SaveTemplate(template);
            return template;
        }

		[TestCase("/blah")]
		[TestCase("/default.aspx/blah")] //this one is actually rather important since this is the path that comes through when we are running in pre-IIS 7 for the root document '/' !
		[TestCase("/home/Sub1/blah")]
		[TestCase("/Home/Sub1/Blah")] //different cases
		[TestCase("/home/Sub1.aspx/blah")]
		public void Match_Document_By_Url_With_Template(string urlAsString)
		{
            var template = CreateTemplate("test");
            var altTemplate = CreateTemplate("blah");
			var routingContext = GetRoutingContext(urlAsString, template);
			var url = routingContext.UmbracoContext.CleanedUmbracoUrl; //very important to use the cleaned up umbraco url
			var docRequest = new PublishedContentRequest(url, routingContext);
			var lookup = new ContentFinderByNiceUrlAndTemplate();

		    SettingsForTests.HideTopLevelNodeFromPath = false;

			var result = lookup.TryFindContent(docRequest);

			Assert.IsTrue(result);
			Assert.IsNotNull(docRequest.PublishedContent);
			Assert.IsNotNull(docRequest.TemplateAlias);
			Assert.AreEqual("blah".ToUpperInvariant(), docRequest.TemplateAlias.ToUpperInvariant());
		}
	}
}