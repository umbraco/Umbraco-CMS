using NUnit.Framework;

namespace Umbraco.Tests.UnitTests.Umbraco.Configuration.UmbracoSettings
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
