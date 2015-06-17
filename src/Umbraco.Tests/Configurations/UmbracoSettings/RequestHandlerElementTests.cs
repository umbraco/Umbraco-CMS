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
            Assert.IsTrue(SettingsSection.RequestHandler.UseDomainPrefixes == false);
            
        }
        [Test]
        public void AddTrailingSlash()
        {
            Assert.IsTrue(SettingsSection.RequestHandler.AddTrailingSlash == true);            
        }
        [Test]
        public void RemoveDoubleDashes()
        {
            Assert.IsTrue(SettingsSection.RequestHandler.RemoveDoubleDashes == true);
            
        }
        [Test]
        public void CharCollection()
        {
            var chars = @" ,"",',%,.,;,/,\,:,#,+,*,&,?,æ,ø,å,ä,ö,ü,ß,Ä,Ö,ã,à,á,â,ç,é,ê,í,õ,ó,ô,ú,|,<,>";
            var items = chars.Split(',');
            Assert.AreEqual(items.Length, SettingsSection.RequestHandler.CharCollection.Count());
            Assert.IsTrue(SettingsSection.RequestHandler.CharCollection
                                 .All(x => items.Contains(x.Char)));

            var vals = @"-,plus,star,ae,oe,aa,ae,oe,ue,ss,ae,oe,a,a,a,a,c,e,e,i,o,o,o,u,-";
            var splitVals = vals.Split(',');
            Assert.AreEqual(splitVals.Length, SettingsSection.RequestHandler.CharCollection.Count(x => x.Replacement.IsNullOrWhiteSpace() == false));
            Assert.IsTrue(SettingsSection.RequestHandler.CharCollection
                                 .All(x => string.IsNullOrEmpty(x.Replacement) || vals.Split(',').Contains(x.Replacement)));
        }
        
    }
}