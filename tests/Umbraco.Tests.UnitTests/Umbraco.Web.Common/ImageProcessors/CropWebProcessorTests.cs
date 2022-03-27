// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using NUnit.Framework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Commands.Converters;
using Umbraco.Cms.Web.Common.ImageProcessors;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.ImageProcessors
{
    [TestFixture]
    public class CropWebProcessorTests
    {
        [Test]
        public void CropWebProcessor_CropsImage()
        {
            var converters = new List<ICommandConverter>
            {
                CreateArrayConverterOfFloat(),
                CreateSimpleCommandConverterOfFloat(),
            };

            var parser = new CommandParser(converters);
            CultureInfo culture = CultureInfo.InvariantCulture;

            var commands = new CommandCollection
            {
                { CropWebProcessor.Coordinates, "0.1,0.2,0.1,0.4" },  // left, top, right, bottom
            };

            using var image = new Image<Rgba32>(50, 80);
            using FormattedImage formatted = CreateFormattedImage(image, PngFormat.Instance);
            new CropWebProcessor().Process(formatted, null, commands, parser, culture);

            Assert.AreEqual(40, image.Width);   // Cropped 5 pixels from each side.
            Assert.AreEqual(32, image.Height);  // Cropped 16 pixels from the top and 32 from the bottom.
        }

        private static ICommandConverter CreateArrayConverterOfFloat()
        {
            // ImageSharp.Web's ArrayConverter is internal, so we need to use reflection to instantiate.
            var type = Type.GetType("SixLabors.ImageSharp.Web.Commands.Converters.ArrayConverter`1, SixLabors.ImageSharp.Web");
            Type[] typeArgs = { typeof(float) };
            Type genericType = type.MakeGenericType(typeArgs);
            return (ICommandConverter)Activator.CreateInstance(genericType);
        }

        private static ICommandConverter CreateSimpleCommandConverterOfFloat()
        {
            // ImageSharp.Web's SimpleCommandConverter is internal, so we need to use reflection to instantiate.
            var type = Type.GetType("SixLabors.ImageSharp.Web.Commands.Converters.SimpleCommandConverter`1, SixLabors.ImageSharp.Web");
            Type[] typeArgs = { typeof(float) };
            Type genericType = type.MakeGenericType(typeArgs);
            return (ICommandConverter)Activator.CreateInstance(genericType);
        }

        private FormattedImage CreateFormattedImage(Image<Rgba32> image, PngFormat format)
        {
            // Again, the constructor of FormattedImage useful for tests is internal, so we need to use reflection.
            Type type = typeof(FormattedImage);
            var instance = type.Assembly.CreateInstance(
                type.FullName,
                false,
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new object[] { image, format },
                null,
                null);
            return (FormattedImage)instance;
        }
    }
}
