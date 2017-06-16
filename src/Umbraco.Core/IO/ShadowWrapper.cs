using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.IO
{
    internal class ShadowWrapper : IFileSystem2
    {
        private readonly IScopeProviderInternal _scopeProvider;
        private readonly IFileSystem _innerFileSystem;
        private readonly string _shadowPath;
        private ShadowFileSystem _shadowFileSystem;
        private string _shadowDir;

        public ShadowWrapper(IFileSystem innerFileSystem, string shadowPath, IScopeProviderInternal scopeProvider)
        {
            _innerFileSystem = innerFileSystem;
            _shadowPath = shadowPath;
            _scopeProvider = scopeProvider;
        }

        internal void Shadow(Guid id)
        {
            // note: no thread-safety here, because ShadowFs is thread-safe due to the check
            // on ShadowFileSystemsScope.None - and if None is false then we should be running
            // in a single thread anyways

            var virt = "~/App_Data/TEMP/ShadowFs/" + id + "/" + _shadowPath;
            _shadowDir = IOHelper.MapPath(virt);
            Directory.CreateDirectory(_shadowDir);
            var tempfs = new PhysicalFileSystem(virt);
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
                if (complete) shadowFileSystem.Complete();
            }
            finally
            {
                // in any case, cleanup
                try
                {
                    Directory.Delete(dir, true);

                    // shadowPath make be path/to/dir, remove each
                    dir = dir.Replace("/", "\\");
                    var min = IOHelper.MapPath("~/App_Data/TEMP/ShadowFs").Length;
                    var pos = dir.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase);
                    while (pos > min)
                    {
                        dir = dir.Substring(0, pos);
                        if (Directory.EnumerateFileSystemEntries(dir).Any() == false)
                            Directory.Delete(dir, true);
                        else
                            break;
                        pos = dir.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase);
                    }
                }
                catch
                {
                    // ugly, isn't it? but if we cannot cleanup, bah, just leave it there
                }
            }
        }

        private IFileSystem FileSystem
        {
            get
            {
                var isScoped = _scopeProvider != null && _scopeProvider.AmbientScope != null && _scopeProvider.AmbientScope.ScopedFileSystems;

                // if the filesystem is created *after* shadowing starts, it won't be shadowing
                // better not ignore that situation and raised a meaningful (?) exception
                if (isScoped && _shadowFileSystem == null)
                    throw new Exception("The filesystems are shadowing, but this filesystem is not.");

                return isScoped
                    ? _shadowFileSystem
                    : _innerFileSystem;
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

        public string GetUrl(string path)
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
            var filesystem = FileSystem; // will be either a ShadowFileSystem OR the actual underlying IFileSystem

            // and the underlying filesystem can be IFileSystem2... or just IFileSystem
            // figure it out and use the most effective GetSize method
            var filesystem2 = filesystem as IFileSystem2;
            return filesystem2 == null ? filesystem.GetSize(path) : filesystem2.GetSize(path);
        }

        public bool CanAddPhysical
        {
            get
            {
                var fileSystem2 = FileSystem as IFileSystem2;
                return fileSystem2 != null && fileSystem2.CanAddPhysical;
            }
        }

        public void AddFile(string path, string physicalPath, bool overrideIfExists = true, bool copy = false)
        {
            var fileSystem2 = FileSystem as IFileSystem2;
            if (fileSystem2 == null)
                throw new NotSupportedException();
            fileSystem2.AddFile(path, physicalPath, overrideIfExists, copy);
        }
    }
}