using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.IO.MediaPathSchemes
{
    /// <summary>
    /// Implements the original media path scheme.
    /// </summary>
    /// <remarks>
    /// <para>Path is "{number}/{filename}" or "{number}-{filename}" where number is an incremented counter.</para>
    /// <para>Use '/' or '-' depending on UploadAllowDirectories setting.</para>
    /// </remarks>
    // scheme: path is "<number>/<filename>" where number is an incremented counter
    public class OriginalMediaPathScheme : IMediaPathScheme
    {
        private readonly object _folderCounterLock = new object();
        private IFileSystem _filesystem;
        private long _folderCounter;
        private bool _folderCounterInitialized;

        /// <inheritdoc />
        public void Initialize(IFileSystem filesystem)
        {
            _filesystem = filesystem;
        }

        /// <inheritdoc />
        public string GetFilePath(Guid itemGuid, Guid propertyGuid, string filename, string previous = null)
        {
            string directory;
            if (previous != null)
            {
                // old scheme, with a previous path
                // prevpath should be "<int>/<filename>" OR "<int>-<filename>"
                // and we want to reuse the "<int>" part, so try to find it

                var sep = UmbracoConfig.For.UmbracoSettings().Content.UploadAllowDirectories ? "/" : "-";
                var pos = previous.IndexOf(sep, StringComparison.Ordinal);
                var s = pos > 0 ? previous.Substring(0, pos) : null;

                directory = pos > 0 && int.TryParse(s, out _) ? s : GetNextDirectory();
            }
            else
            {
                directory = GetNextDirectory();
            }

            if (directory == null)
                throw new InvalidOperationException("Cannot use a null directory.");

            return UmbracoConfig.For.UmbracoSettings().Content.UploadAllowDirectories
                ? Path.Combine(directory, filename).Replace('\\', '/')
                : directory + "-" + filename;
        }

        /// <inheritdoc />
        public string GetDeleteDirectory(string filepath)
        {
            return Path.GetDirectoryName(filepath);
        }

        private string GetNextDirectory()
        {
            EnsureFolderCounterIsInitialized();
            return Interlocked.Increment(ref _folderCounter).ToString(CultureInfo.InvariantCulture);
        }

        private void EnsureFolderCounterIsInitialized()
        {
            lock (_folderCounterLock)
            {
                if (_folderCounterInitialized) return;

                _folderCounter = 1000; // seed
                var directories = _filesystem.GetDirectories("");
                foreach (var directory in directories)
                {
                    if (long.TryParse(directory, out var folderNumber) && folderNumber > _folderCounter)
                        _folderCounter = folderNumber;
                }

                // note: not multi-domains ie LB safe as another domain could create directories
                // while we read and parse them - don't fix, move to new scheme eventually

                _folderCounterInitialized = true;
            }
        }
    }
}
