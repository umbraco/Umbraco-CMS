// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Web.Common.AspNetCore;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Website;

[TestFixture]
public class ApplicationUrlConfigurationNotificationHandlerTests
{
    private FakeLogger _logger = null!;

    [SetUp]
    public void SetUp() => _logger = new FakeLogger();

    [Test]
    public void Handle_WithExplicitUrl_LogsInformationWithConfiguredUrl()
    {
        var settings = new WebRoutingSettings { UmbracoApplicationUrl = "https://my-site.com" };
        var sut = CreateHandler(settings);

        sut.Handle(new UmbracoApplicationStartedNotification(false));

        Assert.That(_logger.LogEntries, Has.Exactly(1).Matches<FakeLogger.LogEntry>(
            e => e.Level == LogLevel.Information && e.Message.Contains("configured")));
    }

    [Test]
    public void Handle_NoneMode_NoExplicitUrl_LogsWarning()
    {
        var settings = new WebRoutingSettings { ApplicationUrlDetection = ApplicationUrlDetection.None };
        var sut = CreateHandler(settings);

        sut.Handle(new UmbracoApplicationStartedNotification(false));

        Assert.That(_logger.LogEntries, Has.Exactly(1).Matches<FakeLogger.LogEntry>(
            e => e.Level == LogLevel.Warning && e.Message.Contains("auto-detection is disabled")));
    }

    [Test]
    public void Handle_FirstRequestMode_NoExplicitUrl_LogsAutoDetectionEnabled()
    {
        var settings = new WebRoutingSettings { ApplicationUrlDetection = ApplicationUrlDetection.FirstRequest };
        var sut = CreateHandler(settings);

        sut.Handle(new UmbracoApplicationStartedNotification(false));

        Assert.That(_logger.LogEntries, Has.Exactly(1).Matches<FakeLogger.LogEntry>(
            e => e.Level == LogLevel.Information && e.Message.Contains("auto-detection is enabled")));
    }

    [Test]
    public void Handle_EveryRequestMode_NoExplicitUrl_LogsAutoDetectionEnabled()
    {
        var settings = new WebRoutingSettings { ApplicationUrlDetection = ApplicationUrlDetection.EveryRequest };
        var sut = CreateHandler(settings);

        sut.Handle(new UmbracoApplicationStartedNotification(false));

        Assert.That(_logger.LogEntries, Has.Exactly(1).Matches<FakeLogger.LogEntry>(
            e => e.Level == LogLevel.Information && e.Message.Contains("auto-detection is enabled")));
    }

    [Test]
    public void Handle_WithExplicitUrl_DoesNotLogWarning()
    {
        var settings = new WebRoutingSettings
        {
            UmbracoApplicationUrl = "https://my-site.com",
            ApplicationUrlDetection = ApplicationUrlDetection.None,
        };
        var sut = CreateHandler(settings);

        sut.Handle(new UmbracoApplicationStartedNotification(false));

        Assert.That(_logger.LogEntries, Has.None.Matches<FakeLogger.LogEntry>(
            e => e.Level == LogLevel.Warning));
    }

    private ApplicationUrlConfigurationNotificationHandler CreateHandler(WebRoutingSettings settings)
    {
        var optionsMonitor = Mock.Of<IOptionsMonitor<WebRoutingSettings>>(
            m => m.CurrentValue == settings);
        return new ApplicationUrlConfigurationNotificationHandler(_logger, optionsMonitor);
    }

    /// <summary>
    ///     Simple logger that captures log entries for assertion, avoiding Moq proxy issues
    ///     with <c>ILogger&lt;T&gt;</c> of internal types.
    /// </summary>
    private sealed class FakeLogger : ILogger<ApplicationUrlConfigurationNotificationHandler>
    {
        public List<LogEntry> LogEntries { get; } = [];

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => LogEntries.Add(new LogEntry(logLevel, formatter(state, exception)));

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public record LogEntry(LogLevel Level, string Message);
    }
}
