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
        public FormattedImage Process(FormattedImage image, ILogger logger, IDictionary<string, string> commands, CommandParser parser, CultureInfo culture)
        {
            RectangleF? coordinates = GetCoordinates(commands, parser, culture);
            if (coordinates != null)
            {
                // Convert the percentage based model of left, top, right, bottom to x, y, width, height
                int sourceWidth = image.Image.Width;
                int sourceHeight = image.Image.Height;
                int x = (int)MathF.Round(coordinates.Value.Left * sourceWidth);
                int y = (int)MathF.Round(coordinates.Value.Top * sourceHeight);
                int width = sourceWidth - (int)MathF.Round(coordinates.Value.Right * sourceWidth);
                int height = sourceHeight - (int)MathF.Round(coordinates.Value.Bottom * sourceHeight);

                var cropRectangle = new Rectangle(x, y, width, height);
                
                image.Image.Mutate(x => x.Crop(cropRectangle));
            }

            return image;
        }

        private static RectangleF? GetCoordinates(IDictionary<string, string> commands, CommandParser parser, CultureInfo culture)
        {
            float[] coordinates = parser.ParseValue<float[]>(commands.GetValueOrDefault(Coordinates), culture);

            if (coordinates.Length != 4)
            {
                return null;
            }

            return new RectangleF(coordinates[0], coordinates[1], coordinates[2], coordinates[3]);
        }
    }
}
