// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Commands.Converters;
using SixLabors.ImageSharp.Web.Middleware;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Imaging.ImageSharp;
using Configuration = SixLabors.ImageSharp.Configuration;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Imaging.ImageSharp;

[TestFixture]
public class ConfigureImageSharpMiddlewareOptionsTests
{
    // The decode hook is what engages the throttle, so nothing is bounded at all if it is not wired
    // up - a failure that would otherwise only show as an OOM under load.
    [Test]
    public async Task Configure_OnBeforeLoadAsync_TakesTheRequestSlot()
    {
        ImageSharpMiddlewareOptions options = Configure();

        using var semaphore = new SemaphoreSlim(2, 2);
        var slot = new ImageProcessingSlot(semaphore);
        ImageCommandContext context = CreateContext(slot);

        await options.OnBeforeLoadAsync(context, Configuration.Default);

        Assert.That(semaphore.CurrentCount, Is.EqualTo(1));
    }

    [Test]
    public async Task Configure_OnBeforeLoadAsync_WhenNoSlotWasPublished_Continues()
    {
        ImageSharpMiddlewareOptions options = Configure();

        // A request the middleware left ungated - it is not throttled, and must still decode.
        ImageCommandContext context = CreateContext(slot: null);

        DecoderOptions? decoderOptions = await options.OnBeforeLoadAsync(context, Configuration.Default);

        Assert.That(decoderOptions, Is.Null);
    }

    // Whatever was configured before must still run, so the throttle does not silently drop another
    // component's decoder options.
    [Test]
    public async Task Configure_OnBeforeLoadAsync_KeepsThePreviouslyConfiguredHook()
    {
        var expected = new DecoderOptions();
        var options = new ImageSharpMiddlewareOptions
        {
            OnBeforeLoadAsync = (_, _) => Task.FromResult<DecoderOptions?>(expected),
        };

        Configure(options);

        using var semaphore = new SemaphoreSlim(1, 1);
        ImageCommandContext context = CreateContext(new ImageProcessingSlot(semaphore));

        DecoderOptions? decoderOptions = await options.OnBeforeLoadAsync(context, Configuration.Default);

        Assert.Multiple(() =>
        {
            Assert.That(decoderOptions, Is.SameAs(expected));
            Assert.That(semaphore.CurrentCount, Is.Zero);
        });
    }

    private static ImageSharpMiddlewareOptions Configure()
    {
        var options = new ImageSharpMiddlewareOptions();
        Configure(options);
        return options;
    }

    private static void Configure(ImageSharpMiddlewareOptions options)
        => new ConfigureImageSharpMiddlewareOptions(
                Configuration.Default,
                Options.Create(new ImagingSettings()))
            .Configure(options);

    private static ImageCommandContext CreateContext(ImageProcessingSlot? slot)
    {
        var httpContext = new DefaultHttpContext();
        if (slot is not null)
        {
            httpContext.Items[ImageProcessingSlot.HttpContextItemKey] = slot;
        }

        return new ImageCommandContext(
            httpContext,
            new CommandCollection(),
            new CommandParser(Array.Empty<ICommandConverter>()),
            CultureInfo.InvariantCulture);
    }
}
