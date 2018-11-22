using Moq;
using NUnit.Framework;
using LightInject;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Routing;
using Umbraco.Core.Models;
using Umbraco.Tests.Testing;
using Current = Umbraco.Web.Composing.Current;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Tests.Routing
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class ContentFinderByUrlAndTemplateTests : BaseWebTest
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

            var globalSettings = Mock.Get(TestObjects.GetGlobalSettings()); //this will modify the IGlobalSettings instance stored in the container
            globalSettings.Setup(x => x.HideTopLevelNodeFromPath).Returns(false);
            SettingsForTests.ConfigureSettings(globalSettings.Object);

            var template1 = CreateTemplate("test");
            var template2 = CreateTemplate("blah");
            var umbracoContext = GetUmbracoContext(urlAsString, template1.Id, globalSettings:globalSettings.Object);
            var publishedRouter = CreatePublishedRouter();
            var frequest = publishedRouter.CreateRequest(umbracoContext);
            var lookup = new ContentFinderByUrlAndTemplate(Logger, ServiceContext.FileService);

            var result = lookup.TryFindContent(frequest);

            Assert.IsTrue(result);
            Assert.IsNotNull(frequest.PublishedContent);
            Assert.IsNotNull(frequest.TemplateAlias);
            Assert.AreEqual("blah".ToUpperInvariant(), frequest.TemplateAlias.ToUpperInvariant());
        }
    }
}
