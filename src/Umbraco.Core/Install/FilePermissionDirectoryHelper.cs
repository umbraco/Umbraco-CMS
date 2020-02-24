using System;
using System.IO;
using Umbraco.Core.IO;

namespace Umbraco.Web.Install
{
    public class FilePermissionDirectoryHelper
    {


        // tries to create a file
        // if successful, the file is deleted
        // creates the directory if needed - does not delete it
        public static bool TryCreateDirectory(string dir, IIOHelper ioHelper)
        {
            try
            {
                var dirPath = ioHelper.MapPath(dir);

                if (Directory.Exists(dirPath) == false)
                    Directory.CreateDirectory(dirPath);

                var filePath = dirPath + "/" + CreateRandomFileName() + ".tmp";
                File.WriteAllText(filePath, "This is an Umbraco internal test file. It is safe to delete it.");
                File.Delete(filePath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string CreateRandomFileName()
        {
            return "umbraco-test." + Guid.NewGuid().ToString("N").Substring(0, 8);
        }
    }
}
