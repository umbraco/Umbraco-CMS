using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Umbraco.Core.IO
{
    internal class PhysicalFileSystem : IFileSystem
    {
        private readonly string _rootPath;
        private readonly string _rootUrl;

        public PhysicalFileSystem(string virtualRoot)
        {
            _rootPath = System.Web.Hosting.HostingEnvironment.MapPath(virtualRoot);
            _rootUrl = VirtualPathUtility.ToAbsolute(virtualRoot);
        }

        public PhysicalFileSystem(string rootPath, string rootUrl)
        {
            if (string.IsNullOrEmpty(rootPath))
                throw new ArgumentException("The argument 'rootPath' cannot be null or empty.");

            if (string.IsNullOrEmpty(rootUrl))
                throw new ArgumentException("The argument 'rootUrl' cannot be null or empty.");

            _rootPath = rootPath;
            _rootUrl = rootUrl;
        }

        public IEnumerable<string> GetDirectories(string path)
        {
            path = EnsureTrailingSeparator(GetFullPath(path));

            try
            {
                if (Directory.Exists(path))
                    return Directory.EnumerateDirectories(path).Select(GetRelativePath);
            }
            catch (UnauthorizedAccessException ex)
            { }
            catch (DirectoryNotFoundException ex)
            { }

            return Enumerable.Empty<string>();
        }

        public void DeleteDirectory(string path)
        {
            DeleteDirectory(path, false);
        }

        public void DeleteDirectory(string path, bool recursive)
        {
            if (!DirectoryExists(path))
                return;

            try
            {
                Directory.Delete(GetFullPath(path), recursive);
            }
            catch (DirectoryNotFoundException ex)
            { }
        }

        public bool DirectoryExists(string path)
        {
            return Directory.Exists(GetFullPath(path));
        }

        public void AddFile(string path, Stream stream)
        {
            AddFile(path, stream, true);
        }

        public void AddFile(string path, Stream stream, bool overrideIfExists)
        {
            if (FileExists(path) && !overrideIfExists)
                throw new InvalidOperationException(string.Format("A file at path '{0}' already exists",
                    path));

            EnsureDirectory(Path.GetDirectoryName(path));

            using (var destination = (Stream)File.Create(GetFullPath(path)))
                stream.CopyTo(destination);
        }

        public IEnumerable<string> GetFiles(string path)
        {
            return GetFiles(path, "*.*");
        }

        public IEnumerable<string> GetFiles(string path, string filter)
        {
            path = EnsureTrailingSeparator(GetFullPath(path));

            try
            {
                if (Directory.Exists(path))
                    return Directory.EnumerateFiles(path, filter).Select(GetRelativePath);
            }
            catch (UnauthorizedAccessException ex)
            { }
            catch (DirectoryNotFoundException ex)
            { }

            return Enumerable.Empty<string>();
        }

        public Stream OpenFile(string path)
        {
            return File.OpenRead(GetFullPath(path));
        }

        public void DeleteFile(string path)
        {
            if (!FileExists(path))
                return;

            try
            {
                File.Delete(GetFullPath(path));
            }
            catch (FileNotFoundException ex)
            { }
        }

        public bool FileExists(string path)
        {
            return File.Exists(GetFullPath(path));
        }

        public string GetRelativePath(string fullPathOrUrl)
        {
            var relativePath = fullPathOrUrl
                .TrimStart(_rootUrl)
                .Replace('/', Path.DirectorySeparatorChar)
                .TrimStart(_rootPath)
                .TrimStart(Path.DirectorySeparatorChar);

            return relativePath;
        }

        public string GetFullPath(string path)
        {
            return !path.StartsWith(_rootPath) 
                ? Path.Combine(_rootPath, path)
                : path;
        }

        public string GetUrl(string path)
        {
            return _rootUrl.TrimEnd("/") + "/" + path
                .TrimStart(Path.DirectorySeparatorChar)
                .Replace(Path.DirectorySeparatorChar, '/');
        }

        public DateTimeOffset GetLastModified(string path)
        {
            return DirectoryExists(path) 
                ? new DirectoryInfo(GetFullPath(path)).LastWriteTimeUtc 
                : new FileInfo(GetFullPath(path)).LastWriteTimeUtc;
        }

        public DateTimeOffset GetCreated(string path)
        {
            return DirectoryExists(path) 
                ? Directory.GetCreationTimeUtc(GetFullPath(path)) 
                : File.GetCreationTimeUtc(GetFullPath(path));
        }

        #region Helper Methods

        protected virtual void EnsureDirectory(string path)
        {
            path = GetFullPath(path);
            Directory.CreateDirectory(path);
        }

        protected string EnsureTrailingSeparator(string path)
        {
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                path = path + Path.DirectorySeparatorChar;

            return path;
        }

        #endregion
    }
}
