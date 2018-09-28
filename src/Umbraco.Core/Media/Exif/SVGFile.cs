using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Media.Exif
{
    internal class SVGFile : ImageFile
    {
        public SVGFile(Stream fileStream)
        {
            fileStream.Position = 0;

            var document = XDocument.Load(fileStream); //if it will throw an exception ugly try catch in MediaFileSystem will catch it

            var width = document.Root?.Attributes().Where(x => x.Name == "width").Select(x => x.Value).FirstOrDefault() ?? UmbracoConfig.For.UmbracoSettings().Content.SvgDefaultSize;
            var height = document.Root?.Attributes().Where(x => x.Name == "height").Select(x => x.Value).FirstOrDefault() ?? UmbracoConfig.For.UmbracoSettings().Content.SvgDefaultSize;

            Properties.Add(new ExifSInt(ExifTag.PixelYDimension, int.Parse(height)));
            Properties.Add(new ExifSInt(ExifTag.PixelXDimension, int.Parse(width)));

            Format = ImageFileFormat.SVG;
        }


        public override void Save(Stream stream)
        {
        }


        public override Image ToImage()
        {
            throw new NotImplementedException();
        }
    }
}
