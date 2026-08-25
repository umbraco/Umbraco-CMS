// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Configuration;
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
    private const string InMemoryAuto = "InMemoryAuto";
    private const string FutureVersionWarning = "Umbraco 19";

    private FakeLogger _logger = null!;

    [SetUp]
    public void SetUp() => _logger = new FakeLogger();

    [Test]
    public void Reports_An_Explicitly_Configured_Mode_That_No_Live_Factory_Can_Satisfy()
    {
        CreateSut(InMemoryAuto, liveFactoryEnabled: false, modelsModeConfigured: true).Handle(Notification);

        Assert.That(_logger.LogEntries, Has.Exactly(1).Matches<FakeLogger.LogEntry>(
            e => e.Level == LogLevel.Error
                 && e.Message.Contains("configured to use")
                 && e.Message.Contains(FutureVersionWarning)));
    }

    [Test]
    public void Reports_A_Defaulted_Mode_That_No_Live_Factory_Can_Satisfy_Without_Warning_Of_A_Future_Version()
    {
        CreateSut(InMemoryAuto, liveFactoryEnabled: false, modelsModeConfigured: false).Handle(Notification);

        Assert.That(_logger.LogEntries, Has.Exactly(1).Matches<FakeLogger.LogEntry>(
            e => e.Level == LogLevel.Error
                 && e.Message.Contains("using the default")
                 && !e.Message.Contains(FutureVersionWarning)));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Reports_Nothing_When_A_Live_Factory_Is_Available(bool modelsModeConfigured)
    {
        CreateSut(InMemoryAuto, liveFactoryEnabled: true, modelsModeConfigured).Handle(Notification);

        Assert.That(_logger.LogEntries, Is.Empty);
    }

    [TestCase(Constants.ModelsBuilder.ModelsModes.Nothing)]
    [TestCase(Constants.ModelsBuilder.ModelsModes.SourceCodeAuto)]
    [TestCase(Constants.ModelsBuilder.ModelsModes.SourceCodeManual)]
    public void Reports_Nothing_For_A_Mode_That_Does_Not_Require_A_Live_Factory(string modelsMode)
    {
        CreateSut(modelsMode, liveFactoryEnabled: false, modelsModeConfigured: true).Handle(Notification);

        Assert.That(_logger.LogEntries, Is.Empty);
    }

    [Test]
    public void Does_Not_Throw_When_The_Mode_Cannot_Be_Satisfied()
    {
        RazorRuntimeCompilationValidator sut = CreateSut(InMemoryAuto, liveFactoryEnabled: false, modelsModeConfigured: true);

        Assert.DoesNotThrow(() => sut.Handle(Notification));
    }

    private static UmbracoApplicationStartedNotification Notification => new(false);

    private RazorRuntimeCompilationValidator CreateSut(string modelsMode, bool liveFactoryEnabled, bool modelsModeConfigured)
    {
        var settings = new ModelsBuilderSettings { ModelsMode = modelsMode };

        var configurationValues = new Dictionary<string, string?>();
        if (modelsModeConfigured)
        {
            configurationValues[Constants.Configuration.ConfigModelsMode] = modelsMode;
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
