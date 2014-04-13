using System;
using System.IO;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Core.IO
{	
    public static class FileSystemExtensions
    {
		public static long GetSize(this IFileSystem fs, string path)
        {
            using (var s = fs.OpenFile(path))
            {
                var size = s.Length;
                s.Close();

                return size;    
            }
        }

        public static void CopyFile(this IFileSystem fs, string path, string newPath)
        {
            fs.AddFile(newPath, fs.OpenFile(path));
        }

        public static string GetExtension(this IFileSystem fs, string path)
		{
			return Path.GetExtension(fs.GetFullPath(path));
		}

        public static string GetFileName(this IFileSystem fs, string path)
		{
			return Path.GetFileName(fs.GetFullPath(path));
		}

        //TODO: Currently this is the only way to do this
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
    }
}
