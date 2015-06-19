using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO; 
using System.Linq;
using Umbraco.Core.Logging;

namespace Umbraco.Core.IO
{
    public class PhysicalFileSystem : IFileSystem
    {
		internal string RootPath { get; private set; }
        private readonly string _rootUrl;

        public PhysicalFileSystem(string virtualRoot)
        {
	        if (virtualRoot == null) throw new ArgumentNullException("virtualRoot");
			if (virtualRoot.StartsWith("~/") == false)
				throw new ArgumentException("The virtualRoot argument must be a virtual path and start with '~/'");

	        RootPath = IOHelper.MapPath(virtualRoot);
            _rootUrl = IOHelper.ResolveUrl(virtualRoot);
        }

        public PhysicalFileSystem(string rootPath, string rootUrl)
        {
            if (string.IsNullOrEmpty(rootPath))
                throw new ArgumentException("The argument 'rootPath' cannot be null or empty.");

            if (string.IsNullOrEmpty(rootUrl))
                throw new ArgumentException("The argument 'rootUrl' cannot be null or empty.");

			if (rootPath.StartsWith("~/"))
				throw new ArgumentException("The rootPath argument cannot be a virtual path and cannot start with '~/'");

            RootPath = rootPath;
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
            {
                LogHelper.Error<PhysicalFileSystem>("Not authorized to get directories", ex);
            }
            catch (DirectoryNotFoundException ex)
            {
                LogHelper.Error<PhysicalFileSystem>("Directory not found", ex);
            }

            return Enumerable.Empty<string>();
        }

        public void DeleteDirectory(string path)
        {
            DeleteDirectory(path, false);
        }

        public void DeleteDirectory(string path, bool recursive)
        {
            if (DirectoryExists(path) == false)
                return;

            try
            {
                Directory.Delete(GetFullPath(path), recursive);
            }
            catch (DirectoryNotFoundException ex)
            {
                LogHelper.Error<PhysicalFileSystem>("Directory not found", ex);
            }
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
            var fsRelativePath = GetRelativePath(path);

            var exists = FileExists(fsRelativePath);
            if (exists && overrideIfExists == false) throw new InvalidOperationException(string.Format("A file at path '{0}' already exists", path));

            EnsureDirectory(Path.GetDirectoryName(fsRelativePath));

            if (stream.CanSeek)
                stream.Seek(0, 0);

            using (var destination = (Stream)File.Create(GetFullPath(fsRelativePath)))
                stream.CopyTo(destination);
        }

        public IEnumerable<string> GetFiles(string path)
        {
            return GetFiles(path, "*.*");
        }

        public IEnumerable<string> GetFiles(string path, string filter)
        {
            var fsRelativePath = GetRelativePath(path);

            var fullPath = EnsureTrailingSeparator(GetFullPath(fsRelativePath));

            try
            {
                if (Directory.Exists(fullPath))
                    return Directory.EnumerateFiles(fullPath, filter).Select(GetRelativePath);
            }
            catch (UnauthorizedAccessException ex)
            {
                LogHelper.Error<PhysicalFileSystem>("Not authorized to get directories", ex);
            }
            catch (DirectoryNotFoundException ex)
            {
                LogHelper.Error<PhysicalFileSystem>("Directory not found", ex);
            }

            return Enumerable.Empty<string>();
        }

        public Stream OpenFile(string path)
        {
            var fullPath = GetFullPath(path);
            return File.OpenRead(fullPath);
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
            {
                LogHelper.Info<PhysicalFileSystem>(string.Format("DeleteFile failed with FileNotFoundException: {0}", ex.InnerException));
            }
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
                .TrimStart(RootPath)
                .TrimStart(Path.DirectorySeparatorChar);

            return relativePath;
        }

        public string GetFullPath(string path)
        {
            //if the path starts with a '/' then it's most likely not a FS relative path which is required so convert it
            if (path.StartsWith("/"))
            {
                path = GetRelativePath(path);
            }

            return !path.StartsWith(RootPath) 
                ? Path.Combine(RootPath, path)
                : path;
        }

        public string GetUrl(string path)
        {
            return _rootUrl.TrimEnd("/") + "/" + path
                .TrimStart(Path.DirectorySeparatorChar)
                .Replace(Path.DirectorySeparatorChar, '/')
                .TrimEnd("/");
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
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal))
                path = path + Path.DirectorySeparatorChar;

            return path;
        }

        #endregion
    }
}
