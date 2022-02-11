using System.IO;

namespace Umbraco.Cms.Core.IO
{
    public static class FileSystemUtility
    {
        private static bool? s_isCaseSensitiveFileSystem;

        /// <summary>
        /// Check to see if the system is a case sensitive filesytem
        /// </summary>
        public static bool IsCaseSensitiveFileSystem()
        {
            if (!s_isCaseSensitiveFileSystem.HasValue)
            {
                var tmp = Path.GetTempPath();

                s_isCaseSensitiveFileSystem = !Directory.Exists(tmp.ToUpper()) || !Directory.Exists(tmp.ToLower());
            }

            return s_isCaseSensitiveFileSystem.Value;
        }
    }
}
