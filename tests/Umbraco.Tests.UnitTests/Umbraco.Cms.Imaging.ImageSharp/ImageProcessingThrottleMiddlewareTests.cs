// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Processors;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Imaging.ImageSharp;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Imaging.ImageSharp;

/// <summary>
/// Tests for <see cref="ImageProcessingThrottleMiddleware" />.
/// </summary>
[TestFixture]
public class ImageProcessingThrottleMiddlewareTests
{
    private const int Limit = 2;
    private const int RequestCount = 12;
    private const string ImagePath = "/media/image.jpg";

    [Test]
    public async Task InvokeAsync_ProcessingRequests_NeverExceedTheConfiguredLimit()
    {
        var probe = new ConcurrencyProbe();
        var middleware = CreateMiddleware(probe.HandleAsync);

        Task[] requests = Send(middleware, () => CreateContext(ImagePath, ("width", "400")));

        // Let every request reach the gate before any is allowed through.
        await WaitUntilAsync(() => probe.Current >= Limit);
        Assert.That(probe.Peak, Is.EqualTo(Limit));

        probe.Release();
        await Task.WhenAll(requests);

        Assert.That(probe.Peak, Is.EqualTo(Limit), "More images were processed concurrently than configured.");
    }

    /// <summary>
    /// None of these is something the imaging middleware will process, so none may queue behind it.
    /// </summary>
    [TestCase(ImagePath, "v", "1234")]
    [TestCase("/umbraco/management/api/v1/tree", "width", "400")]
    [TestCase("/export.csv", "format", "xlsx")]
    public Task InvokeAsync_NonProcessingRequests_AreNotThrottled(string path, string key, string value)
        => AssertAllRequestsPassThrough(CreateMiddleware, () => CreateContext(path, (key, value)));

    /// <summary>
    /// A request the imaging middleware serves from cache never reaches the decode hook, so it
    /// never takes a slot however many arrive at once. This is what gating at the decode buys over
    /// gating in the middleware, where a cache hit waited behind decodes.
    /// </summary>
    [Test]
    public Task InvokeAsync_RequestsServedWithoutDecoding_AreNotThrottled()
        => AssertAllRequestsPassThrough(
            CreateMiddleware,
            () => CreateContext(ImagePath, ("width", "400")),
            decoding: false);

    [Test]
    public async Task InvokeAsync_RequestsWithNoPath_AreNotThrottled()
    {
        var handled = false;
        var middleware = CreateMiddleware(_ =>
        {
            handled = true;
            return Task.CompletedTask;
        });

        // PathString.Empty exposes a null Value, which has to read as "not an image request"
        // rather than faulting the pipeline.
        var context = new DefaultHttpContext();
        context.Request.Path = PathString.Empty;
        context.Request.QueryString = QueryString.Create("width", "400");

        await middleware.InvokeAsync(context);

        Assert.That(handled, Is.True);
    }

    /// <summary>
    /// Ample memory for a single processor: the processor count already bounds concurrent decodes,
    /// so the gate steps aside rather than serialising requests the cache could serve.
    /// </summary>
    [Test]
    public Task InvokeAsync_WhenMemoryIsNotConstrained_DoesNotThrottle()
        => AssertAllRequestsPassThrough(CreateUnconstrainedMiddleware, () => CreateContext(ImagePath, ("width", "400")));

    /// <summary>
    /// An explicit limit is set, but the feature is switched off, so nothing is gated.
    /// </summary>
    [Test]
    public Task InvokeAsync_WhenDisabled_DoesNotThrottle()
        => AssertAllRequestsPassThrough(CreateDisabledMiddleware, () => CreateContext(ImagePath, ("width", "400")));

    [Test]
    public async Task InvokeAsync_ReleasesTheSlot_WhenTheRequestThrows()
    {
        var shouldThrow = true;
        var completed = false;

        // One middleware throughout, so the assertion is about this instance's semaphore.
        var middleware = CreateMiddleware(async context =>
        {
            await AcquireSlotAsync(context);

            if (shouldThrow)
            {
                throw new InvalidOperationException("Decoding failed.");
            }

            completed = true;
        });

        // Exactly Limit failures, so every slot is consumed. Going further would block here rather
        // than reaching the guarded assertion below.
        for (var i = 0; i < Limit; i++)
        {
            Assert.ThrowsAsync<InvalidOperationException>(
                () => middleware.InvokeAsync(CreateContext(ImagePath, ("width", "400"))));
        }

        shouldThrow = false;

        // Every slot would be leaked by now if the release were not in a finally, leaving the gate
        // permanently closed and this waiting forever.
        Task request = middleware.InvokeAsync(CreateContext(ImagePath, ("width", "400")));
        Task winner = await Task.WhenAny(request, Task.Delay(TimeSpan.FromSeconds(10)));

        Assert.That(winner, Is.SameAs(request), "The semaphore slot was not released after the failure.");
        await request;
        Assert.That(completed, Is.True);
    }

    /// <summary>
    /// A request that waits without getting a place is turned away rather than left hanging, and
    /// with a status a proxy in front of the site can act on.
    /// </summary>
    [Test]
    public async Task InvokeAsync_WhenTheWaitIsGivenUpOn_RespondsServiceUnavailable()
    {
        // Thrown from the decode hook in production; raised here from the pipeline it reaches.
        var middleware = CreateMiddleware(_ => throw new ImageProcessingUnavailableException("No place came free."));
        DefaultHttpContext context = CreateContext(ImagePath, ("width", "400"));

        await middleware.InvokeAsync(context);

        Assert.Multiple(() =>
        {
            Assert.That(context.Response.StatusCode, Is.EqualTo(StatusCodes.Status503ServiceUnavailable));
            Assert.That(context.Response.Headers.RetryAfter.ToString(), Is.Not.Empty);
        });
    }

    /// <summary>
    /// The place is taken at the decode, not for the whole request, so a request the imaging
    /// middleware serves from cache passes without waiting even while a decode holds the only place.
    /// Gating the whole request instead - all a gate ahead of the decode could otherwise do - would
    /// have made the cache hit queue behind the decode, which was the objection to doing this here.
    /// </summary>
    [Test]
    public async Task InvokeAsync_CacheHit_PassesWhileADecodeHoldsTheOnlyPlace()
    {
        var decodeStarted = new TaskCompletionSource();
        var releaseDecode = new TaskCompletionSource();

        // A limit of one, so a single decode holds the whole concurrency budget while it runs.
        var middleware = Build(
            async context =>
            {
                // A cache hit never reaches the decode hook, so it takes no place; a decode does.
                if (context.Request.Query.ContainsKey("cached"))
                {
                    return;
                }

                await AcquireSlotAsync(context);
                decodeStarted.SetResult();
                await releaseDecode.Task;
            },
            new ImagingMemorySettings { MaximumConcurrentProcessing = 1 });

        Task decode = middleware.InvokeAsync(CreateContext(ImagePath, ("width", "400")));
        await decodeStarted.Task;

        // The cache hit is a processing URL too - it just happens to be cached - so it is exactly
        // the request the old whole-request gate would have made wait. It must complete regardless.
        Task cacheHit = middleware.InvokeAsync(CreateContext(ImagePath, ("width", "400"), ("cached", "1")));
        Task winner = await Task.WhenAny(cacheHit, Task.Delay(TimeSpan.FromSeconds(10)));

        Assert.That(winner, Is.SameAs(cacheHit), "A cache hit waited behind a decode holding the concurrency limit.");
        await cacheHit;

        releaseDecode.SetResult();
        await decode;
    }

    /// <summary>
    /// A source too large to decode within the ceiling fails - that is what the ceiling is for - but
    /// the imaging library blames the image's dimensions, so the middleware names the setting
    /// responsible and lets the failure through rather than swallowing it.
    /// </summary>
    [Test]
    public void InvokeAsync_WhenADecodeExceedsItsCeiling_NamesTheSettingAndRethrows()
    {
        var logger = new CapturingLogger();
        var middleware = Build(
            async context =>
            {
                await AcquireSlotAsync(context);

                // How the imaging library surfaces an allocation stopped by the ceiling: the memory
                // failure wrapped as a complaint about the image's dimensions.
                throw new InvalidImageContentException(
                    "Failed to allocate buffers for possibly degenerate dimensions.",
                    new InvalidMemoryOperationException("Unable to allocate."));
            },
            new ImagingMemorySettings { MaximumConcurrentProcessing = Limit },
            logger: logger);

        Assert.ThrowsAsync<InvalidImageContentException>(
            () => middleware.InvokeAsync(CreateContext(ImagePath, ("width", "400"))));

        Assert.That(
            logger.Warnings,
            Has.One.Contains(nameof(ImagingMemorySettings.MaximumDecodedImageMegabytes)),
            "The warning must name the setting, since the imaging library attributes the failure to the image.");
    }

    private static ImageProcessingThrottleMiddleware CreateMiddleware(RequestDelegate next)
        => Build(next, new ImagingMemorySettings { MaximumConcurrentProcessing = Limit });

    private static ImageProcessingThrottleMiddleware CreateDisabledMiddleware(RequestDelegate next)
        => Build(next, new ImagingMemorySettings { Enabled = false, MaximumConcurrentProcessing = Limit });

    /// <summary>
    /// Derived settings (zero) against 2 GB and a single processor, so memory is not the binding
    /// constraint and no limit is enforced.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <returns>The middleware under test.</returns>
    private static ImageProcessingThrottleMiddleware CreateUnconstrainedMiddleware(RequestDelegate next)
        => Build(next, new ImagingMemorySettings(), availableMemoryBytes: 2048L * 1024 * 1024, processorCount: 1);

    private static ImageProcessingThrottleMiddleware Build(
        RequestDelegate next,
        ImagingMemorySettings memory,
        long? availableMemoryBytes = null,
        int? processorCount = null,
        ILogger<ImageProcessingThrottleMiddleware>? logger = null)
    {
        var settings = new ImagingSettings { Memory = memory };

        var processor = new Mock<IImageWebProcessor>();
        processor.SetupGet(x => x.Commands).Returns(new[] { "width", "height", "format" });
        IImageWebProcessor[] processors = { processor.Object };

        // The real utility, so the tests use the same supported-format set as runtime.
        var formatUtilities = new FormatUtilities(Options.Create(new ImageSharpMiddlewareOptions()));
        logger ??= NullLogger<ImageProcessingThrottleMiddleware>.Instance;

        return availableMemoryBytes is { } memoryBytes && processorCount is { } cores
            ? new ImageProcessingThrottleMiddleware(next, Options.Create(settings), processors, formatUtilities, logger, memoryBytes, cores)
            : new ImageProcessingThrottleMiddleware(next, Options.Create(settings), processors, formatUtilities, logger);
    }

    /// <summary>
    /// Takes the request's slot the way the imaging middleware's decode hook does.
    /// </summary>
    /// <param name="context">The request context.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    private static async Task AcquireSlotAsync(HttpContext context)
    {
        if (context.Items.TryGetValue(ImageProcessingSlot.HttpContextItemKey, out var value)
            && value is ImageProcessingSlot slot)
        {
            await slot.AcquireAsync(context.RequestAborted);
        }
    }

    private static async Task AssertAllRequestsPassThrough(
        Func<RequestDelegate, ImageProcessingThrottleMiddleware> create,
        Func<DefaultHttpContext> context,
        bool decoding = true)
    {
        var probe = new ConcurrencyProbe();
        var middleware = create(decoding ? probe.HandleAsync : probe.HandleWithoutDecodingAsync);

        Task[] requests = Send(middleware, context);

        await WaitUntilAsync(() => probe.Current >= RequestCount);

        probe.Release();
        await Task.WhenAll(requests);

        Assert.That(probe.Peak, Is.EqualTo(RequestCount));
    }

    private static Task[] Send(ImageProcessingThrottleMiddleware middleware, Func<DefaultHttpContext> context)
        => Enumerable
            .Range(0, RequestCount)
            .Select(_ => middleware.InvokeAsync(context()))
            .ToArray();

    private static DefaultHttpContext CreateContext(string path, params (string Key, string Value)[] query)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.QueryString = QueryString.Create(query.Select(x => new KeyValuePair<string, string?>(x.Key, x.Value)));
        return context;
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        while (!condition())
        {
            timeout.Token.ThrowIfCancellationRequested();
            await Task.Delay(10, timeout.Token);
        }
    }

    /// <summary>
    /// Keeps the rendered message of everything logged at warning, so a test can assert what a
    /// rejection or an over-ceiling decode reported without depending on the template's wording.
    /// </summary>
    private sealed class CapturingLogger : ILogger<ImageProcessingThrottleMiddleware>
    {
        private readonly List<string> _warnings = [];

        public IReadOnlyList<string> Warnings => _warnings;

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
            if (logLevel == LogLevel.Warning)
            {
                _warnings.Add(formatter(state, exception));
            }
        }
    }

    /// <summary>
    /// Holds every request inside the middleware until released, recording how many were in flight
    /// at once so a test can assert what the throttle actually allowed through.
    /// </summary>
    private sealed class ConcurrencyProbe
    {
        private readonly TaskCompletionSource _released = new();
        private int _current;
        private int _peak;

        public int Current => Volatile.Read(ref _current);

        public int Peak => Volatile.Read(ref _peak);

        public void Release() => _released.SetResult();

        /// <summary>
        /// Stands in for a decode, taking the request's slot before it is counted.
        /// </summary>
        public async Task HandleAsync(HttpContext context)
        {
            await AcquireSlotAsync(context);
            await RecordAsync();
        }

        /// <summary>
        /// Stands in for a cache hit, which never reaches the decode hook and so takes no slot.
        /// </summary>
        public Task HandleWithoutDecodingAsync(HttpContext context) => RecordAsync();

        private async Task RecordAsync()
        {
            RecordPeak(Interlocked.Increment(ref _current));

            await _released.Task;

            Interlocked.Decrement(ref _current);
        }

        private void RecordPeak(int value)
        {
            int current;
            while (value > (current = Volatile.Read(ref _peak)))
            {
                if (Interlocked.CompareExchange(ref _peak, value, current) == current)
                {
                    return;
                }
            }
        }
    }
}
