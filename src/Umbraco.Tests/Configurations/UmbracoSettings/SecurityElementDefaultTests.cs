using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class SecurityElementDefaultTests : SecurityElementTests
    {
        protected override bool TestingDefaults
        {
            get { return true; }
        }
    }
}