using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Commands;
using SixLabors.ImageSharp.Web.Processors;
using ImageCropperCropCoordinates = Umbraco.Cms.Core.PropertyEditors.ValueConverters.ImageCropperValue.ImageCropperCropCoordinates;

namespace Umbraco.Cms.Web.Common.ImageProcessors
{
    public class CropWebProcessor : IImageWebProcessor
    {
        /// <summary>
        /// The command constant for the crop definition
        /// </summary>
        public const string Crop = "crop";

        public FormattedImage Process(
            FormattedImage image,
            ILogger logger,
            IDictionary<string, string> commands,
            CommandParser parser,
            CultureInfo culture)
        {
            ImageCropperCropCoordinates cropCoordinates = GetCropCoordinates(commands, parser, culture);
            if (cropCoordinates is null)
            {
                return image;
            }

            Size size = image.Image.Size();
            Rectangle crop = GetCropRectangle(size.Width, size.Height, cropCoordinates);
            image.Image.Mutate(x => x.Crop(crop));
            return image;
        }

        private static readonly IEnumerable<string> CropCommands = new[] {Crop};

        public IEnumerable<string> Commands { get; } = CropCommands;

        /// <summary>
        /// Gets a rectangle that is calculated from crop coordinates.
        /// </summary>
        /// <param name="width">The width of the image being cropped.</param>
        /// <param name="height">The height of the image being cropped.</param>
        /// <param name="coordinates">Coordinate set to calculate rectangle from.</param>
        /// <returns>Rectangle with the position and sized described in coordinates.</returns>
        private static Rectangle GetCropRectangle(int width, int height, ImageCropperCropCoordinates coordinates)
        {
            // Get coordinates of top left corner of the rectangle
            var topX = RoundToInt(width * coordinates.X1);
            var topY = RoundToInt(height * coordinates.Y1);
            // Get coordinated of bottom right corner
            var bottomX = RoundToInt(width - (width * coordinates.X2));
            var bottomY = RoundToInt(height - (height * coordinates.Y2));

            // Get width and height of crop
            var cropWidth = bottomX - topX;
            var cropHeight = bottomY - topY;

            return new Rectangle(topX, topY, cropWidth, cropHeight);
        }

        /// <summary>
        /// Converts a decimal to an int with rounding.
        /// </summary>
        /// <param name="number">Decimal to convert.</param>
        /// <returns>The decimal rounded to an int.</returns>
        private static int RoundToInt(decimal number) => decimal.ToInt32(Math.Round(number));

        /// <summary>
        /// Gets the crop coordinates from the query string.
        /// </summary>
        /// <param name="commands">Commands dictionary to parse the coordinates from.</param>
        /// <param name="parser">Parser provided by imagesharp.</param>
        /// <param name="culture">Culture to use for parsing.</param>
        /// <returns>Coordinates of the crop.</returns>
        private static ImageCropperCropCoordinates GetCropCoordinates(IDictionary<string, string> commands, CommandParser parser, CultureInfo culture)
        {
            decimal[] crops = parser.ParseValue<decimal[]>(commands.GetValueOrDefault(Crop), culture);

            if (crops.Length != 4)
            {
                return null;
            }

            return new ImageCropperCropCoordinates()
            {
                X1 = crops[0],
                Y1 = crops[1],
                X2 = crops[2],
                Y2 = crops[3]
            };
        }
    }
}
