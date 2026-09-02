// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SixLabors.ImageSharp.Web.Processors;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Imaging.ImageSharp;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Imaging.ImageSharp;

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

    // A file with no processing command, and an API call that happens to carry one: neither is
    // something the imaging middleware will process, so neither may queue behind it.
    [TestCase(ImagePath, "v", "1234")]
    [TestCase("/umbraco/management/api/v1/tree", "width", "400")]
    public async Task InvokeAsync_NonProcessingRequests_AreNotThrottled(string path, string key, string value)
    {
        var probe = new ConcurrencyProbe();
        var middleware = CreateMiddleware(probe.HandleAsync);

        Task[] requests = Send(middleware, () => CreateContext(path, (key, value)));

        await WaitUntilAsync(() => probe.Current >= RequestCount);

        probe.Release();
        await Task.WhenAll(requests);

        Assert.That(probe.Peak, Is.EqualTo(RequestCount));
    }

    [Test]
    public async Task InvokeAsync_RequestsWithNoPath_AreNotThrottled()
    {
        var handled = false;
        var middleware = CreateMiddleware(_ =>
        {
            handled = true;
            return Task.CompletedTask;
        });

        // PathString.Empty exposes a null Value, which the extension check has to treat as
        // "not an image request" rather than faulting the pipeline.
        var context = new DefaultHttpContext();
        context.Request.Path = PathString.Empty;
        context.Request.QueryString = QueryString.Create("width", "400");

        await middleware.InvokeAsync(context);

        Assert.That(handled, Is.True);
    }

    [Test]
    public async Task InvokeAsync_WhenMemoryIsNotConstrained_DoesNotThrottle()
    {
        var probe = new ConcurrencyProbe();

        // Ample memory for a single processor: the processor count already bounds concurrent
        // decodes, so the gate steps aside rather than serialising requests the cache could serve.
        var middleware = CreateUnconstrainedMiddleware(probe.HandleAsync);

        Task[] requests = Send(middleware, () => CreateContext(ImagePath, ("width", "400")));

        await WaitUntilAsync(() => probe.Current >= RequestCount);

        probe.Release();
        await Task.WhenAll(requests);

        Assert.That(probe.Peak, Is.EqualTo(RequestCount));
    }

    [Test]
    public async Task InvokeAsync_WhenDisabled_DoesNotThrottle()
    {
        var probe = new ConcurrencyProbe();

        // An explicit limit is set, but the feature is switched off, so nothing is gated.
        var middleware = CreateDisabledMiddleware(probe.HandleAsync);

        Task[] requests = Send(middleware, () => CreateContext(ImagePath, ("width", "400")));

        await WaitUntilAsync(() => probe.Current >= RequestCount);

        probe.Release();
        await Task.WhenAll(requests);

        Assert.That(probe.Peak, Is.EqualTo(RequestCount));
    }

    [Test]
    public async Task InvokeAsync_ReleasesTheSlot_WhenTheRequestThrows()
    {
        var shouldThrow = true;
        var completed = false;

        // One middleware throughout, so the assertion is about this instance's semaphore.
        var middleware = CreateMiddleware(_ =>
        {
            if (shouldThrow)
            {
                throw new InvalidOperationException("Decoding failed.");
            }

            completed = true;
            return Task.CompletedTask;
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

    private static ImageProcessingThrottleMiddleware CreateMiddleware(RequestDelegate next)
    {
        var settings = new ImagingSettings
        {
            Memory = new ImagingMemorySettings { MaximumConcurrentProcessing = Limit },
        };

        var processor = new Mock<IImageWebProcessor>();
        processor.SetupGet(x => x.Commands).Returns(new[] { "width", "height" });

        return new ImageProcessingThrottleMiddleware(
            next,
            Options.Create(settings),
            new[] { processor.Object });
    }

    private static ImageProcessingThrottleMiddleware CreateUnconstrainedMiddleware(RequestDelegate next)
    {
        // Derived settings (zero) against 2 GB and a single processor, so memory is not the binding
        // constraint and no limit is enforced.
        var settings = new ImagingSettings { Memory = new ImagingMemorySettings() };

        var processor = new Mock<IImageWebProcessor>();
        processor.SetupGet(x => x.Commands).Returns(new[] { "width", "height" });

        return new ImageProcessingThrottleMiddleware(
            next,
            Options.Create(settings),
            new[] { processor.Object },
            availableMemoryBytes: 2048L * 1024 * 1024,
            processorCount: 1);
    }

    private static ImageProcessingThrottleMiddleware CreateDisabledMiddleware(RequestDelegate next)
    {
        var settings = new ImagingSettings
        {
            Memory = new ImagingMemorySettings { Enabled = false, MaximumConcurrentProcessing = Limit },
        };

        var processor = new Mock<IImageWebProcessor>();
        processor.SetupGet(x => x.Commands).Returns(new[] { "width", "height" });

        return new ImageProcessingThrottleMiddleware(
            next,
            Options.Create(settings),
            new[] { processor.Object });
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

        public async Task HandleAsync(HttpContext context)
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
