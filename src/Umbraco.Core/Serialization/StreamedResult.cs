using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Umbraco.Core.Serialization
{
    public class StreamedResult : IStreamedResult
    {
        internal StreamedResult(Stream stream, bool success)
        {
            ResultStream = stream;
            Success = success;
        }

        #region Implementation of IStreamedResult

        public Stream ResultStream { get; protected set; }

        public bool Success { get; protected set; }

        #endregion
    }

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