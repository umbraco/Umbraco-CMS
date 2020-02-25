using System;
using System.IO;

namespace Umbraco.Core.IO
{
    public static class IOHelperExtensions
    {
        /// <summary>
        /// Tries to create a directory.
        /// </summary>
        /// <param name="ioHelper">The IOHelper.</param>
        /// <param name="dir">the directory path.</param>
        /// <returns>true if the directory was created, false otherwise.</returns>
        public static bool TryCreateDirectory(this IIOHelper ioHelper, string dir)
        {
            try
            {
                var dirPath = ioHelper.MapPath(dir);

                if (Directory.Exists(dirPath) == false)
                    Directory.CreateDirectory(dirPath);

                var filePath = dirPath + "/" + CreateRandomFileName(ioHelper) + ".tmp";
                File.WriteAllText(filePath, "This is an Umbraco internal test file. It is safe to delete it.");
                File.Delete(filePath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string CreateRandomFileName(this IIOHelper ioHelper)
        {
            return "umbraco-test." + Guid.NewGuid().ToString("N").Substring(0, 8);
        }
    }
}
