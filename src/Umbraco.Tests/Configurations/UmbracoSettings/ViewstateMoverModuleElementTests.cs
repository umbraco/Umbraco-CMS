using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class ViewstateMoverModuleElementDefaultTests : ViewstateMoverModuleElementTests
    {
        protected override bool TestingDefaults
        {
            get { return true; }
        }
    }

    [TestFixture]
    public class ViewstateMoverModuleElementTests : UmbracoSettingsTests
    {
        [Test]
        public void Enable()
        {
            Assert.IsTrue(Section.ViewstateMoverModule.Enable == false);
        }
    }
}