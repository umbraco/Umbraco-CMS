using NUnit.Framework;

namespace Umbraco.Tests.Integration.Umbraco.Configuration.UmbracoSettings
{
    [TestFixture]
    public class LoggingElementDefaultTests : LoggingElementTests
    {
        protected override bool TestingDefaults => true;

        [Test]
        public override void MaxLogAge()
        {
            Assert.IsTrue(LoggingSettings.MaxLogAge == -1);
        }
    }
}
