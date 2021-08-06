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
        /// The command constant for the crop definition.
        /// </summary>
        public const string Crop = "crop";

        /// <summary>
        /// The command constant for the crop mode.
        /// </summary>
        public const string Mode = "cropmode";


        /// <inheritdoc/>
        public IEnumerable<string> Commands { get; } = new[]
        {
            Crop,
            Mode
        };

        /// <inheritdoc/>
        public FormattedImage Process(FormattedImage image, ILogger logger, IDictionary<string, string> commands, CommandParser parser, CultureInfo culture)
        {
            if (GetCrop(commands, parser, culture) is RectangleF crop)
            {
                Size size = image.Image.Size();

                // Convert from percentages to pixels based on crop mode
                if (GetMode(commands, parser, culture) is CropMode.Percentage)
                {
                    // Fix for whole numbers
                    float percentageLeft = crop.Left > 1 ? crop.Left / 100 : crop.Left;
                    float percentageRight = crop.Right > 1 ? crop.Right / 100 : crop.Right;
                    float percentageTop = crop.Top > 1 ? crop.Top / 100 : crop.Top;
                    float percentageBottom = crop.Bottom > 1 ? crop.Bottom / 100 : crop.Bottom;

                    // Work out the percentages
                    float left = percentageLeft * size.Width;
                    float top = percentageTop * size.Height;
                    float width = percentageRight < 1 ? (1 - percentageLeft - percentageRight) * size.Width : size.Width;
                    float height = percentageBottom < 1 ? (1 - percentageTop - percentageBottom) * size.Height : size.Height;

                    crop = new RectangleF(left, top, width, height);
                }

                // Round and validate crop rectangle
                var rectangle = Rectangle.Round(crop);
                if (rectangle.X < size.Width && rectangle.Y < size.Height)
                {
                    if (rectangle.Width > (size.Width - rectangle.X))
                    {
                        rectangle.Width = size.Width - rectangle.X;
                    }

                    if (rectangle.Height > (size.Height - rectangle.Y))
                    {
                        rectangle.Height = size.Height - rectangle.Y;
                    }

                    image.Image.Mutate(x => x.Crop(rectangle));
                }
            }

            return image;
        }

        private static RectangleF? GetCrop(IDictionary<string, string> commands, CommandParser parser, CultureInfo culture)
        {
            float[] crops = parser.ParseValue<float[]>(commands.GetValueOrDefault(Crop), culture);

            return (crops.Length != 4) ? null : new RectangleF(crops[0], crops[1], crops[2], crops[3]);
        }

        private static CropMode GetMode(IDictionary<string, string> commands, CommandParser parser, CultureInfo culture)
            => parser.ParseValue<CropMode>(commands.GetValueOrDefault(Mode), culture);
    }

    /// <summary>
    /// Enumerated cop modes to apply to cropped images.
    /// </summary>
    public enum CropMode
    {
        /// <summary>
        /// Crops the image using the standard rectangle model of x, y, width, height.
        /// </summary>
        Pixels,
        /// <summary>
        /// Crops the image using percentages model left, top, right, bottom.
        /// </summary>
        Percentage
    }
}
