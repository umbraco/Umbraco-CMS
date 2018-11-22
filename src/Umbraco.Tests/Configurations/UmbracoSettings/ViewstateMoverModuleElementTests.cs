using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class ViewstateMoverModuleElementTests : UmbracoSettingsTests
    {
        [Test]
        public void Enable()
        {
            Assert.IsTrue(SettingsSection.ViewStateMoverModule.Enable == false);
        }
    }
}