using System.Globalization;
using System.Xml.Linq;

namespace Umbraco.Cms.Core.Media.Exif;

internal class SvgFile : ImageFile
{
    public SvgFile(Stream fileStream)
    {
        fileStream.Position = 0;

        var document =
            XDocument.Load(fileStream); // if it throws an exception the ugly try catch in MediaFileSystem will catch it

        var width = document.Root?.Attributes().Where(x => x.Name == "width").Select(x => x.Value).FirstOrDefault();
        var height = document.Root?.Attributes().Where(x => x.Name == "height").Select(x => x.Value).FirstOrDefault();

        Properties.Add(new ExifSInt(
            ExifTag.PixelYDimension,
            height == null ? Constants.Conventions.Media.DefaultSize : int.Parse(height, CultureInfo.InvariantCulture)));
        Properties.Add(new ExifSInt(
            ExifTag.PixelXDimension,
            width == null ? Constants.Conventions.Media.DefaultSize : int.Parse(width, CultureInfo.InvariantCulture)));

        Format = ImageFileFormat.SVG;
    }

    public override void Save(Stream stream)
    {
    }
}
