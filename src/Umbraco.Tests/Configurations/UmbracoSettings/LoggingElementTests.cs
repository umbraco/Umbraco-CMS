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
            Assert.IsTrue(Section.Logging.ExternalLoggerIsConfigured == true);
        }

        [Test]
        public void EnableLogging()
        {
            Assert.IsTrue(Section.Logging.EnableLogging == true);
        }
        [Test]
        public void EnableAsyncLogging()
        {
            Assert.IsTrue(Section.Logging.EnableAsyncLogging == true);
        }
        [Test]
        public virtual void DisabledLogTypes()
        {
            Assert.IsTrue(Section.Logging.DisabledLogTypes.Count() == 2);
            Assert.IsTrue(Section.Logging.DisabledLogTypes.ElementAt(0).LogTypeAlias == "[alias-of-log-type-in-lowercase]");
            Assert.IsTrue(Section.Logging.DisabledLogTypes.ElementAt(1).LogTypeAlias == "anotherlogalias");
        }
        [Test]
        public virtual void ExternalLogger_Assembly()
        {
            Assert.IsTrue(Section.Logging.ExternalLogger.Assembly == "~/bin/assemblyFileName.dll");
        }
        [Test]
        public virtual void ExternalLogger_Type()
        {
            Assert.IsTrue(Section.Logging.ExternalLogger.Type == "fully.qualified.namespace.and.type");
        }
        [Test]
        public virtual void ExternalLogger_LogAuditTrail()
        {
            Assert.IsTrue(Section.Logging.ExternalLogger.LogAuditTrail == false);
        }
        [Test]
        public void AutoCleanLogs()
        {
            Assert.IsTrue(Section.Logging.AutoCleanLogs == false);
        }

        [Test]
        public virtual void CleaningMiliseconds()
        {
            Assert.IsTrue(Section.Logging.CleaningMiliseconds == 86400);

        }
        [Test]
        public virtual void MaxLogAge()
        {
            Assert.IsTrue(Section.Logging.MaxLogAge == 1440);

        }

    }
}