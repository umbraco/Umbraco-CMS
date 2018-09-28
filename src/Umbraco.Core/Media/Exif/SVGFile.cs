using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Umbraco.Core.Media.Exif
{
    internal class SVGFile : ImageFile
    {
        public SVGFile(Stream fileStream)
        {
            fileStream.Position = 0;

            var document = new XDocument();

            try
            {
                document = XDocument.Load(fileStream);
            }
            catch (Exception ex)
            {
                return;
            }

            var width = document.Root.Attributes().Where(x => x.Name == "width").Select(x => x.Value).FirstOrDefault();
            var height = document.Root.Attributes().Where(x => x.Name == "height").Select(x => x.Value).FirstOrDefault();

            Properties[ExifTag.PixelYDimension].Value = height;
            Properties[ExifTag.PixelXDimension].Value = width;
        }
        public override void Save(Stream stream)
        {
            throw new NotImplementedException();
        }

        public override Image ToImage()
        {
            throw new NotImplementedException();
        }
    }
}
