// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using SixLabors.ImageSharp.Memory;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Imaging.ImageSharp;
using Configuration = SixLabors.ImageSharp.Configuration;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Imaging.ImageSharp;

/// <summary>
/// Tests for <see cref="ImageProcessingMemory" />.
/// </summary>
/// <remarks>
/// The allocator being configured is process-wide, so these must not run alongside anything else
/// that reads it.
/// </remarks>
[TestFixture]
[NonParallelizable]
public class ImageProcessingMemoryTests
{
    private const long OneMegabyte = 1024 * 1024;

    /// <summary>
    /// Gets the value of memory available in bytes in a constrained environment, below the threshold at which the bounds engage.
    /// </summary>
    private const long ConstrainedMemoryBytes = 512 * OneMegabyte;

    /// <summary>
    /// Gets the value of memory available in bytes in an unconstrained environment, far above the threshold at which the bounds engage.
    /// </summary>
    private const long AmpleMemoryBytes = 16384 * OneMegabyte;

    private MemoryAllocator _originalAllocator = null!;

    /// <summary>
    /// Captures the allocator in use before a test replaces it.
    /// </summary>
    [SetUp]
    public void SetUp() => _originalAllocator = Configuration.Default.MemoryAllocator;

    /// <summary>
    /// Puts the process back as it was, or every later test inherits the allocator built here.
    /// </summary>
    [TearDown]
    public void TearDown() => Configuration.Default.MemoryAllocator = _originalAllocator;

    [Test]
    public void Configure_WhenMemoryIsConstrained_ReplacesTheAllocator()
    {
        LogCapture logs = Configure(new ImagingMemorySettings(), ConstrainedMemoryBytes);

        Assert.Multiple(() =>
        {
            Assert.That(Configuration.Default.MemoryAllocator, Is.Not.SameAs(_originalAllocator));
            Assert.That(logs.Single().Level, Is.EqualTo(LogLevel.Information));
        });
    }

    [Test]
    public void Configure_WhenMemoryIsConstrained_ReportsTheResolvedBounds()
    {
        var settings = new ImagingMemorySettings();

        LogCapture logs = Configure(settings, ConstrainedMemoryBytes);

        // Asserted against the settings rather than against literals, so this covers the wiring
        // between the two rather than restating the derivation the settings tests already cover.
        Assert.Multiple(() =>
        {
            Assert.That(
                logs.Single().Properties["MaximumPoolSizeMegabytes"],
                Is.EqualTo(settings.ResolveMaximumPoolSizeMegabytes(ConstrainedMemoryBytes)));
            Assert.That(
                logs.Single().Properties["MaximumDecodedImageMegabytes"],
                Is.EqualTo(settings.ResolveMaximumDecodedImageMegabytes(ConstrainedMemoryBytes)));
            Assert.That(logs.Single().Properties["AvailableMemoryMegabytes"], Is.EqualTo(512L));
        });
    }

    [Test]
    public void Configure_WhenMemoryIsAmple_LeavesTheAllocatorAlone()
    {
        LogCapture logs = Configure(new ImagingMemorySettings(), AmpleMemoryBytes);

        Assert.Multiple(() =>
        {
            Assert.That(Configuration.Default.MemoryAllocator, Is.SameAs(_originalAllocator));
            Assert.That(logs.Single().Level, Is.EqualTo(LogLevel.Debug));
        });
    }

    /// <summary>
    /// Explicit bounds, but the feature switched off: neither may be applied.
    /// </summary>
    [Test]
    public void Configure_WhenDisabled_LeavesTheAllocatorAlone()
    {
        var settings = new ImagingMemorySettings
        {
            Enabled = false,
            MaximumPoolSizeMegabytes = 128,
            MaximumDecodedImageMegabytes = 512,
        };

        LogCapture logs = Configure(settings, ConstrainedMemoryBytes);

        Assert.Multiple(() =>
        {
            Assert.That(Configuration.Default.MemoryAllocator, Is.SameAs(_originalAllocator));
            Assert.That(logs.Single().Level, Is.EqualTo(LogLevel.Debug));
        });
    }

    /// <summary>
    /// With ample memory only the explicitly configured bound engages, and the other is reported as
    /// unset so the imaging library keeps its own default for it.
    /// </summary>
    [Test]
    public void Configure_WhenOnlyOneBoundApplies_ReportsTheOtherAsUnset()
    {
        var settings = new ImagingMemorySettings { MaximumPoolSizeMegabytes = 128 };

        LogCapture logs = Configure(settings, AmpleMemoryBytes);

        Assert.Multiple(() =>
        {
            Assert.That(Configuration.Default.MemoryAllocator, Is.Not.SameAs(_originalAllocator));
            Assert.That(logs.Single().Properties["MaximumPoolSizeMegabytes"], Is.EqualTo(128));
            Assert.That(logs.Single().Properties["MaximumDecodedImageMegabytes"], Is.Null);
        });
    }

    /// <summary>
    /// The settings have to come from <c>IOptions</c>, not straight from <c>IConfiguration</c>: a
    /// composer contributing them in code reaches the former only.
    /// </summary>
    [Test]
    public void Configure_HonoursSettingsContributedInCode()
    {
        var logs = new LogCapture();
        using ServiceProvider services = new ServiceCollection()
            .AddSingleton<ILoggerFactory>(logs)
            .Configure<ImagingSettings>(x => x.Memory.MaximumPoolSizeMegabytes = 128)
            .PostConfigure<ImagingSettings>(x => x.Memory.Enabled = false)
            .BuildServiceProvider();

        ImageProcessingMemory.Configure(services, ConstrainedMemoryBytes);

        Assert.Multiple(() =>
        {
            Assert.That(Configuration.Default.MemoryAllocator, Is.SameAs(_originalAllocator));
            Assert.That(logs.Single().Level, Is.EqualTo(LogLevel.Debug));
        });
    }

    /// <summary>
    /// Runs the configuration against a provider holding nothing but the settings and the logger,
    /// so reading either from anywhere else would fail rather than silently diverge.
    /// </summary>
    /// <param name="memory">The imaging memory settings to apply.</param>
    /// <param name="availableMemoryBytes">The memory to report as available to the process.</param>
    /// <returns>What was logged.</returns>
    private static LogCapture Configure(ImagingMemorySettings memory, long availableMemoryBytes)
    {
        var logs = new LogCapture();
        using ServiceProvider services = new ServiceCollection()
            .AddSingleton<ILoggerFactory>(logs)
            .Configure<ImagingSettings>(x => x.Memory = memory)
            .BuildServiceProvider();

        ImageProcessingMemory.Configure(services, availableMemoryBytes);

        return logs;
    }

    /// <summary>
    /// Records what was logged, keeping the structured properties rather than the rendered message
    /// so the assertions do not depend on the wording of the templates.
    /// </summary>
    private sealed class LogCapture : ILoggerFactory, ILogger
    {
        private readonly List<Entry> _entries = [];

        public Entry Single()
        {
            Assert.That(_entries, Has.Count.EqualTo(1), "Expected exactly one log entry.");
            return _entries[0];
        }

        public ILogger CreateLogger(string categoryName) => this;

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
            => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Dictionary<string, object?> properties = state is IReadOnlyList<KeyValuePair<string, object?>> values
                ? values.ToDictionary(x => x.Key, x => x.Value)
                : [];

            _entries.Add(new Entry(logLevel, properties));
        }

        public void Dispose()
        {
        }

        internal sealed record Entry(LogLevel Level, IReadOnlyDictionary<string, object?> Properties);
    }
}
