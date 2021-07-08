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
            ImageCropperCropCoordinates cropCoordinates = GetCropCoordinates(commands);
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

        private static Rectangle GetCropRectangle(int width, int height, ImageCropperCropCoordinates coordinates)
        {

            // Get coordinates of top left corner of the rectangle
            var topX = decimal.ToInt32(width * coordinates.X1);
            var topY = decimal.ToInt32(height * coordinates.Y1);
            // Get coordinated of bottom right corner
            var bottomX = decimal.ToInt32(width - (width * coordinates.X2));
            var bottomY = decimal.ToInt32(height - (height * coordinates.Y2));

            // Get width and height of crop
            var cropWidth = bottomX - topX;
            var cropHeight = bottomY - topY;

            return new Rectangle(topX, topY, cropWidth, cropHeight);
        }

        private static ImageCropperCropCoordinates GetCropCoordinates(IDictionary<string, string> commands)
        {
            if (!commands.TryGetValue(Crop, out var crop))
            {
                return null;
            }

            var crops = crop.Split(',');

            return new ImageCropperCropCoordinates()
            {
                X1 = decimal.Parse(crops[0], CultureInfo.InvariantCulture),
                Y1 = decimal.Parse(crops[1], CultureInfo.InvariantCulture),
                X2 = decimal.Parse(crops[2], CultureInfo.InvariantCulture),
                Y2 = decimal.Parse(crops[3], CultureInfo.InvariantCulture)
            };
        }
    }
}
