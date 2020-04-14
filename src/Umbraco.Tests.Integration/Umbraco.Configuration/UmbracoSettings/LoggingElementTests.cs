using NUnit.Framework;

namespace Umbraco.Tests.Integration.Umbraco.Configuration.UmbracoSettings
{
    [TestFixture]
    public class LoggingElementTests : UmbracoSettingsTests
    {

        [Test]
        public virtual void MaxLogAge()
        {
            Assert.IsTrue(LoggingSettings.MaxLogAge == 1440);

        }

    }
}
