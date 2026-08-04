// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Processors;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Imaging.ImageSharp;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Imaging.ImageSharp;

[TestFixture]
public class ImageProcessingThrottleMiddlewareTests
{
    private const int Limit = 2;
    private const int RequestCount = 12;

    [Test]
    public async Task InvokeAsync_ProcessingRequests_NeverExceedTheConfiguredLimit()
    {
        var concurrency = 0;
        var observedPeak = 0;
        var released = new TaskCompletionSource();

        var middleware = CreateMiddleware(async _ =>
        {
            var current = Interlocked.Increment(ref concurrency);
            InterlockedMax(ref observedPeak, current);

            await released.Task;

            Interlocked.Decrement(ref concurrency);
        });

        Task[] requests = Enumerable
            .Range(0, RequestCount)
            .Select(_ => middleware.InvokeAsync(CreateContext(("width", "400"))))
            .ToArray();

        // Give every request the chance to reach the gate before letting any through.
        await WaitUntilAsync(() => Volatile.Read(ref concurrency) >= Limit);
        Assert.That(Volatile.Read(ref observedPeak), Is.EqualTo(Limit));

        released.SetResult();
        await Task.WhenAll(requests);

        Assert.That(observedPeak, Is.EqualTo(Limit), "More images were processed concurrently than configured.");
    }

    [Test]
    public async Task InvokeAsync_RequestsWithoutProcessingCommands_AreNotThrottled()
    {
        var concurrency = 0;
        var observedPeak = 0;
        var released = new TaskCompletionSource();

        var middleware = CreateMiddleware(async _ =>
        {
            var current = Interlocked.Increment(ref concurrency);
            InterlockedMax(ref observedPeak, current);

            await released.Task;

            Interlocked.Decrement(ref concurrency);
        });

        Task[] requests = Enumerable
            .Range(0, RequestCount)
            .Select(_ => middleware.InvokeAsync(CreateContext(("v", "1234"))))
            .ToArray();

        await WaitUntilAsync(() => Volatile.Read(ref concurrency) >= RequestCount);

        released.SetResult();
        await Task.WhenAll(requests);

        Assert.That(observedPeak, Is.EqualTo(RequestCount));
    }

    [Test]
    public async Task InvokeAsync_RequestsWithoutAFileExtension_AreNotThrottled()
    {
        var concurrency = 0;
        var observedPeak = 0;
        var released = new TaskCompletionSource();

        var middleware = CreateMiddleware(async _ =>
        {
            var current = Interlocked.Increment(ref concurrency);
            InterlockedMax(ref observedPeak, current);

            await released.Task;

            Interlocked.Decrement(ref concurrency);
        });

        // An API call that happens to carry a "width" must not queue behind image processing.
        Task[] requests = Enumerable
            .Range(0, RequestCount)
            .Select(_ => middleware.InvokeAsync(CreateContext("/umbraco/management/api/v1/tree", ("width", "400"))))
            .ToArray();

        await WaitUntilAsync(() => Volatile.Read(ref concurrency) >= RequestCount);

        released.SetResult();
        await Task.WhenAll(requests);

        Assert.That(observedPeak, Is.EqualTo(RequestCount));
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
                () => middleware.InvokeAsync(CreateContext(("width", "400"))));
        }

        shouldThrow = false;

        // Every slot would be leaked by now if the release were not in a finally, leaving the gate
        // permanently closed and this waiting forever.
        Task request = middleware.InvokeAsync(CreateContext(("width", "400")));
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

        return new ImageProcessingThrottleMiddleware(
            next,
            Options.Create(settings),
            new IImageWebProcessor[] { new TestImageWebProcessor() });
    }

    private static DefaultHttpContext CreateContext(params (string Key, string Value)[] query)
        => CreateContext("/media/image.jpg", query);

    private static DefaultHttpContext CreateContext(string path, params (string Key, string Value)[] query)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.QueryString = QueryString.Create(query.Select(x => new KeyValuePair<string, string?>(x.Key, x.Value)));
        return context;
    }

    private static void InterlockedMax(ref int target, int value)
    {
        int current;
        while (value > (current = Volatile.Read(ref target)))
        {
            if (Interlocked.CompareExchange(ref target, value, current) == current)
            {
                return;
            }
        }
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

    private sealed class TestImageWebProcessor : IImageWebProcessor
    {
        public IEnumerable<string> Commands { get; } = new[] { "width", "height" };

        public FormattedImage Process(
            FormattedImage image,
            ILogger logger,
            CommandCollection commands,
            CommandParser parser,
            CultureInfo culture) => image;

        public bool RequiresTrueColorPixelFormat(CommandCollection commands, CommandParser parser, CultureInfo culture)
            => false;
    }
}
