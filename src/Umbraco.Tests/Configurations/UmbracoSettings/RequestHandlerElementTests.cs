using System.Linq;
using NUnit.Framework;
using Umbraco.Core;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class RequestHandlerElementTests : UmbracoSettingsTests
    {
        [Test]
        public void AddTrailingSlash()
        {
            Assert.IsTrue(RequestHandlerSettings.AddTrailingSlash == true);
        }

        [Test]
        public void CharCollection()
        {
            var chars = @" ,"",',%,.,;,/,\,:,#,+,*,&,?,æ,ø,å,ä,ö,ü,ß,Ä,Ö,|,<,>";
            var items = chars.Split(',');
            Assert.AreEqual(items.Length, RequestHandlerSettings.CharCollection.Count());
            Assert.IsTrue(RequestHandlerSettings.CharCollection
                                 .All(x => items.Contains(x.Char)));

            var vals = @"-,plus,star,ae,oe,aa,ae,oe,ue,ss,ae,oe,-";
            var splitVals = vals.Split(',');
            Assert.AreEqual(splitVals.Length, RequestHandlerSettings.CharCollection.Count(x => x.Replacement.IsNullOrWhiteSpace() == false));
            Assert.IsTrue(RequestHandlerSettings.CharCollection
                                 .All(x => string.IsNullOrEmpty(x.Replacement) || vals.Split(',').Contains(x.Replacement)));
        }

    }
}
