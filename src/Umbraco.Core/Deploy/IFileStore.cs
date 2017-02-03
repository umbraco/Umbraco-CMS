using System.IO;

namespace Umbraco.Core.Deploy
{
    public interface IFileStore
    {
        void SaveStream(StringUdi udi, Stream stream);
    }
}
