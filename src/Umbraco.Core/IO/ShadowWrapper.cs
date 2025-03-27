using Microsoft.Extensions.FileProviders;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.IO;

internal class ShadowWrapper : IFileSystem, IFileProviderFactory
{
    private const string ShadowFsPath = "ShadowFs";

    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IFileSystemFactory _fileSystemFactory;
    private readonly string _shadowPath;
    private readonly Func<bool?>? _isScoped;

    private string? _shadowDir;
    private ShadowFileSystem? _shadowFileSystem;

    public ShadowWrapper(IFileSystem innerFileSystem, IHostingEnvironment hostingEnvironment, IFileSystemFactory fileSystemFactory, string shadowPath, Func<bool?>? isScoped = null)
    {
        InnerFileSystem = innerFileSystem;

        _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
        _fileSystemFactory = fileSystemFactory;
        _shadowPath = shadowPath;
        _isScoped = isScoped;
    }

    public IFileSystem InnerFileSystem { get; }

    public bool CanAddPhysical => FileSystem.CanAddPhysical;

    private IFileSystem FileSystem
    {
        get
        {
            Func<bool?>? isScoped = _isScoped;
            if (isScoped is not null && _shadowFileSystem is not null)
            {
                bool? scoped = isScoped();

                // if the filesystem is created *after* shadowing starts, it won't be shadowing
                // better not ignore that situation and raise a meaningful (?) exception
                if (scoped.HasValue && scoped.Value && _shadowFileSystem == null)
                {
                    throw new Exception("The filesystems are shadowing, but this filesystem is not.");
                }

                return scoped.HasValue && scoped.Value
                    ? _shadowFileSystem
                    : InnerFileSystem;
            }

            return InnerFileSystem;
        }
    }

    /// <inheritdoc />
    public IFileProvider? Create() => InnerFileSystem.TryCreateFileProvider(out IFileProvider? fileProvider) ? fileProvider : null;

    public IEnumerable<string> GetDirectories(string path) => FileSystem.GetDirectories(path);

    public void DeleteDirectory(string path) => FileSystem.DeleteDirectory(path);

    public void DeleteDirectory(string path, bool recursive) => FileSystem.DeleteDirectory(path, recursive);

    public bool DirectoryExists(string path) => FileSystem.DirectoryExists(path);

    public void AddFile(string path, Stream stream) => FileSystem.AddFile(path, stream);

    public void AddFile(string path, Stream stream, bool overrideExisting) => FileSystem.AddFile(path, stream, overrideExisting);

    public IEnumerable<string> GetFiles(string path) => FileSystem.GetFiles(path);

    public IEnumerable<string> GetFiles(string path, string filter) => FileSystem.GetFiles(path, filter);

    public Stream OpenFile(string path) => FileSystem.OpenFile(path);

    public void DeleteFile(string path) => FileSystem.DeleteFile(path);

    public bool FileExists(string path) => FileSystem.FileExists(path);

    public string GetRelativePath(string fullPathOrUrl) => FileSystem.GetRelativePath(fullPathOrUrl);

    public string GetFullPath(string path) => FileSystem.GetFullPath(path);

    public string GetUrl(string? path) => FileSystem.GetUrl(path);

    public DateTimeOffset GetLastModified(string path) => FileSystem.GetLastModified(path);

    public DateTimeOffset GetCreated(string path) => FileSystem.GetCreated(path);

    public long GetSize(string path) => FileSystem.GetSize(path);

    public static string CreateShadowId(IHostingEnvironment hostingEnvironment)
    {
        const int retries = 50; // avoid infinite loop
        const int idLength = 8; // 6 chars

        // shorten a Guid to idLength chars, and see whether it collides
        // with an existing directory or not - if it does, try again, and
        // we should end up with a unique identifier eventually - but just
        // detect infinite loops (just in case)
        for (var i = 0; i < retries; i++)
        {
            var id = GuidUtils.ToBase32String(Guid.NewGuid(), idLength);

            var shadowDir = Path.Combine(hostingEnvironment.LocalTempPath, ShadowFsPath, id);
            if (Directory.Exists(shadowDir))
            {
                continue;
            }

            Directory.CreateDirectory(shadowDir);
            return id;
        }

        throw new Exception($"Could not get a shadow identifier (tried {retries} times)");
    }

    public void AddFile(string path, string physicalPath, bool overrideIfExists = true, bool copy = false) =>
        FileSystem.AddFile(path, physicalPath, overrideIfExists, copy);

    internal void Shadow(string id)
    {
        // note: no thread-safety here, because ShadowFs is thread-safe due to the check
        // on ShadowFileSystemsScope.None - and if None is false then we should be running
        // in a single thread anyways
        var rootUrl = Path.Combine(ShadowFsPath, id, _shadowPath);
        _shadowDir = Path.Combine(_hostingEnvironment.LocalTempPath, rootUrl);
        Directory.CreateDirectory(_shadowDir);
        var tempfs = _fileSystemFactory.CreateFileSystem(_shadowDir, rootUrl);
        _shadowFileSystem = new ShadowFileSystem(InnerFileSystem, tempfs);
    }

    internal void UnShadow(bool complete)
    {
        ShadowFileSystem? shadowFileSystem = _shadowFileSystem;
        var dir = _shadowDir;
        _shadowFileSystem = null;
        _shadowDir = null;

        try
        {
            // this may throw an AggregateException if some of the changes could not be applied
            if (complete)
            {
                shadowFileSystem?.Complete();
            }
        }
        finally
        {
            // in any case, cleanup
            try
            {
                Directory.Delete(dir!, true);

                // shadowPath make be path/to/dir, remove each
                dir = dir!.Replace('/', Path.DirectorySeparatorChar);
                var min = Path.Combine(_hostingEnvironment.LocalTempPath, ShadowFsPath).Length;
                var pos = dir.LastIndexOf(Path.DirectorySeparatorChar);
                while (pos > min)
                {
                    dir = dir.Substring(0, pos);
                    if (Directory.EnumerateFileSystemEntries(dir).Any() == false)
                    {
                        Directory.Delete(dir, true);
                    }
                    else
                    {
                        break;
                    }

                    pos = dir.LastIndexOf(Path.DirectorySeparatorChar);
                }
            }
            catch
            {
                // ugly, isn't it? but if we cannot cleanup, bah, just leave it there
            }
        }
    }
}
