using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Install.Models;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.ManagementApi.Mapping.Installer;
using Umbraco.Cms.Tests.Common;
using Umbraco.New.Cms.Core.Models.Installer;
using Umbraco.New.Cms.Infrastructure.Factories.Installer;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.New.Cms.Infrastructure.Factories;

[TestFixture]
public class DatabaseSettingsFactoryTests
{
    [Test]
    public void CanBuildDatabaseSettings()
    {
        var metadata = CreateTestMetadata();
        var connectionString = new TestOptionsMonitor<ConnectionStrings>(new ConnectionStrings());
        var mapper = CreateMapper();

        var factory = new DatabaseSettingsFactory(metadata, connectionString, mapper);

        var settingsModels = factory.GetDatabaseSettings();
        Assert.AreEqual(metadata.Count, settingsModels.Count);
        AssertMapping(metadata, settingsModels);
    }

    [Test]
    public void IsConfiguredSetCorrectly()
    {
        var connectionString = new ConnectionStrings
        {
            ConnectionString = "SomeConnectionString",
            ProviderName = "HostedTestMeta",
        };
        var optionsMonitor = new TestOptionsMonitor<ConnectionStrings>(connectionString);
        var mapper = CreateMapper();
        var metadata = CreateTestMetadata();

        var factory = new DatabaseSettingsFactory(metadata, optionsMonitor, mapper);

        var settingsModels = factory.GetDatabaseSettings();

        Assert.AreEqual(1, settingsModels.Count, "Expected only one database settings model, if a database is preconfigured we should only return the configured one.");
        AssertMapping(metadata, settingsModels);
        Assert.IsTrue(settingsModels.First().IsConfigured);
    }

    [Test]
    public void SpecifiedProviderMustExist()
    {
        var connectionString = new ConnectionStrings
        {
            ConnectionString = "SomeConnectionString",
            ProviderName = "NoneExistentProvider",
        };
        var optionsMonitor = new TestOptionsMonitor<ConnectionStrings>(connectionString);
        var mapper = CreateMapper();
        var metadata = CreateTestMetadata();

        var factory = new DatabaseSettingsFactory(metadata, optionsMonitor, mapper);
        Assert.Throws<InvalidOperationException>(() => factory.GetDatabaseSettings());
    }

    /// <summary>
    /// Asserts that the mapping is correct, in other words that the values in DatabaseSettingsModel is as expected.
    /// </summary>
    private void AssertMapping(
        IEnumerable<IDatabaseProviderMetadata> expected,
        ICollection<DatabaseSettingsModel> actual)
    {
        expected = expected.ToList();
        foreach (var model in actual)
        {
            var metadata = expected.FirstOrDefault(x => x.Id == model.Id);
            Assert.IsNotNull(metadata);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(metadata?.SortOrder, model.SortOrder);
                Assert.AreEqual(metadata.DisplayName, model.DisplayName);
                Assert.AreEqual(metadata.DefaultDatabaseName, model.DefaultDatabaseName);
                Assert.AreEqual(metadata.ProviderName ?? string.Empty, model.ProviderName);
                Assert.AreEqual(metadata.RequiresServer, model.RequiresServer);
                Assert.AreEqual(metadata.ServerPlaceholder ?? string.Empty, model.ServerPlaceholder);
                Assert.AreEqual(metadata.RequiresCredentials, model.RequiresCredentials);
                Assert.AreEqual(metadata.SupportsIntegratedAuthentication, model.SupportsIntegratedAuthentication);
                Assert.AreEqual(metadata.RequiresConnectionTest, model.RequiresConnectionTest);
            });
        }
    }

    private IUmbracoMapper CreateMapper()
    {
        var mapper = new UmbracoMapper(
            new MapDefinitionCollection(Enumerable.Empty<IMapDefinition>),
            Mock.Of<ICoreScopeProvider>());

        var definition = new InstallerViewModelsMapDefinition();
        definition.DefineMaps(mapper);
        return mapper;
    }

    private List<IDatabaseProviderMetadata> CreateTestMetadata()
    {

        var metadata = new List<IDatabaseProviderMetadata>
        {
            new TestDatabaseProviderMetadata
            {
                Id = Guid.Parse("EC8ACD63-8CDE-4CA5-B2A3-06322720F274"),
                SortOrder = 1,
                DisplayName = "FirstMetadata",
                DefaultDatabaseName = "TestDatabase",
                IsAvailable = true,
                GenerateConnectionStringDelegate = _ => "FirstTestMetadataConnectionString",
                ProviderName = "SimpleTestMeta"
            },
            new TestDatabaseProviderMetadata
            {
                Id = Guid.Parse("C5AB4E1D-B7E4-47E5-B1A4-C9343B5F59CA"),
                SortOrder = 2,
                DisplayName = "SecondMetadata",
                DefaultDatabaseName = "HostedTest",
                IsAvailable = true,
                RequiresServer = true,
                ServerPlaceholder = "SomeServerPlaceholder",
                RequiresCredentials = true,
                RequiresConnectionTest = true,
                ForceCreateDatabase = true,
                GenerateConnectionStringDelegate = _ => "HostedDatabaseConnectionString",
                ProviderName = "HostedTestMeta"
            },
        };

        return metadata;
    }

    #nullable enable
    public class TestDatabaseProviderMetadata : IDatabaseProviderMetadata
    {
        public Guid Id { get; set; }

        public int SortOrder { get; set; }

        public string DisplayName { get; set; } = string.Empty;

        public string DefaultDatabaseName { get; set; } = string.Empty;

        public string? ProviderName { get; set; }

        public bool SupportsQuickInstall { get; set; }

        public bool IsAvailable { get; set; }

        public bool RequiresServer { get; set; }

        public string? ServerPlaceholder { get; set; }

        public bool RequiresCredentials { get; set; }

        public bool SupportsIntegratedAuthentication { get; set; }

        public bool RequiresConnectionTest { get; set; }

        public bool ForceCreateDatabase { get; set; }

        public Func<DatabaseModel, string> GenerateConnectionStringDelegate { get; set; } =
            _ => "ConnectionString";

        public string? GenerateConnectionString(DatabaseModel databaseModel) => GenerateConnectionStringDelegate(databaseModel);
    }
}
