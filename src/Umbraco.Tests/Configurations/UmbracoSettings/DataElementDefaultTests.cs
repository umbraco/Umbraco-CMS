using System.Linq;
using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class DataElementDefaultTests : UmbracoSettingsTests
    {
        protected override bool TestingDefaults
        {
            get { return true; }
        }

        [Test]
        public void SQLRetryPolicyBehaviour()
        {
            Assert.AreEqual(Core.SQLRetryPolicyBehaviour.Default, SettingsSection.Data.SQLRetryPolicyBehaviour);
        }
    }
}