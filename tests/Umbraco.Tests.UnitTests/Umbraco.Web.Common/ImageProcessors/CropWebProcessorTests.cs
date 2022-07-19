// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Commands.Converters;
using SixLabors.ImageSharp.Web.Middleware;
using Umbraco.Cms.Web.Common.ImageProcessors;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.ImageProcessors;

[TestFixture]
public class CropWebProcessorTests
{
    [Test]

    // Coordinates are percentages to crop from the left, top, right and bottom sides
    [TestCase("0,0,0,0", 50, 90)]
    [TestCase("0.1,0.0,0.0,0.0", 45, 90)]
    [TestCase("0.0,0.1,0.0,0.0", 50, 81)]
    [TestCase("0.0,0.0,0.1,0.0", 45, 90)]
    [TestCase("0.0,0.0,0.0,0.1", 50, 81)]
    [TestCase("0.1,0.0,0.1,0.0", 40, 90)]
    [TestCase("0.0,0.1,0.0,0.1", 50, 72)]
    [TestCase("0.1,0.1,0.1,0.1", 40, 72)]
    [TestCase("0.25,0.25,0.25,0.25", 25, 45)]
    public void CropWebProcessor_CropsImage(string coordinates, int width, int height)
    {
        using var image = new Image<Rgba32>(50, 90);
        using var formattedImage = new FormattedImage(image, PngFormat.Instance);

        var logger = new NullLogger<ImageSharpMiddleware>();
        var commands = new CommandCollection { { CropWebProcessor.Coordinates, coordinates } };
        var parser = new CommandParser(new ICommandConverter[]
        {
            new ArrayConverter<float>(),
            new SimpleCommandConverter<float>(),
        });
        var culture = CultureInfo.InvariantCulture;

        new CropWebProcessor().Process(formattedImage, logger, commands, parser, culture);

        Assert.AreEqual(width, image.Width);
        Assert.AreEqual(height, image.Height);
    }
}
