using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using Umbraco.Core.IO;
using umbraco;

namespace Umbraco.Web.Install
{
    internal class FilePermissionHelper
    {
        internal static readonly string[] PermissionDirs = { SystemDirectories.Css, SystemDirectories.Config, SystemDirectories.Data, SystemDirectories.Media, SystemDirectories.Masterpages, SystemDirectories.Xslt, SystemDirectories.UserControls, SystemDirectories.Preview };
        internal static readonly string[] PermissionFiles = { };
        internal static readonly string[] PackagesPermissionsDirs = { SystemDirectories.Bin, SystemDirectories.Umbraco, SystemDirectories.UserControls, SystemDirectories.Packages };

        public static bool RunFilePermissionTestSuite()
        {
            var newReport = new Dictionary<string, string>();

            if (!TestDirectories(PermissionDirs, ref newReport))
                return false;

            if (!TestDirectories(PackagesPermissionsDirs, ref newReport))
                return false;

            if (!TestFiles(PermissionFiles, ref newReport))
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
            foreach (string dir in PermissionDirs)
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
            foreach (string file in PermissionFiles)
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