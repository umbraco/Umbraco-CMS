using System.Linq;
using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class DistributedCallElementTests : UmbracoSettingsTests
    {
        [Test]
        public virtual void Enabled()
        {
            Assert.IsTrue(SettingsSection.DistributedCall.Enabled == true);
           
        }
        [Test]
        public void UserId()
        {
            Assert.IsTrue(SettingsSection.DistributedCall.UserId == 0);

        }
        [Test]
        public virtual void Servers()
        {
            Assert.IsTrue(SettingsSection.DistributedCall.Servers.Count() == 2);
            Assert.IsTrue(SettingsSection.DistributedCall.Servers.ElementAt(0).ServerAddress == "127.0.0.1");
            Assert.IsTrue(SettingsSection.DistributedCall.Servers.ElementAt(1).ServerAddress == "127.0.0.2");
            Assert.IsTrue(SettingsSection.DistributedCall.Servers.ElementAt(1).ForceProtocol == "https");
            Assert.IsTrue(SettingsSection.DistributedCall.Servers.ElementAt(1).ForcePortnumber == "443");
        }

    }
}