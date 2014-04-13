using NUnit.Framework;
using Umbraco.Core;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class ProvidersElementTests : UmbracoSettingsTests
    {
        [Test]
        public void Users()
        {
            Assert.IsTrue(SettingsSection.Providers.DefaultBackOfficeUserProvider == "UsersMembershipProvider");
        }
    }
}