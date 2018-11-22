using System.Linq;
using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class LoggingElementDefaultTests : LoggingElementTests
    {
        protected override bool TestingDefaults
        {
            get { return true; }
        }

        [Test]
        public override void DisabledLogTypes()
        {
            Assert.IsTrue(SettingsSection.Logging.DisabledLogTypes.Any() == false);
        }
        [Test]
        public override void CleaningMiliseconds()
        {
            Assert.IsTrue(SettingsSection.Logging.CleaningMiliseconds == -1);
        }
        [Test]
        public override void MaxLogAge()
        {
            Assert.IsTrue(SettingsSection.Logging.MaxLogAge == -1);
        }
    }
}
