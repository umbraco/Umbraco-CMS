using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class ProvidersElementDefaultTests : ProvidersElementTests
    {
        protected override bool TestingDefaults
        {
            get { return true; }
        }

    }
}