using NUnit.Framework;
using Umbraco.Core;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class TemplateElementTests : UmbracoSettingsTests
    {
        [Test]
        public void UseAspNetMasterPages()
        {
            Assert.IsTrue(SettingsSection.Templates.UseAspNetMasterPages == true);
        }
        [Test]
        public void EnableSkinSupport()
        {
            Assert.IsTrue(SettingsSection.Templates.EnableSkinSupport);
        }
        [Test]
        public void DefaultRenderingEngine()
        {        
            Assert.IsTrue(SettingsSection.Templates.DefaultRenderingEngine == RenderingEngine.Mvc);
        }
        [Test]
        public void EnableTemplateFolders()
        {            
            Assert.IsTrue(SettingsSection.Templates.EnableTemplateFolders == false);
        }
    }
}