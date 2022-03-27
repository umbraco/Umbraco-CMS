using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Processors;

namespace Umbraco.Cms.Web.Common.ImageProcessors
{
    /// <summary>
    /// Allows the cropping of images.
    /// </summary>
    public class CropWebProcessor : IImageWebProcessor
    {
        /// <summary>
        /// The command constant for the crop coordinates.
        /// </summary>
        public const string Coordinates = "cc";

        /// <inheritdoc/>
        public IEnumerable<string> Commands { get; } = new[]
        {
            Coordinates
        };

        /// <inheritdoc/>
        public FormattedImage Process(FormattedImage image, ILogger logger, CommandCollection commands, CommandParser parser, CultureInfo culture)
        {
            RectangleF? coordinates = GetCoordinates(commands, parser, culture);
            if (coordinates != null)
            {
                // Convert the coordinates to a pixel based rectangle
                int sourceWidth = image.Image.Width;
                int sourceHeight = image.Image.Height;
                int x = (int)MathF.Round(coordinates.Value.X * sourceWidth);
                int y = (int)MathF.Round(coordinates.Value.Y * sourceHeight);
                int width = (int)MathF.Round(coordinates.Value.Width * sourceWidth);
                int height = (int)MathF.Round(coordinates.Value.Height * sourceHeight);

                var cropRectangle = new Rectangle(x, y, width, height);

                image.Image.Mutate(x => x.Crop(cropRectangle));
            }

            return image;
        }

        /// <inheritdoc />
        public bool RequiresTrueColorPixelFormat(CommandCollection commands, CommandParser parser, CultureInfo culture) => false;

        private static RectangleF? GetCoordinates(CommandCollection commands, CommandParser parser, CultureInfo culture)
        {
            float[] coordinates = parser.ParseValue<float[]>(commands.GetValueOrDefault(Coordinates), culture);

            if (coordinates.Length != 4)
            {
                return null;
            }

            // The right and bottom values are actually the distance from those sides, so convert them into real coordinates
            return RectangleF.FromLTRB(coordinates[0], coordinates[1], 1 - coordinates[2], 1 - coordinates[3]);
        }
    }
}
