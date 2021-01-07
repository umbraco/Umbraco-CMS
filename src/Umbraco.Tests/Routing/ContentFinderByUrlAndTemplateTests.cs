using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Routing;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Models;
using Umbraco.Tests.Testing;
using System.Threading.Tasks;

namespace Umbraco.Tests.Routing
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class ContentFinderByUrlAndTemplateTests : BaseWebTest
    {
        Template CreateTemplate(string alias)
        {
            var template = new Template(ShortStringHelper, alias, alias);
            template.Content = ""; // else saving throws with a dirty internal error
            ServiceContext.FileService.SaveTemplate(template);
            return template;
        }

        [TestCase("/blah")]
        [TestCase("/default.aspx/blah")] //this one is actually rather important since this is the path that comes through when we are running in pre-IIS 7 for the root document '/' !
        [TestCase("/home/Sub1/blah")]
        [TestCase("/Home/Sub1/Blah")] //different cases
        [TestCase("/home/Sub1.aspx/blah")]
        public async Task Match_Document_By_Url_With_Template(string urlAsString)
        {
            var globalSettings = new GlobalSettings { HideTopLevelNodeFromPath = false };

            var template1 = CreateTemplate("test");
            var template2 = CreateTemplate("blah");
            var umbracoContext = GetUmbracoContext(urlAsString, template1.Id, globalSettings: globalSettings);
            var publishedRouter = CreatePublishedRouter(GetUmbracoContextAccessor(umbracoContext));
            var reqBuilder = await publishedRouter.CreateRequestAsync(umbracoContext.CleanedUmbracoUrl);
            var webRoutingSettings = new WebRoutingSettings();
            var lookup = new ContentFinderByUrlAndTemplate(
                LoggerFactory.CreateLogger<ContentFinderByUrlAndTemplate>(),
                ServiceContext.FileService,
                ServiceContext.ContentTypeService,
                GetUmbracoContextAccessor(umbracoContext),
                Microsoft.Extensions.Options.Options.Create(webRoutingSettings));

            var result = lookup.TryFindContent(reqBuilder);

            IPublishedRequest frequest = reqBuilder.Build();

            Assert.IsTrue(result);
            Assert.IsNotNull(frequest.PublishedContent);
            Assert.IsNotNull(frequest.GetTemplateAlias());
            Assert.AreEqual("blah".ToUpperInvariant(), frequest.GetTemplateAlias().ToUpperInvariant());
        }
    }
}
