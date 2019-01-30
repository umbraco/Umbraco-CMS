﻿using System;
using System.IO;
using System.Threading;

namespace Umbraco.Core.IO
{
    public static class FileSystemExtensions
    {
        /// <summary>
        /// Attempts to open the file at <code>filePath</code> up to <code>maxRetries</code> times,
        /// with a thread sleep time of <code>sleepPerRetryInMilliseconds</code> between retries.
        /// </summary>
        public static FileStream OpenReadWithRetry(this FileInfo file, int maxRetries = 5, int sleepPerRetryInMilliseconds = 50)
        {
            var retries = maxRetries;

            while (retries > 0)
            {
                try
                {
                    return File.OpenRead(file.FullName);
                }
                catch(IOException)
                {
                    retries--;

                    if (retries == 0)
                    {
                        throw;
                    }

                    Thread.Sleep(sleepPerRetryInMilliseconds);
                }
            }

            throw new ArgumentException("Retries must be greater than zero");
        }

        public static void CopyFile(this IFileSystem fs, string path, string newPath)
        {
            using (var stream = fs.OpenFile(path))
            {
                fs.AddFile(newPath, stream);
            }
        }

        public static string GetExtension(this IFileSystem fs, string path)
        {
            return Path.GetExtension(fs.GetFullPath(path));
        }

        public static string GetFileName(this IFileSystem fs, string path)
        {
            return Path.GetFileName(fs.GetFullPath(path));
        }

        // TODO: Currently this is the only way to do this
        internal static void CreateFolder(this IFileSystem fs, string folderPath)
        {
            var path = fs.GetRelativePath(folderPath);
            var tempFile = Path.Combine(path, Guid.NewGuid().ToString("N") + ".tmp");
            using (var s = new MemoryStream())
            {
                fs.AddFile(tempFile, s);
            }
            fs.DeleteFile(tempFile);
        }

        /// <summary>
        /// Unwraps a filesystem.
        /// </summary>
        /// <remarks>
        /// <para>A filesystem can be wrapped in a <see cref="FileSystemWrapper"/> (public) or a <see cref="ShadowWrapper"/> (internal),
        /// and this method deals with the various wrappers and </para>
        /// </remarks>
        public static IFileSystem Unwrap(this IFileSystem filesystem)
        {
            var unwrapping = true;
            while (unwrapping)
            {
                switch (filesystem)
                {
                    case FileSystemWrapper wrapper:
                        filesystem = wrapper.InnerFileSystem;
                        break;
                    case ShadowWrapper shadow:
                        filesystem = shadow.InnerFileSystem;
                        break;
                    default:
                        unwrapping = false;
                        break;
                }
            }
            return filesystem;
        }
    }
}
