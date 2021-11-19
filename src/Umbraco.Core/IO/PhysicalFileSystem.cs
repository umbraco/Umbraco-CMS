using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Umbraco.Core.Composing;
using System.Threading;
using Umbraco.Core.Logging;

namespace Umbraco.Core.IO
{
    public class PhysicalFileSystem : IFileSystem
    {
        // the rooted, filesystem path, using directory separator chars, NOT ending with a separator
        // eg "c:" or "c:\path\to\site" or "\\server\path"
        private readonly string _rootPath;

        // _rootPath, but with separators replaced by forward-slashes
        // eg "c:" or "c:/path/to/site" or "//server/path"
        // (is used in GetRelativePath)
        private readonly string _rootPathFwd;

        // the relative URL, using URL separator chars, NOT ending with a separator
        // eg "" or "/Views" or "/Media" or "/<vpath>/Media" in case of a virtual path
        private readonly string _rootUrl;

        // virtualRoot should be "~/path/to/root" eg "~/Views"
        // the "~/" is mandatory.
        public PhysicalFileSystem(string virtualRoot)
        {
            if (virtualRoot == null) throw new ArgumentNullException("virtualRoot");
            if (virtualRoot.StartsWith("~/") == false)
                throw new ArgumentException("The virtualRoot argument must be a virtual path and start with '~/'");

            _rootPath = EnsureDirectorySeparatorChar(IOHelper.MapPath(virtualRoot)).TrimEnd(Path.DirectorySeparatorChar);
            _rootPathFwd = EnsureUrlSeparatorChar(_rootPath);
            _rootUrl = EnsureUrlSeparatorChar(IOHelper.ResolveUrl(virtualRoot)).TrimEnd(Constants.CharArrays.ForwardSlash);
        }

        public PhysicalFileSystem(string rootPath, string rootUrl)
        {
            if (rootPath == null) throw new ArgumentNullException(nameof(rootPath));
            if (string.IsNullOrEmpty(rootPath)) throw new ArgumentException("Value can't be empty.", nameof(rootPath));
            if (rootUrl == null) throw new ArgumentNullException(nameof(rootUrl));
            if (string.IsNullOrEmpty(rootUrl)) throw new ArgumentException("Value can't be empty.", nameof(rootUrl));
            if (rootPath.StartsWith("~/")) throw new ArgumentException("Value can't be a virtual path and start with '~/'.", nameof(rootPath));

            // rootPath should be... rooted, as in, it's a root path!
            if (Path.IsPathRooted(rootPath) == false)
            {
                // but the test suite App.config cannot really "root" anything so we have to do it here
                var localRoot = IOHelper.GetRootDirectorySafe();
                rootPath = Path.Combine(localRoot, rootPath);
            }

            _rootPath = EnsureDirectorySeparatorChar(rootPath).TrimEnd(Path.DirectorySeparatorChar);
            _rootPathFwd = EnsureUrlSeparatorChar(_rootPath);
            _rootUrl = EnsureUrlSeparatorChar(rootUrl).TrimEnd(Constants.CharArrays.ForwardSlash);
        }

        /// <summary>
        /// Gets directories in a directory.
        /// </summary>
        /// <param name="path">The filesystem-relative path to the directory.</param>
        /// <returns>The filesystem-relative path to the directories in the directory.</returns>
        /// <remarks>Filesystem-relative paths use forward-slashes as directory separators.</remarks>
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
                Current.Logger.Error<PhysicalFileSystem,string>(ex, "Not authorized to get directories for '{Path}'", fullPath);
            }
            catch (DirectoryNotFoundException ex)
            {
                Current.Logger.Error<PhysicalFileSystem,string>(ex, "Directory not found for '{Path}'", fullPath);
            }

            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Deletes a directory.
        /// </summary>
        /// <param name="path">The filesystem-relative path of the directory.</param>
        public void DeleteDirectory(string path)
        {
            DeleteDirectory(path, false);
        }

        /// <summary>
        /// Deletes a directory.
        /// </summary>
        /// <param name="path">The filesystem-relative path of the directory.</param>
        /// <param name="recursive">A value indicating whether to recursively delete sub-directories.</param>
        public void DeleteDirectory(string path, bool recursive)
        {
            var fullPath = GetFullPath(path);
            if (Directory.Exists(fullPath) == false)
                return;

            try
            {
                WithRetry(() => Directory.Delete(fullPath, recursive));
            }
            catch (DirectoryNotFoundException ex)
            {
                Current.Logger.Error<PhysicalFileSystem,string>(ex, "Directory not found for '{Path}'", fullPath);
            }
        }

        /// <summary>
        /// Gets a value indicating whether a directory exists.
        /// </summary>
        /// <param name="path">The filesystem-relative path of the directory.</param>
        /// <returns>A value indicating whether a directory exists.</returns>
        public bool DirectoryExists(string path)
        {
            var fullPath = GetFullPath(path);
            return Directory.Exists(fullPath);
        }

        /// <summary>
        /// Saves a file.
        /// </summary>
        /// <param name="path">The filesystem-relative path of the file.</param>
        /// <param name="stream">A stream containing the file data.</param>
        /// <remarks>Overrides the existing file, if any.</remarks>
        public void AddFile(string path, Stream stream)
        {
            AddFile(path, stream, true);
        }

        /// <summary>
        /// Saves a file.
        /// </summary>
        /// <param name="path">The filesystem-relative path of the file.</param>
        /// <param name="stream">A stream containing the file data.</param>
        /// <param name="overrideExisting">A value indicating whether to override the existing file, if any.</param>
        /// <remarks>If a file exists and <paramref name="overrideExisting"/> is false, an exception is thrown.</remarks>
        public void AddFile(string path, Stream stream, bool overrideExisting)
        {
            var fullPath = GetFullPath(path);
            var exists = File.Exists(fullPath);
            if (exists && overrideExisting == false)
                throw new InvalidOperationException(string.Format("A file at path '{0}' already exists", path));

            var directory = Path.GetDirectoryName(fullPath);
            if (directory == null) throw new InvalidOperationException("Could not get directory.");
            Directory.CreateDirectory(directory); // ensure it exists

            if (stream.CanSeek) // TODO: what if we cannot?
                stream.Seek(0, 0);

            using (var destination = (Stream) File.Create(fullPath))
                stream.CopyTo(destination);
        }

        /// <summary>
        /// Gets files in a directory.
        /// </summary>
        /// <param name="path">The filesystem-relative path of the directory.</param>
        /// <returns>The filesystem-relative path to the files in the directory.</returns>
        /// <remarks>Filesystem-relative paths use forward-slashes as directory separators.</remarks>
        public IEnumerable<string> GetFiles(string path)
        {
            return GetFiles(path, "*.*");
        }

        /// <summary>
        /// Gets files in a directory.
        /// </summary>
        /// <param name="path">The filesystem-relative path of the directory.</param>
        /// <param name="filter">A filter.</param>
        /// <returns>The filesystem-relative path to the matching files in the directory.</returns>
        /// <remarks>Filesystem-relative paths use forward-slashes as directory separators.</remarks>
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
                Current.Logger.Error<PhysicalFileSystem,string>(ex, "Not authorized to get directories for '{Path}'", fullPath);
            }
            catch (DirectoryNotFoundException ex)
            {
                Current.Logger.Error<PhysicalFileSystem, string>(ex, "Directory not found for '{FullPath}'", fullPath);
            }

            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Opens a file.
        /// </summary>
        /// <param name="path">The filesystem-relative path to the file.</param>
        /// <returns></returns>
        public Stream OpenFile(string path)
        {
            var fullPath = GetFullPath(path);
            return File.OpenRead(fullPath);
        }

        /// <summary>
        /// Deletes a file.
        /// </summary>
        /// <param name="path">The filesystem-relative path to the file.</param>
        public void DeleteFile(string path)
        {
            var fullPath = GetFullPath(path);
            if (File.Exists(fullPath) == false)
                return;

            try
            {
                WithRetry(() => File.Delete(fullPath));
            }
            catch (FileNotFoundException ex)
            {
                Current.Logger.Error<PhysicalFileSystem, string>(ex.InnerException, "DeleteFile failed with FileNotFoundException for '{Path}'", fullPath);
            }
        }

        /// <summary>
        /// Gets a value indicating whether a file exists.
        /// </summary>
        /// <param name="path">The filesystem-relative path to the file.</param>
        /// <returns>A value indicating whether the file exists.</returns>
        public bool FileExists(string path)
        {
            var fullpath = GetFullPath(path);
            return File.Exists(fullpath);
        }

        /// <summary>
        /// Gets the filesystem-relative path of a full path or of an URL.
        /// </summary>
        /// <param name="fullPathOrUrl">The full path or URL.</param>
        /// <returns>The path, relative to this filesystem's root.</returns>
        /// <remarks>
        /// <para>The relative path is relative to this filesystem's root, not starting with any
        /// directory separator. All separators are forward-slashes.</para>
        /// </remarks>
        public string GetRelativePath(string fullPathOrUrl)
        {
            // test URL
            var path = fullPathOrUrl.Replace('\\', '/'); // ensure URL separator char

            // if it starts with the root URL, strip it and trim the starting slash to make it relative
            // eg "/Media/1234/img.jpg" => "1234/img.jpg"
            if (IOHelper.PathStartsWith(path, _rootUrl, '/'))
                return path.Substring(_rootUrl.Length).TrimStart(Constants.CharArrays.ForwardSlash);

            // if it starts with the root path, strip it and trim the starting slash to make it relative
            // eg "c:/websites/test/root/Media/1234/img.jpg" => "1234/img.jpg"
            if (IOHelper.PathStartsWith(path, _rootPathFwd, '/'))
                return path.Substring(_rootPathFwd.Length).TrimStart(Constants.CharArrays.ForwardSlash);

            // unchanged - what else?
            return path;
        }

        /// <summary>
        /// Gets the full path.
        /// </summary>
        /// <param name="path">The full or filesystem-relative path.</param>
        /// <returns>The full path.</returns>
        /// <remarks>
        /// <para>On the physical filesystem, the full path is the rooted (ie non-relative), safe (ie within this
        /// filesystem's root) path. All separators are Path.DirectorySeparatorChar.</para>
        /// </remarks>
        public string GetFullPath(string path)
        {
            // normalize
            var opath = path;
            path = EnsureDirectorySeparatorChar(path);

            // FIXME: this part should go!
            // not sure what we are doing here - so if input starts with a (back) slash,
            // we assume it's not a FS relative path and we try to convert it... but it
            // really makes little sense?
            if (path.StartsWith(Path.DirectorySeparatorChar.ToString()))
                path = GetRelativePath(path);

            // if not already rooted, combine with the root path
            if (IOHelper.PathStartsWith(path, _rootPath, Path.DirectorySeparatorChar) == false)
                path = Path.Combine(_rootPath, path);

            // sanitize - GetFullPath will take care of any relative
            // segments in path, eg '../../foo.tmp' - it may throw a SecurityException
            // if the combined path reaches illegal parts of the filesystem
            path = Path.GetFullPath(path);

            // at that point, path is within legal parts of the filesystem, ie we have
            // permissions to reach that path, but it may nevertheless be outside of
            // our root path, due to relative segments, so better check
            if (IOHelper.PathStartsWith(path, _rootPath, Path.DirectorySeparatorChar))
            {
                // this says that 4.7.2 supports long paths - but Windows does not
                // https://docs.microsoft.com/en-us/dotnet/api/system.io.pathtoolongexception?view=netframework-4.7.2
                if (path.Length > 260)
                    throw new PathTooLongException($"Path {path} is too long.");
                return path;
            }

            // nothing prevents us to reach the file, security-wise, yet it is outside
            // this filesystem's root - throw
            throw new UnauthorizedAccessException("File '" + opath + "' is outside this filesystem's root.");
        }

        /// <summary>
        /// Gets the URL.
        /// </summary>
        /// <param name="path">The filesystem-relative path.</param>
        /// <returns>The URL.</returns>
        /// <remarks>All separators are forward-slashes.</remarks>
        public string GetUrl(string path)
        {
            path = EnsureUrlSeparatorChar(path).Trim(Constants.CharArrays.ForwardSlash);
            return _rootUrl + "/" + path;
        }

        /// <summary>
        /// Gets the last-modified date of a directory or file.
        /// </summary>
        /// <param name="path">The filesystem-relative path to the directory or the file.</param>
        /// <returns>The last modified date of the directory or the file.</returns>
        public DateTimeOffset GetLastModified(string path)
        {
            var fullpath = GetFullPath(path);
            return DirectoryExists(fullpath)
                ? new DirectoryInfo(fullpath).LastWriteTimeUtc
                : new FileInfo(fullpath).LastWriteTimeUtc;
        }

        /// <summary>
        /// Gets the created date of a directory or file.
        /// </summary>
        /// <param name="path">The filesystem-relative path to the directory or the file.</param>
        /// <returns>The created date of the directory or the file.</returns>
        public DateTimeOffset GetCreated(string path)
        {
            var fullpath = GetFullPath(path);
            return DirectoryExists(fullpath)
                ? Directory.GetCreationTimeUtc(fullpath)
                : File.GetCreationTimeUtc(fullpath);
        }

        /// <summary>
        /// Gets the size of a file.
        /// </summary>
        /// <param name="path">The filesystem-relative path to the file.</param>
        /// <returns>The file of the size, in bytes.</returns>
        /// <remarks>If the file does not exist, returns -1.</remarks>
        public long GetSize(string path)
        {
            var fullPath = GetFullPath(path);
            var file = new FileInfo(fullPath);
            return file.Exists ? file.Length : -1;
        }

        public bool CanAddPhysical => true;

        public void AddFile(string path, string physicalPath, bool overrideIfExists = true, bool copy = false)
        {
            var fullPath = GetFullPath(path);

            if (File.Exists(fullPath))
            {
                if (overrideIfExists == false)
                    throw new InvalidOperationException($"A file at path '{path}' already exists");
                WithRetry(() => File.Delete(fullPath));
            }

            var directory = Path.GetDirectoryName(fullPath);
            if (directory == null) throw new InvalidOperationException("Could not get directory.");
            Directory.CreateDirectory(directory); // ensure it exists

            if (copy)
                WithRetry(() => File.Copy(physicalPath, fullPath));
            else
                WithRetry(() => File.Move(physicalPath, fullPath));
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

        protected void WithRetry(Action action)
        {
            // 10 times 100ms is 1s
            const int count = 10;
            const int pausems = 100;

            for (var i = 0;; i++)
            {
                try
                {
                    action();
                    break; // done
                }
                catch (IOException e)
                {
                    // if it's not *exactly* IOException then it could be
                    // some inherited exception such as FileNotFoundException,
                    // and then we don't want to retry
                    if (e.GetType() != typeof(IOException)) throw;

                    // if we have tried enough, throw, else swallow
                    // the exception and retry after a pause
                    if (i == count) throw;
                }

                Thread.Sleep(pausems);
            }
        }

        #endregion
    }
}
