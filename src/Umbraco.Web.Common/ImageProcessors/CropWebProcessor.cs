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
                CropMode mode = GetMode(commands, parser, culture);

                if (mode == CropMode.Percentage)
                {
                    // Convert the percentage based model of left, top, right, bottom to x, y, width, height
                    float x = crop.Left * size.Width;
                    float y = crop.Top * size.Height;
                    float width = crop.Right < 1 ? (1 - crop.Left - crop.Right) * size.Width : size.Width;
                    float height = crop.Bottom < 1 ? (1 - crop.Top - crop.Bottom) * size.Height : size.Height;

                    crop = new RectangleF(x, y, width, height);
                }

                // Round and validate/clamp crop rectangle
                var cropRectangle = Rectangle.Round(crop);
                if (cropRectangle.X < size.Width && cropRectangle.Y < size.Height)
                {
                    if (cropRectangle.Width > (size.Width - cropRectangle.X))
                    {
                        cropRectangle.Width = size.Width - cropRectangle.X;
                    }

                    if (cropRectangle.Height > (size.Height - cropRectangle.Y))
                    {
                        cropRectangle.Height = size.Height - cropRectangle.Y;
                    }

                    image.Image.Mutate(x => x.Crop(cropRectangle));
                }
            }

            return image;
        }

        private static RectangleF? GetCrop(IDictionary<string, string> commands, CommandParser parser, CultureInfo culture)
        {
            float[] coordinates = parser.ParseValue<float[]>(commands.GetValueOrDefault(Crop), culture);

            if (coordinates.Length != 4)
            {
                return null;
            }

            return new RectangleF(coordinates[0], coordinates[1], coordinates[2], coordinates[3]);
        }

        private static CropMode GetMode(IDictionary<string, string> commands, CommandParser parser, CultureInfo culture)
            => parser.ParseValue<CropMode>(commands.GetValueOrDefault(Mode), culture);
    }
}
