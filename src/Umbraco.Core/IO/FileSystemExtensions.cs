using System.IO;
using Umbraco.Core.CodeAnnotations;

namespace Umbraco.Core.IO
{
	[UmbracoExperimentalFeature("http://issues.umbraco.org/issue/U4-1156", "Will be declared public after 4.10")]
    public static class FileSystemExtensions
    {
		[UmbracoExperimentalFeature("http://issues.umbraco.org/issue/U4-1156", "Will be declared public after 4.10")]
		internal static long GetSize(this IFileSystem fs, string path)
        {
            using (var s = fs.OpenFile(path))
            {
                var size = s.Length;
                s.Close();

                return size;    
            }
        }

		[UmbracoExperimentalFeature("http://issues.umbraco.org/issue/U4-1156", "Will be declared public after 4.10")]
		internal static void CopyFile(this IFileSystem fs, string path, string newPath)
        {
            fs.AddFile(newPath, fs.OpenFile(path));
        }

		[UmbracoExperimentalFeature("http://issues.umbraco.org/issue/U4-1156", "Will be declared public after 4.10")]
		internal static string GetExtension(this IFileSystem fs, string path)
		{
			return Path.GetExtension(fs.GetFullPath(path));
		}

		[UmbracoExperimentalFeature("http://issues.umbraco.org/issue/U4-1156", "Will be declared public after 4.10")]
		internal static string GetFileName(this IFileSystem fs, string path)
		{
			return Path.GetFileName(fs.GetFullPath(path));
		}
    }
}
