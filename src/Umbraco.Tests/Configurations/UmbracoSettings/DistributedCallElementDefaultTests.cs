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
            Assert.IsTrue(Section.DistributedCall.Enabled == false);

        }

        [Test]
        public override void Servers()
        {
            Assert.IsTrue(Section.DistributedCall.Servers.Count == 0);
        }
    }
}