using System;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.Routing
{
    [TestFixture]
    public class ContentFinderByNiceUrlTests : BaseRoutingTest
    {


        [TestCase("/", 1046)]
        [TestCase("/default.aspx", 1046)] //this one is actually rather important since this is the path that comes through when we are running in pre-IIS 7 for the root document '/' !
        [TestCase("/Sub1", 1173)]
        [TestCase("/sub1", 1173)]
        [TestCase("/sub1.aspx", 1173)]
        [TestCase("/home/sub1", -1)] // should fail

        // these two are special. getNiceUrl(1046) returns "/" but getNiceUrl(1172) cannot also return "/" so
        // we've made it return "/test-page" => we have to support that url back in the lookup...
        [TestCase("/home", 1046)]
        [TestCase("/test-page", 1172)]
        public void Match_Document_By_Url_Hide_Top_Level(string urlString, int expectedId)
        {
            var routingContext = GetRoutingContext(urlString);
            var url = routingContext.UmbracoContext.CleanedUmbracoUrl; //very important to use the cleaned up umbraco url
            var docreq = new PublishedContentRequest(url, routingContext);
            var lookup = new ContentFinderByNiceUrl();
            SettingsForTests.HideTopLevelNodeFromPath = true;

            var result = lookup.TryFindContent(docreq);

            if (expectedId > 0)
            {
                Assert.IsTrue(result);
                Assert.AreEqual(expectedId, docreq.PublishedContent.Id);
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
            var routingContext = GetRoutingContext(urlString);
            var url = routingContext.UmbracoContext.CleanedUmbracoUrl; //very important to use the cleaned up umbraco url		
            var docreq = new PublishedContentRequest(url, routingContext);
            var lookup = new ContentFinderByNiceUrl();
            SettingsForTests.HideTopLevelNodeFromPath = false;

            var result = lookup.TryFindContent(docreq);

            Assert.IsTrue(result);
            Assert.AreEqual(expectedId, docreq.PublishedContent.Id);
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
            var routingContext = GetRoutingContext(urlString);
            var url = routingContext.UmbracoContext
                .CleanedUmbracoUrl; //very important to use the cleaned up umbraco url		
            var docreq = new PublishedContentRequest(url, routingContext);
            var lookup = new ContentFinderByNiceUrl();
            SettingsForTests.HideTopLevelNodeFromPath = false;

            var result = lookup.TryFindContent(docreq);

            Assert.IsTrue(result);
            Assert.AreEqual(expectedId, docreq.PublishedContent.Id);
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
            var routingContext = GetRoutingContext(urlString);
            var url = routingContext.UmbracoContext
                .CleanedUmbracoUrl; //very important to use the cleaned up umbraco url		
            var docreq = new PublishedContentRequest(url, routingContext);
            docreq.UmbracoDomain = new UmbracoDomain("mysite");
            docreq.DomainUri = new Uri("http://mysite/");
            var lookup = new ContentFinderByNiceUrl();
            SettingsForTests.HideTopLevelNodeFromPath = false;

            var result = lookup.TryFindContent(docreq);

            Assert.IsTrue(result);
            Assert.AreEqual(expectedId, docreq.PublishedContent.Id);
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
            var routingContext = GetRoutingContext(urlString);
            var url = routingContext.UmbracoContext
                .CleanedUmbracoUrl; //very important to use the cleaned up umbraco url		
            var docreq = new PublishedContentRequest(url, routingContext);
            docreq.UmbracoDomain = new UmbracoDomain("http://mysite/æøå");
            docreq.DomainUri = new Uri("http://mysite/æøå");
            var lookup = new ContentFinderByNiceUrl();
            SettingsForTests.HideTopLevelNodeFromPath = false;

            var result = lookup.TryFindContent(docreq);

            Assert.IsTrue(result);
            Assert.AreEqual(expectedId, docreq.PublishedContent.Id);
        }
    }
}