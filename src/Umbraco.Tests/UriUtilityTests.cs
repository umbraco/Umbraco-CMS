using System;
using System.Configuration;
using Moq;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;

namespace Umbraco.Tests
{
	// fixme - not testing virtual directory!

	[TestFixture]
	public class UriUtilityTests
	{

		[TearDown]
		public void TearDown()
		{
            SettingsForTests.Reset();
		}

		// test normal urls
		[TestCase("http://LocalHost/", "http://localhost/")]
		[TestCase("http://LocalHost/?x=y", "http://localhost/?x=y")]
		[TestCase("http://LocalHost/Home", "http://localhost/home")]
		[TestCase("http://LocalHost/Home?x=y", "http://localhost/home?x=y")]
		[TestCase("http://LocalHost/Home/Sub1", "http://localhost/home/sub1")]
		[TestCase("http://LocalHost/Home/Sub1?x=y", "http://localhost/home/sub1?x=y")]

		// same with .aspx
		[TestCase("http://LocalHost/Home.aspx", "http://localhost/home")]
		[TestCase("http://LocalHost/Home.aspx?x=y", "http://localhost/home?x=y")]
		[TestCase("http://LocalHost/Home/Sub1.aspx", "http://localhost/home/sub1")]
		[TestCase("http://LocalHost/Home/Sub1.aspx?x=y", "http://localhost/home/sub1?x=y")]

		// test that the trailing slash goes but not on hostname
		[TestCase("http://LocalHost/", "http://localhost/")]
		[TestCase("http://LocalHost/Home/", "http://localhost/home")]
		[TestCase("http://LocalHost/Home/?x=y", "http://localhost/home?x=y")]
		[TestCase("http://LocalHost/Home/Sub1/", "http://localhost/home/sub1")]
		[TestCase("http://LocalHost/Home/Sub1/?x=y", "http://localhost/home/sub1?x=y")]

		// test that default.aspx goes, even with parameters
		[TestCase("http://LocalHost/deFault.aspx", "http://localhost/")]
		[TestCase("http://LocalHost/deFault.aspx?x=y", "http://localhost/?x=y")]

		// test with inner .aspx
		[TestCase("http://Localhost/Home/Sub1.aspx/Sub2", "http://localhost/home/sub1/sub2")]
		[TestCase("http://Localhost/Home/Sub1.aspx/Sub2?x=y", "http://localhost/home/sub1/sub2?x=y")]
		[TestCase("http://Localhost/Home.aspx/Sub1.aspx/Sub2?x=y", "http://localhost/home/sub1/sub2?x=y")]
		[TestCase("http://Localhost/deFault.aspx/Home.aspx/deFault.aspx/Sub1.aspx", "http://localhost/home/default/sub1")]

		public void Uri_To_Umbraco(string sourceUrl, string expectedUrl)
		{
			UriUtility.SetAppDomainAppVirtualPath("/");

			var expectedUri = new Uri(expectedUrl);
			var sourceUri = new Uri(sourceUrl);
			var resultUri = UriUtility.UriToUmbraco(sourceUri);			

			Assert.AreEqual(expectedUri.ToString(), resultUri.ToString());
		}

		// test directoryUrl false, trailingSlash false
		[TestCase("/", "/", false, false)]
		[TestCase("/home", "/home.aspx", false, false)]
		[TestCase("/home/sub1", "/home/sub1.aspx", false, false)]

		// test directoryUrl false, trailingSlash true
		[TestCase("/", "/", false, true)]
		[TestCase("/home", "/home.aspx", false, true)]
		[TestCase("/home/sub1", "/home/sub1.aspx", false, true)]

		// test directoryUrl true, trailingSlash false
		[TestCase("/", "/", true, false)]
		[TestCase("/home", "/home", true, false)]
		[TestCase("/home/sub1", "/home/sub1", true, false)]

		// test directoryUrl true, trailingSlash true
		[TestCase("/", "/", true, true)]
		[TestCase("/home", "/home/", true, true)]
		[TestCase("/home/sub1", "/home/sub1/", true, true)]

		public void Uri_From_Umbraco(string sourceUrl, string expectedUrl, bool directoryUrls, bool trailingSlash)
		{
			ConfigurationManager.AppSettings.Set("umbracoUseDirectoryUrls", directoryUrls ? "true" : "false");

            var settings = SettingsForTests.GenerateMockSettings();
            var requestMock = Mock.Get(settings.RequestHandler);
            requestMock.Setup(x => x.AddTrailingSlash).Returns(trailingSlash);
            SettingsForTests.ConfigureSettings(settings);
			
            UriUtility.SetAppDomainAppVirtualPath("/");

			var expectedUri = NewUri(expectedUrl);
			var sourceUri = NewUri(sourceUrl);
			var resultUri = UriUtility.UriFromUmbraco(sourceUri);

			Assert.AreEqual(expectedUri.ToString(), resultUri.ToString());
		}

		Uri NewUri(string url)
		{
			return new Uri(url, url.StartsWith("http:") ? UriKind.Absolute : UriKind.Relative);
		}

		//
		[TestCase("/", "/", "/")]
		[TestCase("/", "/foo", "/foo")]
		[TestCase("/", "~/foo", "/foo")]
		[TestCase("/vdir", "/", "/vdir/")]
		[TestCase("/vdir", "/foo", "/vdir/foo")]
		[TestCase("/vdir", "/foo/", "/vdir/foo/")]
		[TestCase("/vdir", "~/foo", "/vdir/foo")]

		public void Uri_To_Absolute(string virtualPath, string sourceUrl, string expectedUrl)
		{
			UriUtility.SetAppDomainAppVirtualPath(virtualPath);
			var resultUrl = UriUtility.ToAbsolute(sourceUrl);
			Assert.AreEqual(expectedUrl, resultUrl);
		}

		//
		[TestCase("/", "/", "/")]
		[TestCase("/", "/foo", "/foo")]
		[TestCase("/", "/foo/", "/foo/")]
		[TestCase("/vdir", "/vdir", "/")]
		[TestCase("/vdir", "/vdir/", "/")]
		[TestCase("/vdir", "/vdir/foo", "/foo")]
		[TestCase("/vdir", "/vdir/foo/", "/foo/")]

		public void Url_To_App_Relative(string virtualPath, string sourceUrl, string expectedUrl)
		{
			UriUtility.SetAppDomainAppVirtualPath(virtualPath);
			var resultUrl = UriUtility.ToAppRelative(sourceUrl);
			Assert.AreEqual(expectedUrl, resultUrl);
		}
	}
}