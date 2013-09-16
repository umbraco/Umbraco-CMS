using System.Linq;
using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class HelpElementTests : UmbracoSettingsTests
    {
        [Test]
        public void DefaultUrl()
        {
            Assert.IsTrue(SettingsSection.Help.DefaultUrl == "http://our.umbraco.org/wiki/umbraco-help/{0}/{1}");            
        }

        [Test]
        public virtual void Links()
        {
            Assert.IsTrue(SettingsSection.Help.Links.Count() == 2);
            Assert.IsTrue(SettingsSection.Help.Links.ElementAt(0).Application == "content");
            Assert.IsTrue(SettingsSection.Help.Links.ElementAt(0).ApplicationUrl == "dashboard.aspx");
            Assert.IsTrue(SettingsSection.Help.Links.ElementAt(0).Language == "en");
            Assert.IsTrue(SettingsSection.Help.Links.ElementAt(0).UserType == "Administrators");
            Assert.IsTrue(SettingsSection.Help.Links.ElementAt(0).HelpUrl == "http://www.xyz.no?{0}/{1}/{2}/{3}");
            Assert.IsTrue(SettingsSection.Help.Links.ElementAt(1).Application == "media");
            Assert.IsTrue(SettingsSection.Help.Links.ElementAt(1).ApplicationUrl == "dashboard2.aspx");
            Assert.IsTrue(SettingsSection.Help.Links.ElementAt(1).Language == "ch");
            Assert.IsTrue(SettingsSection.Help.Links.ElementAt(1).UserType == "Writers");
            Assert.IsTrue(SettingsSection.Help.Links.ElementAt(1).HelpUrl == "http://www.abc.no?{0}/{1}/{2}/{3}");
        }
    }
}