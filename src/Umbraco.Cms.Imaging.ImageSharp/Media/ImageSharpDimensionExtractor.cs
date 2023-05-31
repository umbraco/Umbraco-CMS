using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using Umbraco.Cms.Core.Media;
using Size = System.Drawing.Size;

namespace Umbraco.Cms.Imaging.ImageSharp.Media;

public sealed class ImageSharpDimensionExtractor : IImageDimensionExtractor
{
    private readonly Configuration _configuration;

    /// <inheritdoc />
    public IEnumerable<string> SupportedImageFileTypes { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageSharpDimensionExtractor" /> class.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    public ImageSharpDimensionExtractor(Configuration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        SupportedImageFileTypes = configuration.ImageFormats.SelectMany(f => f.FileExtensions).ToArray();
    }

    /// <inheritdoc />
    public Size? GetDimensions(Stream? stream)
    {
        Size? size = null;

        if (stream is not null)
        {
            DecoderOptions options = new()
            {
                Configuration = _configuration,
            };

            ImageInfo imageInfo = Image.Identify(options, stream);
            if (imageInfo != null)
            {
                size = IsExifOrientationRotated(imageInfo)
                    ? new Size(imageInfo.Height, imageInfo.Width)
                    : new Size(imageInfo.Width, imageInfo.Height);
            }
        }

        return size;
    }

    private static bool IsExifOrientationRotated(ImageInfo imageInfo)
        => GetExifOrientation(imageInfo) switch
        {
            ExifOrientationMode.LeftTop or ExifOrientationMode.RightTop or ExifOrientationMode.RightBottom or ExifOrientationMode.LeftBottom => true,
            _ => false,
        };

    private static ushort GetExifOrientation(ImageInfo imageInfo)
    {
        if (imageInfo.Metadata.ExifProfile?.TryGetValue(ExifTag.Orientation, out IExifValue<ushort>? orientation) == true)
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
