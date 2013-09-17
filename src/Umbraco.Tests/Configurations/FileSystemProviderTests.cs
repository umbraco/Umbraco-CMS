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
    public class UmbracoConfigurationTests
    {
        [TearDown]
        public void TearDown()
        {
            UmbracoConfiguration.Reset();
        }

        [Test]
        public void Can_Set_Custom_Implementation()
        {
            var mockedContent = new Mock<IContentSection>();
            mockedContent.Setup(section => section.NotificationEmailAddress).Returns("test@test.com");
            UmbracoConfiguration.Set<IContentSection>(mockedContent.Object);

            Assert.AreEqual("test@test.com", UmbracoConfiguration.For<IContentSection>().NotificationEmailAddress);            
        }

        [Test]
        public void Can_Reset()
        {
            var mockedContent = new Mock<IContentSection>();
            mockedContent.Setup(section => section.NotificationEmailAddress).Returns("test@test.com");
            UmbracoConfiguration.Set<IContentSection>(mockedContent.Object);
            
            UmbracoConfiguration.Reset();

            Assert.Throws<InvalidOperationException>(() => UmbracoConfiguration.For<IContentSection>());
        }

    }

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