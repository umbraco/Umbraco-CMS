using System.Linq;
using NUnit.Framework;

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
            Assert.IsTrue(Section.RequestHandler.UrlReplacing.CharCollection.Count == 26);
            var chars = @" ,"",',%,.,;,/,\,:,#,+,*,&,?,æ,ø,å,ä,ö,ü,ß,Ä,Ö,|,<,>";
            Assert.IsTrue(Section.RequestHandler.UrlReplacing.CharCollection
                                 .All(x => chars.Split(',').Contains(x.Char)));
            var vals = @"-,plus,star,ae,oe,aa,ae,oe,ue,ss,ae,oe";
            Assert.IsTrue(Section.RequestHandler.UrlReplacing.CharCollection
                                 .All(x => string.IsNullOrEmpty(x.Value) || vals.Split(',').Contains(x.Value)));
        }
        
    }
}