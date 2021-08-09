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
                if (GetMode(commands, parser, culture) == CropMode.Percentage)
                {
                    // Convert the percentage based model of left, top, right, bottom to x, y, width, height
                    int sourceWidth = image.Image.Width;
                    int sourceHeight = image.Image.Height;

                    float left = crop.Left * sourceWidth;
                    float top = crop.Top * sourceHeight;
                    float width = sourceWidth - (sourceWidth * crop.Width) - left;
                    float height = sourceHeight - (sourceHeight * crop.Height) - top;

                    crop = new RectangleF(left, top, width, height);
                }

                var cropRectangle = Rectangle.Round(crop);
                
                image.Image.Mutate(x => x.Crop(cropRectangle));
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
