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
                Is.EqualTo(ImageProcessingMemory.ResolveMaximumPoolSizeMegabytes(settings, ConstrainedMemoryBytes)));
            Assert.That(
                logs.Single().Properties["MaximumDecodedImageMegabytes"],
                Is.EqualTo(ImageProcessingMemory.ResolveMaximumDecodedImageMegabytes(settings, ConstrainedMemoryBytes)));
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

    [Test]
    public void ResolveMaximumPoolSizeMegabytes_WhenConfigured_UsesConfiguredValue()
    {
        var settings = new ImagingMemorySettings { MaximumPoolSizeMegabytes = 256 };

        Assert.That(ImageProcessingMemory.ResolveMaximumPoolSizeMegabytes(settings, 512 * OneMegabyte), Is.EqualTo(256));
    }

    [TestCase(512, 16)] // Clamped to the minimum.
    [TestCase(2048, 64)]
    [TestCase(16384, 64)] // Clamped to the maximum.
    public void ResolveMaximumPoolSizeMegabytes_WhenNotConfigured_DerivesFromAvailableMemory(
        int availableMegabytes,
        int expected)
    {
        var settings = new ImagingMemorySettings();

        Assert.That(ImageProcessingMemory.ResolveMaximumPoolSizeMegabytes(settings, availableMegabytes * OneMegabyte), Is.EqualTo(expected));
    }

    [Test]
    public void ResolveMaximumPoolSizeMegabytes_StaysWellBelowTheImageSharpDefault()
    {
        // ImageSharp defaults to an eighth of available memory on a 64-bit process, which is what
        // leaves a container sitting far above its working set at rest.
        const long available = 2048 * OneMegabyte;
        var imageSharpDefaultMegabytes = (int)(available / 8 / OneMegabyte);

        var resolved = ImageProcessingMemory.ResolveMaximumPoolSizeMegabytes(new ImagingMemorySettings(), available);

        Assert.That(resolved, Is.LessThan(imageSharpDefaultMegabytes));
    }

    [Test]
    public void ResolveMaximumConcurrentProcessing_WhenConfigured_UsesConfiguredValue()
    {
        var settings = new ImagingMemorySettings { MaximumConcurrentProcessing = 12 };

        Assert.That(ImageProcessingMemory.ResolveMaximumConcurrentProcessing(settings, 512 * OneMegabyte, 4), Is.EqualTo(12));
    }

    [TestCase(512, 32, 4)]
    [TestCase(1024, 32, 8)]
    [TestCase(2048, 32, 16)]
    public void ResolveMaximumConcurrentProcessing_WhenNotConfigured_DerivesFromAvailableMemory(
        int availableMegabytes,
        int processorCount,
        int expected)
    {
        var settings = new ImagingMemorySettings();

        Assert.That(
            ImageProcessingMemory.ResolveMaximumConcurrentProcessing(settings, availableMegabytes * OneMegabyte, processorCount),
            Is.EqualTo(expected));
    }

    [Test]
    public void ResolveMaximumConcurrentProcessing_IsCappedByProcessorCount()
    {
        var settings = new ImagingMemorySettings();

        Assert.That(ImageProcessingMemory.ResolveMaximumConcurrentProcessing(settings, 64L * 1024 * OneMegabyte, 4), Is.EqualTo(4));
    }

    [Test]
    public void ResolveMaximumConcurrentProcessing_NeverReturnsLessThanOne()
    {
        var settings = new ImagingMemorySettings();

        Assert.That(ImageProcessingMemory.ResolveMaximumConcurrentProcessing(settings, 16 * OneMegabyte, 1), Is.EqualTo(1));
    }

    // A container, or a small VM, where what the library retains at rest competes with the memory
    // the rest of the site needs.
    [TestCase(384)]
    [TestCase(2048)]
    public void RequiresPoolSizeLimit_WhenMemoryIsLow_IsTrue(int availableMegabytes)
    {
        var settings = new ImagingMemorySettings();

        Assert.That(ImageProcessingMemory.RequiresPoolSizeLimit(settings, availableMegabytes * OneMegabyte), Is.True);
    }

    // Enough memory that an eighth of it, which is what the library keeps by default on a 64-bit
    // process, is not worth reclaiming - so an upgrade must not change how it allocates.
    [TestCase(4096)]
    [TestCase(65536)]
    public void RequiresPoolSizeLimit_WhenMemoryIsAmple_IsFalse(int availableMegabytes)
    {
        var settings = new ImagingMemorySettings();

        Assert.That(ImageProcessingMemory.RequiresPoolSizeLimit(settings, availableMegabytes * OneMegabyte), Is.False);
    }

    [Test]
    public void RequiresPoolSizeLimit_WhenConfigured_IsTrueEvenWithAmpleMemory()
    {
        var settings = new ImagingMemorySettings { MaximumPoolSizeMegabytes = 128 };

        Assert.That(ImageProcessingMemory.RequiresPoolSizeLimit(settings, 65536 * OneMegabyte), Is.True);
    }

    [Test]
    public void RequiresPoolSizeLimit_WhenDisabled_IsFalse()
    {
        var settings = new ImagingMemorySettings { Enabled = false, MaximumPoolSizeMegabytes = 128 };

        Assert.That(ImageProcessingMemory.RequiresPoolSizeLimit(settings, 384 * OneMegabyte), Is.False);
    }

    [Test]
    public void ResolveMaximumDecodedImageMegabytes_WhenConfigured_UsesConfiguredValue()
    {
        var settings = new ImagingMemorySettings { MaximumDecodedImageMegabytes = 96 };

        Assert.That(ImageProcessingMemory.ResolveMaximumDecodedImageMegabytes(settings, 512 * OneMegabyte), Is.EqualTo(96));
    }

    [TestCase(512, 256)] // Clamped to the minimum.
    [TestCase(2048, 512)]
    [TestCase(8192, 1024)] // Clamped to the maximum.
    public void ResolveMaximumDecodedImageMegabytes_WhenNotConfigured_DerivesFromAvailableMemory(
        int availableMegabytes,
        int expected)
    {
        var settings = new ImagingMemorySettings();

        Assert.That(
            ImageProcessingMemory.ResolveMaximumDecodedImageMegabytes(settings, availableMegabytes * OneMegabyte),
            Is.EqualTo(expected));
    }

    // The bound has to clear any legitimate source comfortably, or a large upload starts failing
    // on a host that could have served it. A 12 megapixel decode is ~48 MB.
    [TestCase(384)]
    [TestCase(2048)]
    public void ResolveMaximumDecodedImageMegabytes_LeavesRoomForALargeSource(int availableMegabytes)
    {
        const int twelveMegapixelDecodeMegabytes = 48;

        var resolved = ImageProcessingMemory.ResolveMaximumDecodedImageMegabytes(new ImagingMemorySettings(), availableMegabytes * OneMegabyte);

        Assert.That(resolved, Is.GreaterThan(twelveMegapixelDecodeMegabytes * 4));
    }

    // Never looser than the library's own ceiling, which is a flat 1 GB even on a 32-bit process.
    [TestCase(384)]
    [TestCase(2048)]
    [TestCase(8192)]
    public void ResolveMaximumDecodedImageMegabytes_NeverExceedsTheImageSharpDefault(int availableMegabytes)
    {
        const int imageSharpDefaultMegabytes = 1024;

        var resolved = ImageProcessingMemory.ResolveMaximumDecodedImageMegabytes(new ImagingMemorySettings(), availableMegabytes * OneMegabyte);

        Assert.That(resolved, Is.LessThanOrEqualTo(imageSharpDefaultMegabytes));
    }

    [TestCase(384)]
    [TestCase(2048)]
    public void RequiresAllocationLimit_WhenMemoryIsLow_IsTrue(int availableMegabytes)
    {
        var settings = new ImagingMemorySettings();

        Assert.That(ImageProcessingMemory.RequiresAllocationLimit(settings, availableMegabytes * OneMegabyte), Is.True);
    }

    // Above the threshold the library's own ceiling stands, so a derived value would only loosen it.
    [TestCase(4096)]
    [TestCase(65536)]
    public void RequiresAllocationLimit_WhenMemoryIsAmple_IsFalse(int availableMegabytes)
    {
        var settings = new ImagingMemorySettings();

        Assert.That(ImageProcessingMemory.RequiresAllocationLimit(settings, availableMegabytes * OneMegabyte), Is.False);
    }

    [Test]
    public void RequiresAllocationLimit_WhenConfigured_IsTrueEvenWithAmpleMemory()
    {
        var settings = new ImagingMemorySettings { MaximumDecodedImageMegabytes = 512 };

        Assert.That(ImageProcessingMemory.RequiresAllocationLimit(settings, 65536 * OneMegabyte), Is.True);
    }

    [Test]
    public void RequiresAllocationLimit_WhenDisabled_IsFalse()
    {
        var settings = new ImagingMemorySettings { Enabled = false, MaximumDecodedImageMegabytes = 512 };

        Assert.That(ImageProcessingMemory.RequiresAllocationLimit(settings, 384 * OneMegabyte), Is.False);
    }

    [Test]
    public void RequiresConcurrencyLimit_WhenConfigured_IsAlwaysTrue()
    {
        var settings = new ImagingMemorySettings { MaximumConcurrentProcessing = 4 };

        // Explicit configuration is honoured even on a host with memory to spare.
        Assert.That(ImageProcessingMemory.RequiresConcurrencyLimit(settings, 64L * 1024 * OneMegabyte, 4), Is.True);
    }

    // Memory is the binding constraint: it affords fewer concurrent decodes than there are
    // processors, so an unbounded page of thumbnails would exhaust it.
    [TestCase(384, 28)] // Many cores, little memory - the container that gets OOM-killed.
    [TestCase(256, 8)]
    [TestCase(1024, 32)]
    public void RequiresConcurrencyLimit_WhenMemoryIsTheBindingConstraint_IsTrue(
        int availableMegabytes,
        int processorCount)
    {
        var settings = new ImagingMemorySettings();

        Assert.That(ImageProcessingMemory.RequiresConcurrencyLimit(settings, availableMegabytes * OneMegabyte, processorCount), Is.True);
    }

    // The processor count already bounds concurrent decodes below what memory could hold, so a
    // limit would only add latency without preventing anything.
    [TestCase(512, 4)] // Derived concurrency equals the processor count - not strictly constrained.
    [TestCase(2048, 4)]
    [TestCase(65536, 8)]
    public void RequiresConcurrencyLimit_WhenMemoryIsNotTheBindingConstraint_IsFalse(
        int availableMegabytes,
        int processorCount)
    {
        var settings = new ImagingMemorySettings();

        Assert.That(ImageProcessingMemory.RequiresConcurrencyLimit(settings, availableMegabytes * OneMegabyte, processorCount), Is.False);
    }

    [Test]
    public void RequiresConcurrencyLimit_WhenDisabled_IsFalse()
    {
        // Disabled wins over both an explicit limit and a memory-constrained host - the whole
        // feature is off.
        var settings = new ImagingMemorySettings { Enabled = false, MaximumConcurrentProcessing = 4 };

        Assert.That(ImageProcessingMemory.RequiresConcurrencyLimit(settings, 384 * OneMegabyte, 28), Is.False);
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
