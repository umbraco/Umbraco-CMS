using System;
using System.Globalization;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.Routing
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class ContentFinderByUrlTests : BaseWebTest
    {
        [TestCase("/", 1046)]
        [TestCase("/default.aspx", 1046)] //this one is actually rather important since this is the path that comes through when we are running in pre-IIS 7 for the root document '/' !
        [TestCase("/Sub1", 1173)]
        [TestCase("/sub1", 1173)]
        [TestCase("/sub1.aspx", 1173)]
        [TestCase("/home/sub1", -1)] // should fail

        // these two are special. getNiceUrl(1046) returns "/" but getNiceUrl(1172) cannot also return "/" so
        // we've made it return "/test-page" => we have to support that URL back in the lookup...
        [TestCase("/home", 1046)]
        [TestCase("/test-page", 1172)]
        public void Match_Document_By_Url_Hide_Top_Level(string urlString, int expectedId)
        {
            var globalSettingsMock = Mock.Get(Factory.GetInstance<IGlobalSettings>()); //this will modify the IGlobalSettings instance stored in the container
            globalSettingsMock.Setup(x => x.HideTopLevelNodeFromPath).Returns(true);

            var umbracoContext = GetUmbracoContext(urlString, globalSettings:globalSettingsMock.Object);
            var publishedRouter = CreatePublishedRouter();
            var frequest = publishedRouter.CreateRequest(umbracoContext);
            var lookup = new ContentFinderByUrl(Logger);

            Assert.IsTrue(Current.Configs.Global().HideTopLevelNodeFromPath);

            // FIXME: debugging - going further down, the routes cache is NOT empty?!
            if (urlString == "/home/sub1")
                System.Diagnostics.Debugger.Break();

            var result = lookup.TryFindContent(frequest);

            if (expectedId > 0)
            {
                Assert.IsTrue(result);
                Assert.AreEqual(expectedId, frequest.PublishedContent.Id);
            }
            else
            {
                Assert.IsFalse(result);
            }
        }

        [TestCase("/", 1046)]
        [TestCase("/default.aspx", 1046)] //this one is actually rather important since this is the path that comes through when we are running in pre-IIS 7 for the root document '/' !
        [TestCase("/home", 1046)]
        [TestCase("/home/Sub1", 1173)]
        [TestCase("/Home/Sub1", 1173)] //different cases
        [TestCase("/home/Sub1.aspx", 1173)]
        public void Match_Document_By_Url(string urlString, int expectedId)
        {
            var globalSettingsMock = Mock.Get(Factory.GetInstance<IGlobalSettings>()); //this will modify the IGlobalSettings instance stored in the container
            globalSettingsMock.Setup(x => x.HideTopLevelNodeFromPath).Returns(false);

            var umbracoContext = GetUmbracoContext(urlString, globalSettings:globalSettingsMock.Object);
            var publishedRouter = CreatePublishedRouter();
            var frequest = publishedRouter.CreateRequest(umbracoContext);
            var lookup = new ContentFinderByUrl(Logger);

            Assert.IsFalse(Current.Configs.Global().HideTopLevelNodeFromPath);

            var result = lookup.TryFindContent(frequest);

            Assert.IsTrue(result);
            Assert.AreEqual(expectedId, frequest.PublishedContent.Id);
        }
        /// <summary>
        /// This test handles requests with special characters in the URL.
        /// </summary>
        /// <param name="urlString"></param>
        /// <param name="expectedId"></param>
        [TestCase("/", 1046)]
        [TestCase("/home/sub1/custom-sub-3-with-accént-character", 1179)]
        [TestCase("/home/sub1/custom-sub-4-with-æøå", 1180)]
        public void Match_Document_By_Url_With_Special_Characters(string urlString, int expectedId)
        {
            var globalSettingsMock = Mock.Get(Factory.GetInstance<IGlobalSettings>()); //this will modify the IGlobalSettings instance stored in the container
            globalSettingsMock.Setup(x => x.HideTopLevelNodeFromPath).Returns(false);

            var umbracoContext = GetUmbracoContext(urlString, globalSettings:globalSettingsMock.Object);
            var publishedRouter = CreatePublishedRouter();
            var frequest = publishedRouter.CreateRequest(umbracoContext);
            var lookup = new ContentFinderByUrl(Logger);

            var result = lookup.TryFindContent(frequest);

            Assert.IsTrue(result);
            Assert.AreEqual(expectedId, frequest.PublishedContent.Id);
        }

        /// <summary>
        /// This test handles requests with a hostname associated.
        /// The logic for handling this goes through the DomainHelper and is a bit different
        /// from what happens in a normal request - so it has a separate test with a mocked
        /// hostname added.
        /// </summary>
        /// <param name="urlString"></param>
        /// <param name="expectedId"></param>
        [TestCase("/", 1046)]
        [TestCase("/home/sub1/custom-sub-3-with-accént-character", 1179)]
        [TestCase("/home/sub1/custom-sub-4-with-æøå", 1180)]
        public void Match_Document_By_Url_With_Special_Characters_Using_Hostname(string urlString, int expectedId)
        {
            var globalSettingsMock = Mock.Get(Factory.GetInstance<IGlobalSettings>()); //this will modify the IGlobalSettings instance stored in the container
            globalSettingsMock.Setup(x => x.HideTopLevelNodeFromPath).Returns(false);

            var umbracoContext = GetUmbracoContext(urlString, globalSettings:globalSettingsMock.Object);
            var publishedRouter = CreatePublishedRouter();
            var frequest = publishedRouter.CreateRequest(umbracoContext);
            frequest.Domain = new DomainAndUri(new Domain(1, "mysite", -1, CultureInfo.CurrentCulture, false), new Uri("http://mysite/"));
            var lookup = new ContentFinderByUrl(Logger);

            var result = lookup.TryFindContent(frequest);

            Assert.IsTrue(result);
            Assert.AreEqual(expectedId, frequest.PublishedContent.Id);
        }

        /// <summary>
        /// This test handles requests with a hostname with special characters associated.
        /// The logic for handling this goes through the DomainHelper and is a bit different
        /// from what happens in a normal request - so it has a separate test with a mocked
        /// hostname added.
        /// </summary>
        /// <param name="urlString"></param>
        /// <param name="expectedId"></param>
        [TestCase("/æøå/", 1046)]
        [TestCase("/æøå/home/sub1", 1173)]
        [TestCase("/æøå/home/sub1/custom-sub-3-with-accént-character", 1179)]
        [TestCase("/æøå/home/sub1/custom-sub-4-with-æøå", 1180)]
        public void Match_Document_By_Url_With_Special_Characters_In_Hostname(string urlString, int expectedId)
        {
            var globalSettingsMock = Mock.Get(Factory.GetInstance<IGlobalSettings>()); //this will modify the IGlobalSettings instance stored in the container
            globalSettingsMock.Setup(x => x.HideTopLevelNodeFromPath).Returns(false);

            var umbracoContext = GetUmbracoContext(urlString, globalSettings:globalSettingsMock.Object);
            var publishedRouter = CreatePublishedRouter();
            var frequest = publishedRouter.CreateRequest(umbracoContext);
            frequest.Domain = new DomainAndUri(new Domain(1, "mysite/æøå", -1, CultureInfo.CurrentCulture, false), new Uri("http://mysite/æøå"));
            var lookup = new ContentFinderByUrl(Logger);

            var result = lookup.TryFindContent(frequest);

            Assert.IsTrue(result);
            Assert.AreEqual(expectedId, frequest.PublishedContent.Id);
        }
    }
}
