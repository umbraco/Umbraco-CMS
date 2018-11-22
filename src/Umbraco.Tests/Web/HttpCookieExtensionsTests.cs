using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Web;

namespace Umbraco.Tests.Web
{
    [TestFixture]
    public class HttpCookieExtensionsTests
    {
        [TestCase("hello=world;cookies=are fun;", "hello", "world", true)]
        [TestCase("HELlo=world;cookies=are fun", "hello", "world", true)]
        [TestCase("HELlo= world;cookies=are fun", "hello", "world", true)]
        [TestCase("HELlo =world;cookies=are fun", "hello", "world", true)]
        [TestCase("hello = world;cookies=are fun;", "hello", "world", true)]
        [TestCase("hellos=world;cookies=are fun", "hello", "world", false)]
        [TestCase("hello=world;cookies?=are fun?", "hello", "world", true)]
        [TestCase("hel?lo=world;cookies=are fun?", "hel?lo", "world", true)]
        public void Get_Cookie_Value_From_HttpRequestHeaders(string cookieHeaderVal, string cookieName, string cookieVal, bool matches)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");
            var requestHeaders = request.Headers;
            requestHeaders.Add("Cookie", cookieHeaderVal);

            var valueFromHeader = requestHeaders.GetCookieValue(cookieName);

            if (matches)
            {
                Assert.IsNotNull(valueFromHeader);
                Assert.AreEqual(cookieVal, valueFromHeader);
            }
            else
            {
                Assert.IsNull(valueFromHeader);
            }                
        }
    }
}
