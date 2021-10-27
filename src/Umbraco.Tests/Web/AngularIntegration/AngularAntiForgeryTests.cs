using System.IO;
using System.Net;
using System.Security.Principal;
using System.Web;
using NUnit.Framework;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Tests.Web.AngularIntegration
{
    [TestFixture]
    public class AngularAntiForgeryTests
    {

        [TearDown]
        public void TearDown()
        {
            HttpContext.Current = null;
        }

        [Test]
        public void Can_Validate_Generated_Tokens()
        {
            using (var writer = new StringWriter())
            {
                HttpContext.Current = new HttpContext(new HttpRequest("test.html", "http://test/", ""), new HttpResponse(writer));

                string cookieToken, headerToken;
                AngularAntiForgeryHelper.GetTokens(out cookieToken, out headerToken);

                Assert.IsTrue(AngularAntiForgeryHelper.ValidateTokens(cookieToken, headerToken));
            }

        }

        [Test]
        public void Can_Validate_Generated_Tokens_With_User()
        {
            using (var writer = new StringWriter())
            {
                HttpContext.Current = new HttpContext(new HttpRequest("test.html", "http://test/", ""), new HttpResponse(writer))
                    {
                        User = new GenericPrincipal(new HttpListenerBasicIdentity("test", "test"), new string[] {})
                    };

                string cookieToken, headerToken;
                AngularAntiForgeryHelper.GetTokens(out cookieToken, out headerToken);

                Assert.IsTrue(AngularAntiForgeryHelper.ValidateTokens(cookieToken, headerToken));
            }

        }

    }
}
