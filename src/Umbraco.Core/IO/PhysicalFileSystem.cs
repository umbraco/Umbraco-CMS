using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Umbraco.Core.Logging;

namespace Umbraco.Core.IO
{
    public class PhysicalFileSystem : IFileSystem
    {
        // the rooted, filesystem path, using directory separator chars, NOT ending with a separator
        // eg "c:" or "c:\path\to\site" or "\\server\path"
        private readonly string _rootPath;

        // the ??? url, using url separator chars, NOT ending with a separator
        // eg "" (?) or "/Scripts" or ???
        private readonly string _rootUrl;

        public PhysicalFileSystem(string virtualRoot)
        {
	        if (virtualRoot == null) throw new ArgumentNullException("virtualRoot");
			if (virtualRoot.StartsWith("~/") == false)
				throw new ArgumentException("The virtualRoot argument must be a virtual path and start with '~/'");

            _rootPath = IOHelper.MapPath(virtualRoot);
            _rootPath = EnsureDirectorySeparatorChar(_rootPath);
            _rootPath = _rootPath.TrimEnd(Path.DirectorySeparatorChar);

            _rootUrl = IOHelper.ResolveUrl(virtualRoot);
            _rootUrl = EnsureUrlSeparatorChar(_rootUrl);
            _rootUrl = _rootUrl.TrimEnd('/');
        }

        public PhysicalFileSystem(string rootPath, string rootUrl)
        {
            if (string.IsNullOrEmpty(rootPath))
                throw new ArgumentException("The argument 'rootPath' cannot be null or empty.");

            if (string.IsNullOrEmpty(rootUrl))
                throw new ArgumentException("The argument 'rootUrl' cannot be null or empty.");

			if (rootPath.StartsWith("~/"))
				throw new ArgumentException("The rootPath argument cannot be a virtual path and cannot start with '~/'");

            // rootPath should be... rooted, as in, it's a root path!
            // but the test suite App.config cannot really "root" anything so we'll have to do it here

            //var localRoot = AppDomain.CurrentDomain.BaseDirectory;
            var localRoot = IOHelper.GetRootDirectorySafe();
            if (Path.IsPathRooted(rootPath) == false)
            {
                rootPath = Path.Combine(localRoot, rootPath);
            }

            rootPath = EnsureDirectorySeparatorChar(rootPath);
            rootUrl = EnsureUrlSeparatorChar(rootUrl);

            _rootPath = rootPath.TrimEnd(Path.DirectorySeparatorChar);
            _rootUrl = rootUrl.TrimEnd('/');
        }

        public IEnumerable<string> GetDirectories(string path)
        {
            var fullPath = GetFullPath(path);

            try
            {
                if (Directory.Exists(fullPath))
                    return Directory.EnumerateDirectories(fullPath).Select(GetRelativePath);
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
            var fullPath = GetFullPath(path);
            if (Directory.Exists(fullPath) == false)
                return;

            try
            {
                Directory.Delete(fullPath, recursive);
            }
            catch (DirectoryNotFoundException ex)
            {
                LogHelper.Error<PhysicalFileSystem>("Directory not found", ex);
            }
        }

        public bool DirectoryExists(string path)
        {
            var fullPath = GetFullPath(path);
            return Directory.Exists(fullPath);
        }

        public void AddFile(string path, Stream stream)
        {
            AddFile(path, stream, true);
        }

        public void AddFile(string path, Stream stream, bool overrideIfExists)
        {
            var fullPath = GetFullPath(path);
            var exists = File.Exists(fullPath);
            if (exists && overrideIfExists == false) 
                throw new InvalidOperationException(string.Format("A file at path '{0}' already exists", path));

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)); // ensure it exists

            if (stream.CanSeek)
                stream.Seek(0, 0);

            using (var destination = (Stream)File.Create(fullPath))
                stream.CopyTo(destination);
        }

        public IEnumerable<string> GetFiles(string path)
        {
            return GetFiles(path, "*.*");
        }

        public IEnumerable<string> GetFiles(string path, string filter)
        {
            var fullPath = GetFullPath(path);

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
            var fullPath = GetFullPath(path);
            if (File.Exists(fullPath) == false)
                return;

            try
            {
                File.Delete(fullPath);
            }
            catch (FileNotFoundException ex)
            {
                LogHelper.Info<PhysicalFileSystem>(string.Format("DeleteFile failed with FileNotFoundException: {0}", ex.InnerException));
            }
        }

        public bool FileExists(string path)
        {
            var fullpath = GetFullPath(path);
            return File.Exists(fullpath);
        }

        // beware, many things depend on how the GetRelative/AbsolutePath methods work!

        /// <summary>
        /// Gets the relative path.
        /// </summary>
        /// <param name="fullPathOrUrl">The full path or url.</param>
        /// <returns>The path, relative to this filesystem's root.</returns>
        /// <remarks>
        /// <para>The relative path is relative to this filesystem's root, not starting with any
        /// directory separator. If input was recognized as a url (path), then output uses url (path) separator
        /// chars.</para>
        /// </remarks>
        public string GetRelativePath(string fullPathOrUrl)
        {
            // test url
            var path = fullPathOrUrl.Replace('\\', '/'); // ensure url separator char

            if (IOHelper.PathStartsWith(path, _rootUrl, '/')) // if it starts with the root url...
                return path.Substring(_rootUrl.Length) // strip it
                            .TrimStart('/'); // it's relative

            // test path
            path = EnsureDirectorySeparatorChar(fullPathOrUrl);

            if (IOHelper.PathStartsWith(path, _rootPath, Path.DirectorySeparatorChar)) // if it starts with the root path
                return path.Substring(_rootPath.Length) // strip it
                            .TrimStart(Path.DirectorySeparatorChar); // it's relative

            // unchanged - including separators
            return fullPathOrUrl;
        }

        /// <summary>
        /// Gets the full path.
        /// </summary>
        /// <param name="path">The full or relative path.</param>
        /// <returns>The full path.</returns>
        /// <remarks>
        /// <para>On the physical filesystem, the full path is the rooted (ie non-relative), safe (ie within this
        /// filesystem's root) path. All separators are converted to Path.DirectorySeparatorChar.</para>
        /// </remarks>
        public string GetFullPath(string path)
        {
            // normalize
            var opath = path;
            path = EnsureDirectorySeparatorChar(path);

            // not sure what we are doing here - so if input starts with a (back) slash,
            // we assume it's not a FS relative path and we try to convert it... but it
            // really makes little sense?
            if (path.StartsWith(Path.DirectorySeparatorChar.ToString()))
                path = GetRelativePath(path);

            // if already a full path, return
            if (IOHelper.PathStartsWith(path, _rootPath, Path.DirectorySeparatorChar))
                return path;

            // else combine and sanitize, ie GetFullPath will take care of any relative
            // segments in path, eg '../../foo.tmp' - it may throw a SecurityException
            // if the combined path reaches illegal parts of the filesystem
            var fpath = Path.Combine(_rootPath, path);
            fpath = Path.GetFullPath(fpath);

            // at that point, path is within legal parts of the filesystem, ie we have
            // permissions to reach that path, but it may nevertheless be outside of
            // our root path, due to relative segments, so better check
            if (IOHelper.PathStartsWith(fpath, _rootPath, Path.DirectorySeparatorChar))
                return fpath;

            throw new FileSecurityException("File '" + opath + "' is outside this filesystem's root.");
        }

        public string GetUrl(string path)
        {
            path = EnsureUrlSeparatorChar(path).Trim('/');
            return _rootUrl + "/" + path;
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
            return path.EnsureEndsWith(Path.DirectorySeparatorChar);
        }

        protected string EnsureDirectorySeparatorChar(string path)
        {
            path = path.Replace('/', Path.DirectorySeparatorChar);
            path = path.Replace('\\', Path.DirectorySeparatorChar);
            return path;
        }

        protected string EnsureUrlSeparatorChar(string path)
        {
            path = path.Replace('\\', '/');
            return path;
        }

        #endregion
    }
}
