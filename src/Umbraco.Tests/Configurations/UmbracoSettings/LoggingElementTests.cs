using System.Linq;
using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class LoggingElementTests : UmbracoSettingsTests
    {
        [Test]
        public virtual void ExternalLoggerConfigured()
        {
            Assert.IsTrue(SettingsSection.Logging.ExternalLoggerIsConfigured == true);
        }

        [Test]
        public void EnableLogging()
        {
            Assert.IsTrue(SettingsSection.Logging.EnableLogging == true);
        }
        [Test]
        public void EnableAsyncLogging()
        {
            Assert.IsTrue(SettingsSection.Logging.EnableAsyncLogging == true);
        }
        [Test]
        public virtual void DisabledLogTypes()
        {
            Assert.IsTrue(SettingsSection.Logging.DisabledLogTypes.Count() == 2);
            Assert.IsTrue(SettingsSection.Logging.DisabledLogTypes.ElementAt(0).LogTypeAlias == "[alias-of-log-type-in-lowercase]");
            Assert.IsTrue(SettingsSection.Logging.DisabledLogTypes.ElementAt(1).LogTypeAlias == "anotherlogalias");
        }
        [Test]
        public virtual void ExternalLogger_Assembly()
        {
            Assert.IsTrue(SettingsSection.Logging.ExternalLoggerAssembly == "~/bin/assemblyFileName.dll");
        }
        [Test]
        public virtual void ExternalLogger_Type()
        {
            Assert.IsTrue(SettingsSection.Logging.ExternalLoggerType == "fully.qualified.namespace.and.type");
        }
        [Test]
        public virtual void ExternalLogger_LogAuditTrail()
        {
            Assert.IsTrue(SettingsSection.Logging.ExternalLoggerEnableAuditTrail == false);
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