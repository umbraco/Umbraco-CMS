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
            Assert.IsTrue(Section.Templates.UseAspNetMasterPages == true);
        }
        [Test]
        public void EnableSkinSupport()
        {
            Assert.IsTrue(Section.Templates.EnableSkinSupport);
        }
        [Test]
        public void DefaultRenderingEngine()
        {        
            Assert.IsTrue(Section.Templates.DefaultRenderingEngine == RenderingEngine.Mvc);
        }
        [Test]
        public void EnableTemplateFolders()
        {            
            Assert.IsTrue(Section.Templates.EnableTemplateFolders == false);
        }
    }
}