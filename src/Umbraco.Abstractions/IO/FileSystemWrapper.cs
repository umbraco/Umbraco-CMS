using System;
using System.Collections.Generic;
using System.IO;

namespace Umbraco.Core.IO
{
    /// <summary>
    /// All custom file systems that are based upon another IFileSystem should inherit from FileSystemWrapper
    /// </summary>
    /// <remarks>
    /// An IFileSystem is generally used as a base file system, for example like a PhysicalFileSystem or an S3FileSystem.
    /// Then, other custom file systems are wrapped upon these files systems like MediaFileSystem, etc... All of the custom
    /// file systems must inherit from FileSystemWrapper.
    ///
    /// This abstract class just wraps the 'real' IFileSystem object passed in to its constructor.
    /// </remarks>
    public abstract class FileSystemWrapper : IFileSystem
    {
        protected FileSystemWrapper(IFileSystem innerFileSystem)
        {
            InnerFileSystem = innerFileSystem;
        }

        internal IFileSystem InnerFileSystem { get; set; }

        public IEnumerable<string> GetDirectories(string path)
        {
            return InnerFileSystem.GetDirectories(path);
        }

        public void DeleteDirectory(string path)
        {
            InnerFileSystem.DeleteDirectory(path);
        }

        public void DeleteDirectory(string path, bool recursive)
        {
            InnerFileSystem.DeleteDirectory(path, recursive);
        }

        public bool DirectoryExists(string path)
        {
            return InnerFileSystem.DirectoryExists(path);
        }

        public void AddFile(string path, Stream stream)
        {
            InnerFileSystem.AddFile(path, stream);
        }

        public void AddFile(string path, Stream stream, bool overrideExisting)
        {
            InnerFileSystem.AddFile(path, stream, overrideExisting);
        }

        public IEnumerable<string> GetFiles(string path)
        {
            return InnerFileSystem.GetFiles(path);
        }

        public IEnumerable<string> GetFiles(string path, string filter)
        {
            return InnerFileSystem.GetFiles(path, filter);
        }

        public Stream OpenFile(string path)
        {
            return InnerFileSystem.OpenFile(path);
        }

        public void DeleteFile(string path)
        {
            InnerFileSystem.DeleteFile(path);
        }

        public bool FileExists(string path)
        {
            return InnerFileSystem.FileExists(path);
        }

        public string GetRelativePath(string fullPathOrUrl)
        {
            return InnerFileSystem.GetRelativePath(fullPathOrUrl);
        }

        public string GetFullPath(string path)
        {
            return InnerFileSystem.GetFullPath(path);
        }

        public string GetUrl(string path)
        {
            return InnerFileSystem.GetUrl(path);
        }

        public DateTimeOffset GetLastModified(string path)
        {
            return InnerFileSystem.GetLastModified(path);
        }

        public DateTimeOffset GetCreated(string path)
        {
            return InnerFileSystem.GetCreated(path);
        }

        public long GetSize(string path)
        {
            return InnerFileSystem.GetSize(path);
        }

        public bool CanAddPhysical => InnerFileSystem.CanAddPhysical;

        public void AddFile(string path, string physicalPath, bool overrideIfExists = true, bool copy = false)
        {
            InnerFileSystem.AddFile(path, physicalPath, overrideIfExists, copy);
        }
    }
}
