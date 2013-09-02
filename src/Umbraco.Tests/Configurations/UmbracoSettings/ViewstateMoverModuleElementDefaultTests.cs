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
}