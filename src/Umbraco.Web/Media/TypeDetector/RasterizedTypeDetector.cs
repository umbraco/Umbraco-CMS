using System.IO;

namespace Umbraco.Web.Media.TypeDetector
{
    public abstract class RasterizedTypeDetector
    {
        public static byte[] GetFileHeader(Stream fileStream)
        {
            fileStream.Seek(0, SeekOrigin.Begin);
            byte[] header = new byte[8];
            fileStream.Seek(0, SeekOrigin.Begin);

            return header;
        }
    }
}
