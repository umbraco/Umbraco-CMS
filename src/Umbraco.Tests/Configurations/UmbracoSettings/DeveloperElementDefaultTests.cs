using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class DeveloperElementDefaultTests : DeveloperElementTests
    {
        protected override bool TestingDefaults
        {
            get { return true; }
        }
    }
}