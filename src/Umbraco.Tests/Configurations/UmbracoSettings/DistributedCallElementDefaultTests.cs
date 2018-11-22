using System.Linq;
using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class DistributedCallElementDefaultTests : DistributedCallElementTests
    {
        protected override bool TestingDefaults
        {
            get { return true; }
        }

        [Test]
        public override void Enabled()
        {
            Assert.IsTrue(SettingsSection.DistributedCall.Enabled == false);

        }

        [Test]
        public override void Servers()
        {
            Assert.IsTrue(SettingsSection.DistributedCall.Servers.Count() == 0);
        }
    }
}