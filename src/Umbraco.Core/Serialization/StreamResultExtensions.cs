using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Umbraco.Core.Serialization
{
    public static class StreamResultExtensions
    {
        public static string ToJsonString(this Stream stream)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(bytes, 0, (int)stream.Length);
            return Encoding.UTF8.GetString(bytes);
        }

        public static XDocument ToXDoc(this Stream stream)
        {
            return XDocument.Load(stream);
        }
    }
}