using System.IO;

namespace Umbraco.Core.Serialization
{
    public interface IStreamedResult
    {
        Stream ResultStream { get; }
        bool Success { get; }
    }
}