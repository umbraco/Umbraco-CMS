using System;
using System.Configuration;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Tests.Configurations
{
  
    [TestFixture]
    public class FileSystemProviderTests
    {
        [Test]
        public void Can_Get_Media_Provider()
        {
            var config = (FileSystemProvidersSection)ConfigurationManager.GetSection("umbracoConfiguration/FileSystemProviders");
            var providerConfig = config.Providers["media"];

            Assert.That(providerConfig, Is.Not.Null);
            Assert.That(providerConfig.Parameters.AllKeys.Any(), Is.True);
        }
    }
}