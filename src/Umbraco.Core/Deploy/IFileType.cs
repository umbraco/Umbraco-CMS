using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Umbraco.Cms.Core.Deploy
{
    public interface IFileType
    {
        Stream GetStream(StringUdi udi);

        Task<Stream> GetStreamAsync(StringUdi udi, CancellationToken token);

        Stream GetChecksumStream(StringUdi udi);

        long GetLength(StringUdi udi);

        void SetStream(StringUdi udi, Stream stream);

        Task SetStreamAsync(StringUdi udi, Stream stream, CancellationToken token);

        bool CanSetPhysical { get; }

        void Set(StringUdi udi, string physicalPath, bool copy = false);

        // this is not pretty as *everywhere* in Deploy we take care of ignoring
        // the physical path and always rely on Core's virtual IFileSystem but
        // Cloud wants to add some of these files to Git and needs the path...
        string GetPhysicalPath(StringUdi udi);

        string GetVirtualPath(StringUdi udi);
    }
}
