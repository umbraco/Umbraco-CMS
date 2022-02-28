using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Tests.Common.Published;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Extensions;


namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing
{
    [TestFixture]
    public class ContentFinderByUrlTests : PublishedSnapshotServiceTestBase
    {
        private async Task<(ContentFinderByUrl finder, IPublishedRequestBuilder frequest)> GetContentFinder(string urlString)
        {
            string xml = PublishedContentXml.BaseWebTestXml(1234);

            IEnumerable<ContentNodeKit> kits = PublishedContentXmlAdapter.GetContentNodeKits(
                xml,
                TestHelper.ShortStringHelper,
                out ContentType[] contentTypes,
                out DataType[] dataTypes).ToList();

            InitializedCache(kits, contentTypes, dataTypes: dataTypes);

            var umbracoContextAccessor = GetUmbracoContextAccessor(urlString);
            var umbracoContext = umbracoContextAccessor.GetRequiredUmbracoContext();
            var publishedRouter = CreatePublishedRouter(umbracoContextAccessor);
            var frequest = await publishedRouter.CreateRequestAsync(umbracoContext.CleanedUmbracoUrl);
            var lookup = new ContentFinderByUrl(Mock.Of<ILogger<ContentFinderByUrl>>(), umbracoContextAccessor);
            return (lookup, frequest);
        }

        [TestCase("/", 1046)]
        [TestCase("/Sub1", 1173)]
        [TestCase("/sub1", 1173)]
        [TestCase("/home/sub1", -1)] // should fail

        // these two are special. getNiceUrl(1046) returns "/" but getNiceUrl(1172) cannot also return "/" so
        // we've made it return "/test-page" => we have to support that URL back in the lookup...
        [TestCase("/home", 1046)]
        [TestCase("/test-page", 1172)]
        public async Task Match_Document_By_Url_Hide_Top_Level(string urlString, int expectedId)
        {
            GlobalSettings.HideTopLevelNodeFromPath = true;

            var lookup = await GetContentFinder(urlString);

            Assert.IsTrue(GlobalSettings.HideTopLevelNodeFromPath);

            // FIXME: debugging - going further down, the routes cache is NOT empty?!
            if (urlString == "/home/sub1")
                System.Diagnostics.Debugger.Break();

            var result = lookup.finder.TryFindContent(lookup.frequest);

            if (expectedId > 0)
            {
                Assert.IsTrue(result);
                Assert.AreEqual(expectedId, lookup.frequest.PublishedContent.Id);
            }
            else
            {
                Assert.IsFalse(result);
            }
        }

        [TestCase("/", 1046)]
        [TestCase("/home", 1046)]
        [TestCase("/home/Sub1", 1173)]
        [TestCase("/Home/Sub1", 1173)] //different cases
        public async Task Match_Document_By_Url(string urlString, int expectedId)
        {
            GlobalSettings.HideTopLevelNodeFromPath = false;

            var lookup = await GetContentFinder(urlString);

            Assert.IsFalse(GlobalSettings.HideTopLevelNodeFromPath);

            var result = lookup.finder.TryFindContent(lookup.frequest);

            Assert.IsTrue(result);
            Assert.AreEqual(expectedId, lookup.frequest.PublishedContent.Id);
        }
        /// <summary>
        /// This test handles requests with special characters in the URL.
        /// </summary>
        /// <param name="urlString"></param>
        /// <param name="expectedId"></param>
        [TestCase("/", 1046)]
        [TestCase("/home/sub1/custom-sub-3-with-accént-character", 1179)]
        [TestCase("/home/sub1/custom-sub-4-with-æøå", 1180)]
        public async Task Match_Document_By_Url_With_Special_Characters(string urlString, int expectedId)
        {
            GlobalSettings.HideTopLevelNodeFromPath = false;

            var lookup = await GetContentFinder(urlString);

            var result = lookup.finder.TryFindContent(lookup.frequest);

            Assert.IsTrue(result);
            Assert.AreEqual(expectedId, lookup.frequest.PublishedContent.Id);
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
        public async Task Match_Document_By_Url_With_Special_Characters_Using_Hostname(string urlString, int expectedId)
        {
            GlobalSettings.HideTopLevelNodeFromPath = false;

            var lookup = await GetContentFinder(urlString);

            lookup.frequest.SetDomain(new DomainAndUri(new Domain(1, "mysite", -1, "en-US", false), new Uri("http://mysite/")));

            var result = lookup.finder.TryFindContent(lookup.frequest);

            Assert.IsTrue(result);
            Assert.AreEqual(expectedId, lookup.frequest.PublishedContent.Id);
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
        public async Task Match_Document_By_Url_With_Special_Characters_In_Hostname(string urlString, int expectedId)
        {
            GlobalSettings.HideTopLevelNodeFromPath = false;

            var lookup = await GetContentFinder(urlString);

            lookup.frequest.SetDomain(new DomainAndUri(new Domain(1, "mysite/æøå", -1, "en-US", false), new Uri("http://mysite/æøå")));

            var result = lookup.finder.TryFindContent(lookup.frequest);

            Assert.IsTrue(result);
            Assert.AreEqual(expectedId, lookup.frequest.PublishedContent.Id);
        }
    }
}
