using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class DataElementTests : UmbracoSettingsTests
    {
        [Test]
        public void SQLRetryPolicyBehaviour()
        {
            Assert.AreEqual(Core.SQLRetryPolicyBehaviour.Azure, SettingsSection.Data.SQLRetryPolicyBehaviour);
        }
    }
}
