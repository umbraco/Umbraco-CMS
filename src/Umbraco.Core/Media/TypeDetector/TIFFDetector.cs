using System.IO;
using System.Text;

namespace Umbraco.Core.Media.TypeDetector
{
    public class TIFFDetector
    {
        public static bool IsOfType(Stream fileStream)
        {
            string tiffHeader = GetFileHeader(fileStream);

            return tiffHeader == "MM\x00\x2a" || tiffHeader == "II\x2a\x00";
        }

        public static string GetFileHeader(Stream fileStream)
        {
            var header = RasterizedTypeDetector.GetFileHeader(fileStream);

            string tiffHeader = Encoding.ASCII.GetString(header, 0, 4);

            return tiffHeader;
        }
    }
}
