using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.IO
{
    internal class ShadowWrapper : IFileSystem, IFileProviderFactory
    {
        private static readonly string ShadowFsPath = Constants.SystemDirectories.TempData.EnsureEndsWith('/') + "ShadowFs";

        private readonly Func<bool?>? _isScoped;
        private readonly IFileSystem _innerFileSystem;
        private readonly string _shadowPath;
        private ShadowFileSystem? _shadowFileSystem;
        private string? _shadowDir;
        private readonly IIOHelper _ioHelper;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILoggerFactory _loggerFactory;

        public ShadowWrapper(IFileSystem innerFileSystem, IIOHelper ioHelper, IHostingEnvironment hostingEnvironment, ILoggerFactory loggerFactory, string shadowPath, Func<bool?>? isScoped = null)
        {
            _innerFileSystem = innerFileSystem;
            _ioHelper = ioHelper ?? throw new ArgumentNullException(nameof(ioHelper));
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
            _loggerFactory = loggerFactory;
            _shadowPath = shadowPath;
            _isScoped = isScoped;
        }

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

                var virt = ShadowFsPath + "/" + id;
                var shadowDir = hostingEnvironment.MapPathContentRoot(virt);
                if (Directory.Exists(shadowDir))
                    continue;

                Directory.CreateDirectory(shadowDir);
                return id;
            }

            throw new Exception($"Could not get a shadow identifier (tried {retries} times)");
        }

        internal void Shadow(string id)
        {
            // note: no thread-safety here, because ShadowFs is thread-safe due to the check
            // on ShadowFileSystemsScope.None - and if None is false then we should be running
            // in a single thread anyways

            var virt = Path.Combine(ShadowFsPath , id , _shadowPath);
            _shadowDir = _hostingEnvironment.MapPathContentRoot(virt);
            Directory.CreateDirectory(_shadowDir);
            var tempfs = new PhysicalFileSystem(_ioHelper, _hostingEnvironment, _loggerFactory.CreateLogger<PhysicalFileSystem>(), _shadowDir, _hostingEnvironment.ToAbsolute(virt));
            _shadowFileSystem = new ShadowFileSystem(_innerFileSystem, tempfs);
        }

        internal void UnShadow(bool complete)
        {
            var shadowFileSystem = _shadowFileSystem;
            var dir = _shadowDir;
            _shadowFileSystem = null;
            _shadowDir = null;

            try
            {
                // this may throw an AggregateException if some of the changes could not be applied
                if (complete) shadowFileSystem?.Complete();
            }
            finally
            {
                // in any case, cleanup
                try
                {
                    Directory.Delete(dir!, true);

                    // shadowPath make be path/to/dir, remove each
                    dir = dir!.Replace('/', Path.DirectorySeparatorChar);
                    var min = _hostingEnvironment.MapPathContentRoot(ShadowFsPath).Length;
                    var pos = dir.LastIndexOf(Path.DirectorySeparatorChar);
                    while (pos > min)
                    {
                        dir = dir.Substring(0, pos);
                        if (Directory.EnumerateFileSystemEntries(dir).Any() == false)
                            Directory.Delete(dir, true);
                        else
                            break;
                        pos = dir.LastIndexOf(Path.DirectorySeparatorChar);
                    }
                }
                catch
                {
                    // ugly, isn't it? but if we cannot cleanup, bah, just leave it there
                }
            }
        }

        public IFileSystem InnerFileSystem => _innerFileSystem;

        private IFileSystem FileSystem
        {
            get
            {
                if (_isScoped is not null && _shadowFileSystem is not null)
                {
                    var isScoped = _isScoped!();

                    // if the filesystem is created *after* shadowing starts, it won't be shadowing
                    // better not ignore that situation and raised a meaningful (?) exception
                    if ( isScoped.HasValue && isScoped.Value && _shadowFileSystem == null)
                        throw new Exception("The filesystems are shadowing, but this filesystem is not.");

                    return isScoped.HasValue && isScoped.Value
                        ? _shadowFileSystem
                        : _innerFileSystem;
                }

                return _innerFileSystem;
            }
        }

        public IEnumerable<string> GetDirectories(string path)
        {
            return FileSystem.GetDirectories(path);
        }

        public void DeleteDirectory(string path)
        {
            FileSystem.DeleteDirectory(path);
        }

        public void DeleteDirectory(string path, bool recursive)
        {
            FileSystem.DeleteDirectory(path, recursive);
        }

        public bool DirectoryExists(string path)
        {
            return FileSystem.DirectoryExists(path);
        }

        public void AddFile(string path, Stream stream)
        {
            FileSystem.AddFile(path, stream);
        }

        public void AddFile(string path, Stream stream, bool overrideExisting)
        {
            FileSystem.AddFile(path, stream, overrideExisting);
        }

        public IEnumerable<string> GetFiles(string path)
        {
            return FileSystem.GetFiles(path);
        }

        public IEnumerable<string> GetFiles(string path, string filter)
        {
            return FileSystem.GetFiles(path, filter);
        }

        public Stream OpenFile(string path)
        {
            return FileSystem.OpenFile(path);
        }

        public void DeleteFile(string path)
        {
            FileSystem.DeleteFile(path);
        }

        public bool FileExists(string path)
        {
            return FileSystem.FileExists(path);
        }

        public string GetRelativePath(string fullPathOrUrl)
        {
            return FileSystem.GetRelativePath(fullPathOrUrl);
        }

        public string GetFullPath(string path)
        {
            return FileSystem.GetFullPath(path);
        }

        public string GetUrl(string? path)
        {
            return FileSystem.GetUrl(path);
        }

        public DateTimeOffset GetLastModified(string path)
        {
            return FileSystem.GetLastModified(path);
        }

        public DateTimeOffset GetCreated(string path)
        {
            return FileSystem.GetCreated(path);
        }

        public long GetSize(string path)
        {
            return FileSystem.GetSize(path);
        }

        public bool CanAddPhysical => FileSystem.CanAddPhysical;

        public void AddFile(string path, string physicalPath, bool overrideIfExists = true, bool copy = false)
        {
            FileSystem.AddFile(path, physicalPath, overrideIfExists, copy);
        }

        /// <inheritdoc />
        public IFileProvider? Create() => _innerFileSystem.TryCreateFileProvider(out IFileProvider? fileProvider) ? fileProvider : null;
        public Task AddFileAsync(string path, Stream stream) => FileSystem.AddFileAsync(path, stream);
        public Task AddFileAsync(string path, Stream stream, bool overrideIfExists) => FileSystem.AddFileAsync(path, stream, overrideIfExists);
    }
}
