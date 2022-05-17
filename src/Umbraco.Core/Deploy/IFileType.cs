namespace Umbraco.Cms.Core.Deploy;

public interface IFileType
{
    bool CanSetPhysical { get; }

    Stream GetStream(StringUdi udi);

    Task<Stream> GetStreamAsync(StringUdi udi, CancellationToken token);

    Stream GetChecksumStream(StringUdi udi);

    long GetLength(StringUdi udi);

    void SetStream(StringUdi udi, Stream stream);

    Task SetStreamAsync(StringUdi udi, Stream stream, CancellationToken token);

    void Set(StringUdi udi, string physicalPath, bool copy = false);

    // this is not pretty as *everywhere* in Deploy we take care of ignoring
    // the physical path and always rely on Core's virtual IFileSystem but
    // Cloud wants to add some of these files to Git and needs the path...
    string GetPhysicalPath(StringUdi udi);

    string GetVirtualPath(StringUdi udi);
}
