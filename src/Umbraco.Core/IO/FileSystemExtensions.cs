using System;
using System.IO;

namespace Umbraco.Core.IO
{	
    public static class FileSystemExtensions
    {
		public static long GetSize(this IFileSystem fs, string path)
        {
			 using (var ms = new MemoryStream())
			 {
				 using (var sr = new StreamReader(ms))
				 {
					 using (Stream file = fs.OpenFile(path))
					 {
						 var bytes = new byte[file.Length];
						 file.Read(bytes, 0, (int) file.Length);
						 ms.Write(bytes, 0, (int) file.Length);
						 file.Close();
						 ms.Position = 0;
						 string str = sr.ReadToEnd();
						 return str.Length;
					 }
				 }
			 }
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
