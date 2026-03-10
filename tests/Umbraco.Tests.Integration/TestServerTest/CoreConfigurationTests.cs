// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Persistence.Sqlite;
using Umbraco.Cms.Persistence.SqlServer;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.TestServerTest;

/// <summary>
/// Integration tests to verify that the Umbraco DI container can be built with different configurations.
/// These tests verify that AddCore(), AddBackOffice(), AddWebsite(), and AddDeliveryApi() can be
/// called in various combinations without throwing exceptions during service registration.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Logger = UmbracoTestOptions.Logger.Console)]
public class CoreConfigurationTests : UmbracoIntegrationTestBase
{
    /// <summary>
    /// Verifies that AddCore() can be called and all core services are registered.
    /// </summary>
    [Test]
    public void AddCore_RegistersRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(InMemoryConfiguration)
            .Build();

        // Register IConfiguration which is required by many services
        services.AddSingleton<IConfiguration>(configuration);

        TypeLoader typeLoader = services.AddTypeLoader(
            GetType().Assembly,
            TestHelper.ConsoleLoggerFactory,
            configuration);

        var builder = new UmbracoBuilder(
            services,
            configuration,
            typeLoader,
            TestHelper.ConsoleLoggerFactory,
            TestHelper.Profiler,
            AppCaches.NoCache);

        // Act - Register core services only (no backoffice)
        builder
            .AddCore()
            .AddUmbracoSqlServerSupport()
            .AddUmbracoSqliteSupport();

        builder.Build();

        // Assert - Key services should be registered
        var provider = services.BuildServiceProvider();
        Assert.DoesNotThrow(() => provider.GetRequiredService<IRuntimeState>());
    }

    /// <summary>
    /// Verifies that AddBackOffice() calls AddCore() internally and registers backoffice services.
    /// </summary>
    [Test]
    public void AddBackOffice_CallsAddCoreInternally()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(InMemoryConfiguration)
            .Build();

        // Register IConfiguration which is required by many services
        services.AddSingleton<IConfiguration>(configuration);

        TypeLoader typeLoader = services.AddTypeLoader(
            GetType().Assembly,
            TestHelper.ConsoleLoggerFactory,
            configuration);

        var builder = new UmbracoBuilder(
            services,
            configuration,
            typeLoader,
            TestHelper.ConsoleLoggerFactory,
            TestHelper.Profiler,
            AppCaches.NoCache);

        // Act - Register backoffice (which should call AddCore internally)
        builder
            .AddBackOffice()
            .AddUmbracoSqlServerSupport()
            .AddUmbracoSqliteSupport();

        builder.Build();

        // Assert - Both core and backoffice services should be registered
        var provider = services.BuildServiceProvider();
        Assert.DoesNotThrow(() => provider.GetRequiredService<IRuntimeState>());
        // Verify backoffice marker is registered
        Assert.That(
            services.Any(s => s.ServiceType == typeof(IBackOfficeEnabledMarker)),
            Is.True,
            "IBackOfficeEnabledMarker should be registered when AddBackOffice() is called");
    }

    /// <summary>
    /// Verifies that AddCore() followed by AddWebsite() works without AddBackOffice().
    /// </summary>
    [Test]
    public void AddCore_WithWebsite_RegistersWebsiteServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(InMemoryConfiguration)
            .Build();

        // Register IConfiguration which is required by many services
        services.AddSingleton<IConfiguration>(configuration);

        TypeLoader typeLoader = services.AddTypeLoader(
            GetType().Assembly,
            TestHelper.ConsoleLoggerFactory,
            configuration);

        var builder = new UmbracoBuilder(
            services,
            configuration,
            typeLoader,
            TestHelper.ConsoleLoggerFactory,
            TestHelper.Profiler,
            AppCaches.NoCache);

        // Act - Register core + website (no backoffice)
        builder
            .AddCore()
            .AddWebsite()
            .AddUmbracoSqlServerSupport()
            .AddUmbracoSqliteSupport();

        builder.Build();

        // Assert - Core services should be registered, no backoffice marker
        var provider = services.BuildServiceProvider();
        Assert.DoesNotThrow(() => provider.GetRequiredService<IRuntimeState>());
        Assert.That(
            services.Any(s => s.ServiceType == typeof(IBackOfficeEnabledMarker)),
            Is.False,
            "IBackOfficeEnabledMarker should NOT be registered when only AddCore() + AddWebsite() are called");
    }

    /// <summary>
    /// Verifies that AddCore() followed by AddDeliveryApi() works without AddBackOffice().
    /// </summary>
    [Test]
    public void AddCore_WithDeliveryApi_RegistersDeliveryApiServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(InMemoryConfiguration)
            .Build();

        // Register IConfiguration which is required by many services
        services.AddSingleton<IConfiguration>(configuration);

        TypeLoader typeLoader = services.AddTypeLoader(
            GetType().Assembly,
            TestHelper.ConsoleLoggerFactory,
            configuration);

        var builder = new UmbracoBuilder(
            services,
            configuration,
            typeLoader,
            TestHelper.ConsoleLoggerFactory,
            TestHelper.Profiler,
            AppCaches.NoCache);

        // Act - Register core + delivery API (no backoffice)
        builder
            .AddCore()
            .AddDeliveryApi()
            .AddUmbracoSqlServerSupport()
            .AddUmbracoSqliteSupport();

        builder.Build();

        // Assert - Core services should be registered, no backoffice marker
        var provider = services.BuildServiceProvider();
        Assert.DoesNotThrow(() => provider.GetRequiredService<IRuntimeState>());
        Assert.That(
            services.Any(s => s.ServiceType == typeof(IBackOfficeEnabledMarker)),
            Is.False,
            "IBackOfficeEnabledMarker should NOT be registered when only AddCore() + AddDeliveryApi() are called");
    }

    /// <summary>
    /// Verifies that AddCore() is idempotent - calling it multiple times doesn't cause issues.
    /// </summary>
    [Test]
    public void AddCore_IsIdempotent()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(InMemoryConfiguration)
            .Build();

        // Register IConfiguration which is required by many services
        services.AddSingleton<IConfiguration>(configuration);

        TypeLoader typeLoader = services.AddTypeLoader(
            GetType().Assembly,
            TestHelper.ConsoleLoggerFactory,
            configuration);

        var builder = new UmbracoBuilder(
            services,
            configuration,
            typeLoader,
            TestHelper.ConsoleLoggerFactory,
            TestHelper.Profiler,
            AppCaches.NoCache);

        // Act - Call AddCore multiple times
        builder
            .AddCore()
            .AddCore()  // Second call should be no-op
            .AddUmbracoSqlServerSupport()
            .AddUmbracoSqliteSupport();

        // Assert - Should not throw
        Assert.DoesNotThrow(() => builder.Build());
    }

    /// <summary>
    /// Verifies that calling both AddBackOffice() and AddCore() is safe due to idempotency.
    /// </summary>
    [Test]
    public void AddBackOffice_ThenAddCore_IsSafe()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(InMemoryConfiguration)
            .Build();

        // Register IConfiguration which is required by many services
        services.AddSingleton<IConfiguration>(configuration);

        TypeLoader typeLoader = services.AddTypeLoader(
            GetType().Assembly,
            TestHelper.ConsoleLoggerFactory,
            configuration);

        var builder = new UmbracoBuilder(
            services,
            configuration,
            typeLoader,
            TestHelper.ConsoleLoggerFactory,
            TestHelper.Profiler,
            AppCaches.NoCache);

        // Act - Call AddBackOffice first (which calls AddCore internally), then AddCore again
        builder
            .AddBackOffice()
            .AddCore()  // Should be no-op due to idempotency
            .AddUmbracoSqlServerSupport()
            .AddUmbracoSqliteSupport();

        // Assert - Should not throw
        Assert.DoesNotThrow(() => builder.Build());
    }

    /// <summary>
    /// Verifies that calling AddCore() then AddBackOffice() is safe.
    /// </summary>
    [Test]
    public void AddCore_ThenAddBackOffice_IsSafe()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(InMemoryConfiguration)
            .Build();

        // Register IConfiguration which is required by many services
        services.AddSingleton<IConfiguration>(configuration);

        TypeLoader typeLoader = services.AddTypeLoader(
            GetType().Assembly,
            TestHelper.ConsoleLoggerFactory,
            configuration);

        var builder = new UmbracoBuilder(
            services,
            configuration,
            typeLoader,
            TestHelper.ConsoleLoggerFactory,
            TestHelper.Profiler,
            AppCaches.NoCache);

        // Act - Call AddCore first, then AddBackOffice
        builder
            .AddCore()
            .AddBackOffice()  // AddCore inside should be no-op
            .AddUmbracoSqlServerSupport()
            .AddUmbracoSqliteSupport();

        // Assert - Should not throw, and backoffice marker should be registered
        Assert.DoesNotThrow(() => builder.Build());
        Assert.That(
            services.Any(s => s.ServiceType == typeof(IBackOfficeEnabledMarker)),
            Is.True,
            "IBackOfficeEnabledMarker should be registered when AddBackOffice() is called");
    }

    /// <summary>
    /// Verifies the complete delivery-only scenario with website and delivery API.
    /// </summary>
    [Test]
    public void DeliveryOnlyScenario_RegistersAllRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(InMemoryConfiguration)
            .Build();

        // Register IConfiguration which is required by many services
        services.AddSingleton<IConfiguration>(configuration);

        TypeLoader typeLoader = services.AddTypeLoader(
            GetType().Assembly,
            TestHelper.ConsoleLoggerFactory,
            configuration);

        var builder = new UmbracoBuilder(
            services,
            configuration,
            typeLoader,
            TestHelper.ConsoleLoggerFactory,
            TestHelper.Profiler,
            AppCaches.NoCache);

        // Act - Typical delivery-only setup
        builder
            .AddCore()
            .AddWebsite()
            .AddDeliveryApi()
            .AddUmbracoSqlServerSupport()
            .AddUmbracoSqliteSupport()
            .AddComposers();

        builder.Build();

        // Assert
        var provider = services.BuildServiceProvider();

        // Core services should be available
        Assert.DoesNotThrow(() => provider.GetRequiredService<IRuntimeState>());

        // Backoffice marker should NOT be registered
        Assert.That(
            services.Any(s => s.ServiceType == typeof(IBackOfficeEnabledMarker)),
            Is.False,
            "IBackOfficeEnabledMarker should NOT be registered in delivery-only scenario");
    }
}
