using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.IO;

/// <summary>
/// Wraps a file system to support shadow mode for transactional file operations.
/// </summary>
/// <remarks>
/// When in shadow mode, all file operations are captured and can be applied or discarded
/// when the shadow scope ends.
/// </remarks>
internal sealed class ShadowWrapper : IFileSystem, IFileProviderFactory
{
    private const string ShadowFsPath = "ShadowFs";

    private readonly IIOHelper _ioHelper;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly ILoggerFactory _loggerFactory;
    private readonly string _shadowPath;
    private readonly Func<bool?>? _isScoped;

    private string? _shadowDir;
    private ShadowFileSystem? _shadowFileSystem;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShadowWrapper"/> class.
    /// </summary>
    /// <param name="innerFileSystem">The inner file system to wrap.</param>
    /// <param name="ioHelper">The IO helper.</param>
    /// <param name="hostingEnvironment">The hosting environment.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="shadowPath">The shadow path for temporary files.</param>
    /// <param name="isScoped">A function that returns whether the file system is currently scoped.</param>
    public ShadowWrapper(IFileSystem innerFileSystem, IIOHelper ioHelper, IHostingEnvironment hostingEnvironment, ILoggerFactory loggerFactory, string shadowPath, Func<bool?>? isScoped = null)
    {
        InnerFileSystem = innerFileSystem;

        _ioHelper = ioHelper ?? throw new ArgumentNullException(nameof(ioHelper));
        _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
        _loggerFactory = loggerFactory;
        _shadowPath = shadowPath;
        _isScoped = isScoped;
    }

    /// <summary>
    /// Gets the inner file system being wrapped.
    /// </summary>
    public IFileSystem InnerFileSystem { get; }

    /// <inheritdoc />
    public bool CanAddPhysical => FileSystem.CanAddPhysical;

    /// <summary>
    /// Gets the current file system, which may be the shadow or inner file system depending on scope.
    /// </summary>
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

    /// <inheritdoc />
    public IEnumerable<string> GetDirectories(string path) => FileSystem.GetDirectories(path);

    /// <inheritdoc />
    public void DeleteDirectory(string path) => FileSystem.DeleteDirectory(path);

    /// <inheritdoc />
    public void DeleteDirectory(string path, bool recursive) => FileSystem.DeleteDirectory(path, recursive);

    /// <inheritdoc />
    public bool DirectoryExists(string path) => FileSystem.DirectoryExists(path);

    /// <inheritdoc />
    public void AddFile(string path, Stream stream) => FileSystem.AddFile(path, stream);

    /// <inheritdoc />
    public void AddFile(string path, Stream stream, bool overrideExisting) => FileSystem.AddFile(path, stream, overrideExisting);

    /// <inheritdoc />
    public IEnumerable<string> GetFiles(string path) => FileSystem.GetFiles(path);

    /// <inheritdoc />
    public IEnumerable<string> GetFiles(string path, string filter) => FileSystem.GetFiles(path, filter);

    /// <inheritdoc />
    public Stream OpenFile(string path) => FileSystem.OpenFile(path);

    /// <inheritdoc />
    public void DeleteFile(string path) => FileSystem.DeleteFile(path);

    /// <summary>
    /// Moves a file from the source path to the target path.
    /// </summary>
    /// <param name="source">The source file path.</param>
    /// <param name="target">The target file path.</param>
    public void MoveFile(string source, string target) => FileSystem.MoveFile(source, target);

    /// <inheritdoc />
    public bool FileExists(string path) => FileSystem.FileExists(path);

    /// <inheritdoc />
    public string GetRelativePath(string fullPathOrUrl) => FileSystem.GetRelativePath(fullPathOrUrl);

    /// <inheritdoc />
    public string GetFullPath(string path) => FileSystem.GetFullPath(path);

    /// <inheritdoc />
    public string GetUrl(string? path) => FileSystem.GetUrl(path);

    /// <inheritdoc />
    public DateTimeOffset GetLastModified(string path) => FileSystem.GetLastModified(path);

    /// <inheritdoc />
    public DateTimeOffset GetCreated(string path) => FileSystem.GetCreated(path);

    /// <inheritdoc />
    public long GetSize(string path) => FileSystem.GetSize(path);

    /// <summary>
    /// Creates a unique shadow identifier for a shadow scope.
    /// </summary>
    /// <param name="hostingEnvironment">The hosting environment.</param>
    /// <returns>A unique identifier for the shadow scope.</returns>
    /// <exception cref="Exception">Thrown when unable to create a unique identifier after multiple retries.</exception>
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

    /// <inheritdoc />
    public void AddFile(string path, string physicalPath, bool overrideIfExists = true, bool copy = false) =>
        FileSystem.AddFile(path, physicalPath, overrideIfExists, copy);

    /// <summary>
    /// Enters shadow mode with the specified identifier.
    /// </summary>
    /// <param name="id">The shadow identifier.</param>
    internal void Shadow(string id)
    {
        // note: no thread-safety here, because ShadowFs is thread-safe due to the check
        // on ShadowFileSystemsScope.None - and if None is false then we should be running
        // in a single thread anyways
        var rootUrl = Path.Combine(ShadowFsPath, id, _shadowPath);
        _shadowDir = Path.Combine(_hostingEnvironment.LocalTempPath, rootUrl);
        Directory.CreateDirectory(_shadowDir);
        var tempfs = new PhysicalFileSystem(_ioHelper, _hostingEnvironment, _loggerFactory.CreateLogger<PhysicalFileSystem>(), _shadowDir, rootUrl);
        _shadowFileSystem = new ShadowFileSystem(InnerFileSystem, tempfs);
    }

    /// <summary>
    /// Exits shadow mode, optionally applying the changes.
    /// </summary>
    /// <param name="complete">If <c>true</c>, apply the shadow changes to the inner file system; otherwise, discard them.</param>
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
