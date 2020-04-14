using NUnit.Framework;

namespace Umbraco.Tests.Integration.Umbraco.Configuration.UmbracoSettings
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
