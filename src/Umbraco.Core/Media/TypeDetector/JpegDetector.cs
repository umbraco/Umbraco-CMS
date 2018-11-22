using System.IO;

namespace Umbraco.Core.Media.TypeDetector
{
    public class JPEGDetector : RasterizedTypeDetector
    {
        public static bool IsOfType(Stream fileStream)
        {
            var header = GetFileHeader(fileStream);

            return header[0] == 0xff && header[1] == 0xD8;
        }
    }
}
