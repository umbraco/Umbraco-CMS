using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Routing;
using Umbraco.Core.Models;
using Current = Umbraco.Web.Current;

namespace Umbraco.Tests.Routing
{
	[TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class ContentFinderByNiceUrlAndTemplateTests : BaseWebTest
    {
        Template CreateTemplate(string alias)
        {
            var template = new Template(alias, alias);
            template.Content = ""; // else saving throws with a dirty internal error
            Current.Services.FileService.SaveTemplate(template);
            return template;
        }

		[TestCase("/blah")]
		[TestCase("/default.aspx/blah")] //this one is actually rather important since this is the path that comes through when we are running in pre-IIS 7 for the root document '/' !
		[TestCase("/home/Sub1/blah")]
		[TestCase("/Home/Sub1/Blah")] //different cases
		[TestCase("/home/Sub1.aspx/blah")]
		public void Match_Document_By_Url_With_Template(string urlAsString)
		{
            var template1 = CreateTemplate("test");
            var template2 = CreateTemplate("blah");
            var umbracoContext = GetUmbracoContext(urlAsString, template1.Id);
		    var facadeRouter = CreateFacadeRouter();
			var frequest = facadeRouter.CreateRequest(umbracoContext);
            var lookup = new ContentFinderByNiceUrlAndTemplate(Logger);

		    SettingsForTests.HideTopLevelNodeFromPath = false;

			var result = lookup.TryFindContent(frequest);

			Assert.IsTrue(result);
			Assert.IsNotNull(frequest.PublishedContent);
			Assert.IsNotNull(frequest.TemplateAlias);
			Assert.AreEqual("blah".ToUpperInvariant(), frequest.TemplateAlias.ToUpperInvariant());
		}
	}
}