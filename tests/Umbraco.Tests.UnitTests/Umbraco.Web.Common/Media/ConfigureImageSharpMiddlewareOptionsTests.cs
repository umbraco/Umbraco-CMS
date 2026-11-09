// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Commands.Converters;
using SixLabors.ImageSharp.Web.Middleware;
using SixLabors.ImageSharp.Web.Processors;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Imaging.ImageSharp;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Media;

[TestFixture]
public class ConfigureImageSharpMiddlewareOptionsTests
{
    private const int MaxWidth = ImagingResizeSettings.StaticMaxWidth;
    private const int MaxHeight = ImagingResizeSettings.StaticMaxHeight;

    private static readonly byte[] _hmacKey =
    {
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
        16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31,
    };

    [Test]
    public async Task Cannot_Request_Width_Above_Maximum_When_Hmac_Configured()
    {
        ImageCommandContext context = BuildContext((ResizeWebProcessor.Width, (MaxWidth + 1000).ToString(CultureInfo.InvariantCulture)));

        await ConfigureAndParse(withHmac: true, context);

        Assert.IsFalse(context.Commands.Contains(ResizeWebProcessor.Width));
    }

    [Test]
    public async Task Cannot_Request_Height_Above_Maximum_When_Hmac_Configured()
    {
        ImageCommandContext context = BuildContext((ResizeWebProcessor.Height, (MaxHeight + 1000).ToString(CultureInfo.InvariantCulture)));

        await ConfigureAndParse(withHmac: true, context);

        Assert.IsFalse(context.Commands.Contains(ResizeWebProcessor.Height));
    }

    [Test]
    public async Task Cannot_Request_Negative_Width_When_Hmac_Configured()
    {
        ImageCommandContext context = BuildContext((ResizeWebProcessor.Width, "-100"));

        await ConfigureAndParse(withHmac: true, context);

        Assert.IsFalse(context.Commands.Contains(ResizeWebProcessor.Width));
    }

    [Test]
    public async Task Cannot_Request_Negative_Height_When_Hmac_Configured()
    {
        ImageCommandContext context = BuildContext((ResizeWebProcessor.Height, "-100"));

        await ConfigureAndParse(withHmac: true, context);

        Assert.IsFalse(context.Commands.Contains(ResizeWebProcessor.Height));
    }

    [Test]
    public async Task Can_Request_Dimensions_Within_Maximum_When_Hmac_Configured()
    {
        ImageCommandContext context = BuildContext(
            (ResizeWebProcessor.Width, "800"),
            (ResizeWebProcessor.Height, "600"));

        await ConfigureAndParse(withHmac: true, context);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(context.Commands.Contains(ResizeWebProcessor.Width));
            Assert.AreEqual("800", context.Commands[ResizeWebProcessor.Width]);
            Assert.IsTrue(context.Commands.Contains(ResizeWebProcessor.Height));
            Assert.AreEqual("600", context.Commands[ResizeWebProcessor.Height]);
        });
    }

    [Test]
    public async Task Cannot_Request_Width_Above_Maximum_When_Hmac_Not_Configured()
    {
        ImageCommandContext context = BuildContext((ResizeWebProcessor.Width, (MaxWidth + 1000).ToString(CultureInfo.InvariantCulture)));

        await ConfigureAndParse(withHmac: false, context);

        Assert.IsFalse(context.Commands.Contains(ResizeWebProcessor.Width));
    }

    private static async Task ConfigureAndParse(bool withHmac, ImageCommandContext context)
    {
        var settings = new ImagingSettings
        {
            HMACSecretKey = withHmac ? _hmacKey : Array.Empty<byte>(),
        };

        var sut = new ConfigureImageSharpMiddlewareOptions(
            SixLabors.ImageSharp.Configuration.Default.Clone(),
            Options.Create(settings));

        var options = new ImageSharpMiddlewareOptions();
        sut.Configure(options);

        await options.OnParseCommandsAsync(context);
    }

    private static ImageCommandContext BuildContext(params (string Key, string Value)[] commands)
    {
        var collection = new CommandCollection();
        foreach ((var key, var value) in commands)
        {
            collection.Add(key, value);
        }

        return new ImageCommandContext(
            new DefaultHttpContext(),
            collection,
            new CommandParser(Enumerable.Empty<ICommandConverter>()),
            CultureInfo.InvariantCulture);
    }
}
