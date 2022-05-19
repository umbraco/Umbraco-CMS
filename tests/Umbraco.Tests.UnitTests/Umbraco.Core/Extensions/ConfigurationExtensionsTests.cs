// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions
{
    [TestFixture]
    public class ConfigurationExtensionsTests
    {
        private const string DataDirectory = @"C:\Data";

        [Test]
        public void CanParseSqlServerConnectionString()
        {
            const string ConfiguredConnectionString = @"Server=.\SQLEXPRESS;Database=UmbracoCms;Integrated Security=true";

            Mock<IConfiguration> mockedConfig = CreateConfig(ConfiguredConnectionString);

            string connectionString = mockedConfig.Object.GetUmbracoConnectionString(out string providerName);

            AssertResults(
                ConfiguredConnectionString,
                "Microsoft.Data.SqlClient",
                connectionString,
                providerName);
        }

        [Test]
        public void CanParseLocalDbConnectionString()
        {
            const string ConfiguredConnectionString = @"Server=(LocalDb)\MyInstance;Integrated Security=true;";

            Mock<IConfiguration> mockedConfig = CreateConfig(ConfiguredConnectionString);

            string connectionString = mockedConfig.Object.GetUmbracoConnectionString(out string providerName);

            AssertResults(
                ConfiguredConnectionString,
                "Microsoft.Data.SqlClient",
                connectionString,
                providerName);
        }

        [Test]
        public void CanParseLocalDbConnectionStringWithDataDirectory()
        {
            const string ConfiguredConnectionString = @"Data Source=(LocalDb)\MyInstance;Initial Catalog=UmbracoDb;Integrated Security=SSPI;AttachDBFilename=|DataDirectory|\Umbraco.mdf";

            Mock<IConfiguration> mockedConfig = CreateConfig(ConfiguredConnectionString);
            SetDataDirectory();

            string connectionString = mockedConfig.Object.GetUmbracoConnectionString(out string providerName);

            AssertResults(
                @"Data Source=(LocalDb)\MyInstance;Initial Catalog=UmbracoDb;Integrated Security=SSPI;AttachDBFilename=C:\Data\Umbraco.mdf",
                "Microsoft.Data.SqlClient",
                connectionString,
                providerName);
        }

        [Test]
        public void CanParseSQLiteConnectionStringWithDataDirectory()
        {
            const string ConfiguredConnectionString = "Data Source=|DataDirectory|/Umbraco.sqlite.db;Cache=Shared;Foreign Keys=True;Pooling=True";
            const string ConfiguredProviderName = "Microsoft.Data.SQLite";

            Mock<IConfiguration> mockedConfig = CreateConfig(ConfiguredConnectionString, ConfiguredProviderName);
            SetDataDirectory();

            string connectionString = mockedConfig.Object.GetUmbracoConnectionString(out string providerName);

            AssertResults(
                @"Data Source=C:\Data/Umbraco.sqlite.db;Cache=Shared;Foreign Keys=True;Pooling=True",
                "Microsoft.Data.SQLite",
                connectionString,
                providerName);
        }

        [Test]
        public void CanParseConnectionStringWithNamedProvider()
        {
            const string ConfiguredConnectionString = @"Server=.\SQLEXPRESS;Database=UmbracoCms;Integrated Security=true";
            const string ConfiguredProviderName = "MyProvider";

            Mock<IConfiguration> mockedConfig = CreateConfig(ConfiguredConnectionString, ConfiguredProviderName);

            string connectionString = mockedConfig.Object.GetUmbracoConnectionString(out string providerName);

            AssertResults(
                ConfiguredConnectionString,
                ConfiguredProviderName,
                connectionString,
                providerName);
        }

        private static Mock<IConfiguration> CreateConfig(string configuredConnectionString, string configuredProviderName = ConnectionStrings.DefaultProviderName)
        {
            var mockConfigSection = new Mock<IConfigurationSection>();
            mockConfigSection
                .SetupGet(m => m[It.Is<string>(s => s == Constants.System.UmbracoConnectionName)])
                .Returns(configuredConnectionString);
            mockConfigSection
                .SetupGet(m => m[It.Is<string>(s => s == $"{Constants.System.UmbracoConnectionName}{ConnectionStrings.ProviderNamePostfix}")])
                .Returns(configuredProviderName);

            var mockedConfig = new Mock<IConfiguration>();
            mockedConfig
                .Setup(a => a.GetSection(It.Is<string>(s => s == "ConnectionStrings")))
                .Returns(mockConfigSection.Object);

            return mockedConfig;
        }

        private static void SetDataDirectory() =>
            AppDomain.CurrentDomain.SetData("DataDirectory", DataDirectory);

        private static void AssertResults(string expectedConnectionString, string expectedProviderName, string connectionString, string providerName)
        {
            Assert.AreEqual(expectedConnectionString, connectionString);
            Assert.AreEqual(expectedProviderName, providerName);
        }
    }
}
