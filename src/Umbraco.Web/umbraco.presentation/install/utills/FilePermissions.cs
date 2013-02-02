using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using Umbraco.Core.IO;

namespace umbraco.presentation.install.utills
{
    [Obsolete("This is no longer used and will be removed from the codebase in the future. Has been superceded by Umbraco.Web.Install.FilePermissionHelper but this is marked internal it is not to be used in external code.")]
    public class FilePermissions
    {
        public static string[] permissionDirs = { SystemDirectories.Css, SystemDirectories.Config, SystemDirectories.Data, SystemDirectories.Media, SystemDirectories.Masterpages, SystemDirectories.Xslt, SystemDirectories.UserControls, SystemDirectories.Preview };
        public static string[] permissionFiles = { };
        public static string[] packagesPermissionsDirs = { SystemDirectories.Bin, SystemDirectories.Umbraco, SystemDirectories.UserControls, SystemDirectories.Packages };

        public static bool RunFilePermissionTestSuite()
        {
            var newReport = new Dictionary<string, string>();

            if (!TestDirectories(permissionDirs, ref newReport))
                return false;

            if (!TestDirectories(packagesPermissionsDirs, ref newReport))
                return false;

            if (!TestFiles(permissionFiles, ref newReport))
                return false;

            if (!TestContentXml(ref newReport)) 
                return false;

            if (!TestFolderCreation(SystemDirectories.Media, ref newReport))
                return false;

            return true;
        }

        public static bool TestDirectories(string[] directories, ref Dictionary<string, string> errorReport)
        {
            bool succes = true;
            foreach (string dir in permissionDirs)
            {
                bool result = SaveAndDeleteFile(IOHelper.MapPath(dir + "/configWizardPermissionTest.txt"));

                if (!result)
                {
                    succes = false;

                    if (errorReport != null)
                        errorReport.Add(dir, "Missing permissions, cannot create new files");
                }
            }

            return succes;
        }

        public static bool TestFiles(string[] files, ref Dictionary<string,string> errorReport)
        {
            bool succes = true;
            foreach (string file in permissionFiles)
            {
                bool result = OpenFileForWrite(IOHelper.MapPath(file));
                if (!result)
                {
                    if (errorReport != null)
                        errorReport.Add(file, "Missing write permissions");

                    succes = false;
                }
            }

            return succes;
        }

        public static bool TestFolderCreation(string folder, ref Dictionary<string,string> errorReport)
        {
            try
            {
                string tempDir = IOHelper.MapPath(folder + "/testCreatedByConfigWizard");
                Directory.CreateDirectory(tempDir);
                Directory.Delete(tempDir);
                return true;
            }
            catch
            {
                if (errorReport != null)
                    errorReport.Add(folder, "Could not create sub-directory");
                return false;
            }
        }

        public static bool TestContentXml(ref Dictionary<string, string> errorReport)
        {
            // Test umbraco.xml file
            try
            {
                content.Instance.PersistXmlToFile();
                return true;
            }
            catch
            {
                if(errorReport != null)
                    errorReport.Add(SystemFiles.ContentCacheXml, "Could not persist content cache");
                return false;    
            }
        }

        private static bool SaveAndDeleteFile(string file)
        {
            try
            {
                //first check if the directory of the file exists, and if not try to create that first.
                FileInfo fi = new FileInfo(file);
                if (!fi.Directory.Exists)
                {
                    fi.Directory.Create();
                }

                File.WriteAllText(file,
                                  "This file has been created by the umbraco configuration wizard. It is safe to delete it!");
                File.Delete(file);
                return true;
            }
            catch
            {
                return false;
            }

        }

        private static bool OpenFileForWrite(string file)
        {
            try
            {
                File.AppendText(file).Close();
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}