using System.IO;

namespace Umbraco.Core.Serialization
{
    public class StreamedResult : IStreamedResult
    {
        public StreamedResult(Stream stream, bool success)
        {
            ResultStream = stream;
            Success = success;
        }

        #region Implementation of IStreamedResult

        public Stream ResultStream { get; protected set; }

        public bool Success { get; protected set; }

        #endregion
    }
}
