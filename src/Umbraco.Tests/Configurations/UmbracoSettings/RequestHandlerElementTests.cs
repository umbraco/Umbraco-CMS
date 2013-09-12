using System.Linq;
using NUnit.Framework;
using Umbraco.Core;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class RequestHandlerElementTests : UmbracoSettingsTests
    {
        [Test]
        public void UseDomainPrefixes()
        {
            Assert.IsTrue(Section.RequestHandler.UseDomainPrefixes == false);
            
        }
        [Test]
        public void AddTrailingSlash()
        {
            Assert.IsTrue(Section.RequestHandler.AddTrailingSlash == true);            
        }
        [Test]
        public void RemoveDoubleDashes()
        {
            Assert.IsTrue(Section.RequestHandler.UrlReplacing.RemoveDoubleDashes == true);
            
        }
        [Test]
        public void CharCollection()
        {            
            var chars = @" ,"",',%,.,;,/,\,:,#,+,*,&,?,æ,ø,å,ä,ö,ü,ß,Ä,Ö,|,<,>";
            var items = chars.Split(',');
            Assert.AreEqual(items.Length, Section.RequestHandler.UrlReplacing.CharCollection.Count);
            Assert.IsTrue(Section.RequestHandler.UrlReplacing.CharCollection
                                 .All(x => items.Contains(x.Char)));

            var vals = @"-,plus,star,ae,oe,aa,ae,oe,ue,ss,ae,oe,-";
            var splitVals = vals.Split(',');
            Assert.AreEqual(splitVals.Length, Section.RequestHandler.UrlReplacing.CharCollection.Count(x => x.Replacement.IsNullOrWhiteSpace() == false));
            Assert.IsTrue(Section.RequestHandler.UrlReplacing.CharCollection
                                 .All(x => string.IsNullOrEmpty(x.Replacement) || vals.Split(',').Contains(x.Replacement)));
        }
        
    }

    [TestFixture]
    public class RequestHandlerElementDefaultTests : RequestHandlerElementTests
    {
        protected override bool TestingDefaults
        {
            get { return true; }
        }
    }
}