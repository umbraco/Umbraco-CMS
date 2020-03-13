using System.Linq;
using NUnit.Framework;

namespace Umbraco.Tests.Configurations.UmbracoSettings
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
