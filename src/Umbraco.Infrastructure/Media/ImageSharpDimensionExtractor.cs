using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using Umbraco.Cms.Core.Media;
using Size = System.Drawing.Size;

namespace Umbraco.Cms.Infrastructure.Media;

internal class ImageSharpDimensionExtractor : IImageDimensionExtractor
{
    private readonly Configuration _configuration;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ImageSharpDimensionExtractor" /> class.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    public ImageSharpDimensionExtractor(Configuration configuration)
        => _configuration = configuration;

    /// <summary>
    ///     Gets the dimensions of an image.
    /// </summary>
    /// <param name="stream">A stream containing the image bytes.</param>
    /// <returns>
    ///     The dimension of the image.
    /// </returns>
    public Size? GetDimensions(Stream? stream)
    {
        Size? size = null;

        IImageInfo imageInfo = Image.Identify(_configuration, stream);
        if (imageInfo != null)
        {
            size = IsExifOrientationRotated(imageInfo)
                ? new Size(imageInfo.Height, imageInfo.Width)
                : new Size(imageInfo.Width, imageInfo.Height);
        }

        return size;
    }

    private static bool IsExifOrientationRotated(IImageInfo imageInfo)
        => GetExifOrientation(imageInfo) switch
        {
            ExifOrientationMode.LeftTop
                or ExifOrientationMode.RightTop
                or ExifOrientationMode.RightBottom
                or ExifOrientationMode.LeftBottom => true,
            _ => false,
        };

    private static ushort GetExifOrientation(IImageInfo imageInfo)
    {
        IExifValue<ushort>? orientation = imageInfo.Metadata.ExifProfile?.GetValue(ExifTag.Orientation);
        if (orientation is not null)
        {
            if (orientation.DataType == ExifDataType.Short)
            {
                return orientation.Value;
            }

            return Convert.ToUInt16(orientation.Value);
        }

        return ExifOrientationMode.Unknown;
    }
}
