// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Runtime.RuntimeModeValidators;

[TestFixture]
public class RazorRuntimeCompilationValidatorTests
{
    private const string FutureVersionWarning = "Umbraco 19";

    private FakeLogger _logger = null!;

    [SetUp]
    public void SetUp() => _logger = new FakeLogger();

    [TestCase(true, "Install the package")]
    [TestCase(false, "Install the package")]
    public void Handle_WhenRuntimeModeAllowsTheFactoryButThereIsNone_ReportsTheMissingPackage(bool modelsModeConfigured, string expectedRemedy)
    {
        // The runtime mode this models mode requires is also the default, so once it is in force the package
        // providing the factory is the only thing that can still be missing.
        CreateSut(Constants.ModelsBuilder.InMemoryAutoModelsMode, liveFactoryEnabled: false, modelsModeConfigured).Handle(Notification);

        Assert.That(_logger.LogEntries, Has.Exactly(1).Matches<FakeLogger.LogEntry>(
            e => e.Level == LogLevel.Error
                 && e.Message.Contains(expectedRemedy)
                 && !e.Message.Contains("Change the runtime mode")));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Handle_WhenRuntimeModeBlocksTheFactory_ReportsTheRuntimeModeInForce(bool modelsModeConfigured)
    {
        // The runtime mode blocks the factory before any package can supply it, so reporting a missing package
        // here would send the reader after the wrong thing.
        CreateSut(Constants.ModelsBuilder.InMemoryAutoModelsMode, liveFactoryEnabled: false, modelsModeConfigured, RuntimeMode.Development)
            .Handle(Notification);

        Assert.That(_logger.LogEntries, Has.Exactly(1).Matches<FakeLogger.LogEntry>(
            e => e.Level == LogLevel.Error
                 && e.Message.Contains($"currently {RuntimeMode.Development}")
                 && e.Message.Contains("Change the runtime mode")
                 && !e.Message.Contains("Install the package")));
    }

    [TestCase(RuntimeMode.BackofficeDevelopment)]
    [TestCase(RuntimeMode.Development)]
    public void Handle_WhenModeIsExplicitlyConfigured_WarnsOfTheFutureVersion(RuntimeMode runtimeMode)
    {
        CreateSut(Constants.ModelsBuilder.InMemoryAutoModelsMode, liveFactoryEnabled: false, modelsModeConfigured: true, runtimeMode).Handle(Notification);

        Assert.That(_logger.LogEntries, Has.Exactly(1).Matches<FakeLogger.LogEntry>(
            e => e.Message.Contains("configured to use") && e.Message.Contains(FutureVersionWarning)));
    }

    [TestCase(RuntimeMode.BackofficeDevelopment)]
    [TestCase(RuntimeMode.Development)]
    public void Handle_WhenModeIsDefaulted_DoesNotWarnOfTheFutureVersion(RuntimeMode runtimeMode)
    {
        CreateSut(Constants.ModelsBuilder.InMemoryAutoModelsMode, liveFactoryEnabled: false, modelsModeConfigured: false, runtimeMode).Handle(Notification);

        Assert.That(_logger.LogEntries, Has.Exactly(1).Matches<FakeLogger.LogEntry>(
            e => e.Message.Contains("using the default") && !e.Message.Contains(FutureVersionWarning)));
    }

    [Test]
    public void Handle_WhenModeCannotBeSatisfied_DoesNotThrow()
    {
        RazorRuntimeCompilationValidator sut = CreateSut(Constants.ModelsBuilder.InMemoryAutoModelsMode, liveFactoryEnabled: false, modelsModeConfigured: true);

        Assert.DoesNotThrow(() => sut.Handle(Notification));
    }

    [Test]
    public void Resolve_FromValidatingContainer_DoesNotThrow()
    {
        // The obsolete constructor kept for binary compatibility gives the type two public constructors, and
        // container activation with validation enabled is where an ambiguous choice between them would surface.
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddSingleton(Mock.Of<IOptionsMonitor<ModelsBuilderSettings>>(
            m => m.CurrentValue == new ModelsBuilderSettings()));
        services.AddSingleton(Mock.Of<IPublishedModelFactory>());
        services.AddTransient<RazorRuntimeCompilationValidator>();

        using ServiceProvider provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true,
        });

        Assert.DoesNotThrow(() => provider.GetRequiredService<RazorRuntimeCompilationValidator>());
    }

    private static UmbracoApplicationStartedNotification Notification => new(false);

    private RazorRuntimeCompilationValidator CreateSut(
        string modelsMode,
        bool liveFactoryEnabled,
        bool modelsModeConfigured,
        RuntimeMode? runtimeMode = null)
    {
        var settings = new ModelsBuilderSettings { ModelsMode = modelsMode };

        var configurationValues = new Dictionary<string, string?>();
        if (modelsModeConfigured)
        {
            configurationValues[Constants.Configuration.ConfigModelsMode] = modelsMode;
        }

        if (runtimeMode is not null)
        {
            configurationValues[Constants.Configuration.ConfigRuntimeMode] = runtimeMode.ToString();
        }

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues)
            .Build();

        return new RazorRuntimeCompilationValidator(
            Mock.Of<IOptionsMonitor<ModelsBuilderSettings>>(m => m.CurrentValue == settings),
            CreatePublishedModelFactory(liveFactoryEnabled),
            configuration,
            _logger);
    }

    private static IPublishedModelFactory CreatePublishedModelFactory(bool liveFactoryEnabled)
    {
        if (liveFactoryEnabled is false)
        {
            return Mock.Of<IPublishedModelFactory>();
        }

        var factory = new Mock<IAutoPublishedModelFactory>();
        factory.SetupGet(x => x.Enabled).Returns(true);
        return factory.Object;
    }

    /// <summary>
    ///     Captures log entries so the reported message can be asserted on.
    /// </summary>
    private sealed class FakeLogger : ILogger<RazorRuntimeCompilationValidator>
    {
        public List<LogEntry> LogEntries { get; } = [];

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => LogEntries.Add(new LogEntry(logLevel, formatter(state, exception)));

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => null;

        public record LogEntry(LogLevel Level, string Message);
    }
}
