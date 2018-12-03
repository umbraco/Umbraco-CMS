using System.Linq;
using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class LoggingElementTests : UmbracoSettingsTests
    {
        [Test]
        public void EnableLogging()
        {
            Assert.IsTrue(SettingsSection.Logging.EnableLogging == true);
        }

        [Test]
        public virtual void DisabledLogTypes()
        {
            Assert.IsTrue(SettingsSection.Logging.DisabledLogTypes.Count() == 2);
            Assert.IsTrue(SettingsSection.Logging.DisabledLogTypes.ElementAt(0).LogTypeAlias == "[alias-of-log-type-in-lowercase]");
            Assert.IsTrue(SettingsSection.Logging.DisabledLogTypes.ElementAt(1).LogTypeAlias == "anotherlogalias");
        }

        [Test]
        public void AutoCleanLogs()
        {
            Assert.IsTrue(SettingsSection.Logging.AutoCleanLogs == false);
        }

        [Test]
        public virtual void CleaningMiliseconds()
        {
            Assert.IsTrue(SettingsSection.Logging.CleaningMiliseconds == 86400);

        }
        [Test]
        public virtual void MaxLogAge()
        {
            Assert.IsTrue(SettingsSection.Logging.MaxLogAge == 1440);

        }

    }
}
