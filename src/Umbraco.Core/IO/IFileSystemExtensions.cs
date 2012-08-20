using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Core.IO
{
    public static class IFileSystemExtensions
    {
        internal static long GetSize(this IFileSystem fs, string path)
        {
            var s = fs.OpenFile(path);
            var size = s.Length;
            s.Close();

            return size;
        }

        internal static void CopyFile(string path, string newPath)
        {
            
        }
    }
}
