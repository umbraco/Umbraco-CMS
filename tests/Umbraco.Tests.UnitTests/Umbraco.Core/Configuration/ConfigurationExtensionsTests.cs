// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configuration
{
    [TestFixture]
    public class ConfigurationExtensionsTests
    {
        private const string DataDirectory = "c:\\test\\data\\";

        [Test]
        public void CanParseSqlServerConnectionString()
        {
            const string ConfiguredConnectionString = "Server=.\\SQLEXPRESS;Database=UmbracoCms;Integrated Security=true";

            Mock<IConfiguration> mockedConfig = CreateConfig(ConfiguredConnectionString);

            string connectionString = mockedConfig.Object.GetUmbracoConnectionString(out string providerName);

            AssertResults(
                ConfiguredConnectionString,
                "System.Data.SqlClient",
                connectionString,
                providerName);
        }

        [Test]
        public void CanParseLocalDbConnectionString()
        {
            const string ConfiguredConnectionString = "Server=(LocalDb)\\MyInstance;Integrated Security=true;";

            Mock<IConfiguration> mockedConfig = CreateConfig(ConfiguredConnectionString);

            string connectionString = mockedConfig.Object.GetUmbracoConnectionString(out string providerName);

            AssertResults(
                ConfiguredConnectionString,
                "System.Data.SqlClient",
                connectionString,
                providerName);
        }

        [Test]
        public void CanParseLocalDbConnectionStringWithDataDirectory()
        {
            const string ConfiguredConnectionString = "Data Source=(LocalDb)\\MyInstance;Initial Catalog=UmbracoDb;Integrated Security=SSPI;AttachDBFilename=|DataDirectory|Umbraco.mdf";

            Mock<IConfiguration> mockedConfig = CreateConfig(ConfiguredConnectionString);
            SetDataDirectory();

            string connectionString = mockedConfig.Object.GetUmbracoConnectionString(out string providerName);

            AssertResults(
                "data source=(LocalDb)\\MyInstance;initial catalog=UmbracoDb;integrated security=SSPI;attachdbfilename=c:\\test\\data\\Umbraco.mdf",
                "System.Data.SqlClient",
                connectionString,
                providerName);
        }

        [Test]
        public void CanParseSqlCeConnectionString()
        {
            const string ConfiguredConnectionString = "Data Source=Umbraco.sdf;Max Buffer Size=1024;Persist Security Info=False";

            Mock<IConfiguration> mockedConfig = CreateConfig(ConfiguredConnectionString);

            string connectionString = mockedConfig.Object.GetUmbracoConnectionString(out string providerName);

            AssertResults(
                ConfiguredConnectionString,
                "System.Data.SqlServerCe.4.0",
                connectionString,
                providerName);
        }

        [Test]
        public void CanPariseConnectionStringWithNamedProvider()
        {
            const string ConfiguredConnectionString = "Server=.\\SQLEXPRESS;Database=UmbracoCms;Integrated Security=true";
            const string ConfiguredProviderName = "MyProvider";

            Mock<IConfiguration> mockedConfig = CreateConfig(ConfiguredConnectionString, ConfiguredProviderName);

            string connectionString = mockedConfig.Object.GetUmbracoConnectionString(out string providerName);

            AssertResults(
                ConfiguredConnectionString,
                ConfiguredProviderName,
                connectionString,
                providerName);
        }

        private static Mock<IConfiguration> CreateConfig(string configuredConnectionString, string configuredProviderName = "")
        {
            var mockConfigSection = new Mock<IConfigurationSection>();
            mockConfigSection
                .SetupGet(m => m[It.Is<string>(s => s == Cms.Core.Constants.System.UmbracoConnectionName)])
                .Returns(configuredConnectionString);
            mockConfigSection
                .SetupGet(m => m[It.Is<string>(s => s == $"{Cms.Core.Constants.System.UmbracoConnectionName}_ProviderName")])
                .Returns(configuredProviderName);

            var mockedConfig = new Mock<IConfiguration>();
            mockedConfig
                .Setup(a => a.GetSection(It.Is<string>(s => s == "ConnectionStrings")))
                .Returns(mockConfigSection.Object);
            return mockedConfig;
        }

        private static void SetDataDirectory() =>
            AppDomain.CurrentDomain.SetData("DataDirectory", DataDirectory);

        private static void AssertResults(
            string expectedConnectionString,
            string expectedProviderName,
            string connectionString,
            string providerName)
        {
            Assert.AreEqual(expectedConnectionString, connectionString);
            Assert.AreEqual(expectedProviderName, providerName);
        }
    }
}
