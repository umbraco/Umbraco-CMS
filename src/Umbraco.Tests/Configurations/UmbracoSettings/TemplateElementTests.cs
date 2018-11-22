using NUnit.Framework;
using Umbraco.Core;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class TemplateElementTests : UmbracoSettingsTests
    {
        [Test]
        public void DefaultRenderingEngine()
        {
            Assert.IsTrue(SettingsSection.Templates.DefaultRenderingEngine == RenderingEngine.Mvc);
        }

    }
}
