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
        public override void ExternalLoggerConfigured()
        {
            Assert.IsTrue(Section.Logging.ExternalLoggerIsConfigured == false);
        }
        [Test]
        public override void ExternalLogger_Assembly()
        {
            Assert.IsTrue(Section.Logging.ExternalLoggerIsConfigured == false);
        }
        [Test]
        public override void ExternalLogger_LogAuditTrail()
        {
            Assert.IsTrue(Section.Logging.ExternalLoggerIsConfigured == false);
        }
        [Test]
        public override void ExternalLogger_Type()
        {
            Assert.IsTrue(Section.Logging.ExternalLoggerIsConfigured == false);
        }
        [Test]
        public override void DisabledLogTypes()
        {
            Assert.IsTrue(Section.Logging.DisabledLogTypes.Count == 0);
        }
        [Test]
        public override void CleaningMiliseconds()
        {
            Assert.IsTrue(Section.Logging.CleaningMiliseconds == -1);
        }
        [Test]
        public override void MaxLogAge()
        {
            Assert.IsTrue(Section.Logging.MaxLogAge == -1);
        }
    }
}