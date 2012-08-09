using System;
using NUnit.Framework;
using Umbraco.Web;

namespace Umbraco.Tests
{
	[TestFixture]
	public class UriUtilityTests
	{

		[TestCase("http://Localhost/", "http://localhost/")]
		[TestCase("http://localhost/default.aspx", "http://localhost/")]
		[TestCase("http://localhost/default.aspx?test=blah", "http://localhost/?test=blah")]
		[TestCase("http://localhost/home/Sub1", "http://localhost/home/sub1")]
		[TestCase("http://localhost/home/Sub1.aspx", "http://localhost/home/sub1")]
		[TestCase("http://localhost/home/Sub1.aspx?test=blah", "http://localhost/home/sub1?test=blah")]
		public void Uri_To_Umbraco(string url, string expected)
		{
			var uri = new Uri(url);
			var expectedUri = new Uri(expected);
			var result = UriUtility.UriToUmbraco(uri);			

			Assert.AreEqual(expectedUri.ToString(), result.ToString());
		}
	}
}