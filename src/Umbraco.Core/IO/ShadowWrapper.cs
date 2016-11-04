using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Umbraco.Core.IO
{
    internal class ShadowWrapper : IFileSystem
    {
        private readonly IFileSystem _innerFileSystem;
        private readonly string _shadowPath;
        private ShadowFileSystem _shadowFileSystem;
        private string _shadowDir;

        public ShadowWrapper(IFileSystem innerFileSystem, string shadowPath)
        {
            _innerFileSystem = innerFileSystem;
            _shadowPath = shadowPath;
        }

        internal void Shadow(Guid id)
        {
            // note: no thread-safety here, because ShadowFs is thread-safe due to the check
            // on ShadowFileSystemsScope.None - and if None is false then we should be running
            // in a single thread anyways

            var virt = "~/App_Data/Shadow/" + id + "/" + _shadowPath;
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
                    dir = dir.Substring(0, dir.Length - _shadowPath.Length - 1);
                    if (Directory.EnumerateFileSystemEntries(dir).Any() == false)
                        Directory.Delete(dir, true);
                }
                catch
                {
                    // ugly, isn't it? but if we cannot cleanup, bah, just leave it there
                }
            }
        }

        private IFileSystem FileSystem
        {
            get { return ShadowFileSystemsScope.NoScope ? _innerFileSystem : _shadowFileSystem; }
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
            return FileSystem.GetSize(path);
        }
    }
}