using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Umbraco.Core.IO;

namespace Umbraco.Core.Media
{
    /// <summary>
    /// Internal singleton to handle the numbering of subfolders within the Media-folder.
    /// When this class is initiated it will look for numbered subfolders and select the highest number,
    /// which will be the start point for the naming of the next subfolders. If no subfolders exists
    /// then the starting point will be 1000, ie. /media/1000/koala.jpg
    /// </summary>
    internal class MediaSubfolderCounter
    {
        #region Singleton

        private long _numberedFolder = 1000;//Default starting point
        private static readonly ReaderWriterLockSlim ClearLock = new ReaderWriterLockSlim();
        private static readonly Lazy<MediaSubfolderCounter> Lazy = new Lazy<MediaSubfolderCounter>(() => new MediaSubfolderCounter());

        public static MediaSubfolderCounter Current { get { return Lazy.Value; } }

        private MediaSubfolderCounter()
        {
            var folders = new List<long>();
            var fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
            var directories = fs.GetDirectories("");
            foreach (var directory in directories)
            {
                long dirNum;
                if (long.TryParse(directory, out dirNum))
                {
                    folders.Add(dirNum);
                }
            }
            var last = folders.OrderBy(x => x).LastOrDefault();
            if(last != default(long))
                _numberedFolder = last;
        }

        #endregion

        /// <summary>
        /// Returns an increment of the numbered media subfolders.
        /// </summary>
        /// <returns>A <see cref="System.Int64"/> value</returns>
        public long Increment()
        {
            using (new ReadLock(ClearLock))
            {
                _numberedFolder = _numberedFolder + 1;
                return _numberedFolder;
            }
        }
    }
}