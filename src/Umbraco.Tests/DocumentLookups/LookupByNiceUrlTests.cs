using System;
using System.Web;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Resolving;
using Umbraco.Tests.Stubs;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.DocumentLookups
{

	[TestFixture]
	public abstract class BaseTest
	{
		[SetUp]
		public virtual void Initialize()
		{
			TestHelper.SetupLog4NetForTests();
			
			Resolution.Freeze();
		}

		[TearDown]
		public virtual void TearDown()
		{
			ActionsResolver.Reset();
			Resolution.IsFrozen = false;
		}

		protected FakeHttpContextFactory GetHttpContextFactory(string url)
		{
			var factory = new FakeHttpContextFactory(url);
			return factory;
		}

		protected UmbracoContext GetUmbracoContext(string url)
		{
			return new UmbracoContext(
				GetHttpContextFactory(url).HttpContext,
				new ApplicationContext(),
				new NullRoutesCache());
		}
	}

	[TestFixture]
	public class LookupByNiceUrlTests : BaseTest
	{
		
		
		[Test]
		public void Test_Default_ASPX()
		{
			var urlAsString = "http://localhost/default.aspx";
			var ctx = GetUmbracoContext(urlAsString);
			var cleanUrl = new Uri(urlAsString);
			var path = ctx.RequestUrl.AbsolutePath.ToLower();
			UmbracoModule.LegacyCleanUmbPageFromQueryString(ref cleanUrl, ref path);
			var docRequest = new DocumentRequest(cleanUrl, ctx);
			var lookup = new LookupByNiceUrl();

			var result = lookup.TrySetDocument(docRequest);
		}

	}
}