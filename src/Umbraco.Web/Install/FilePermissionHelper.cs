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

        public static bool RunFilePermissionTestSuite(out Dictionary<string, List<string>> errorReport)
        {
            errorReport = new Dictionary<string, List<string>>();

            List<string> errors;

            if (TestDirectories(PermissionDirs, out errors) == false)
                errorReport["Folder creation failed"] = errors.ToList();

            if (TestDirectories(PackagesPermissionsDirs, out errors) == false)
                errorReport["File writing for packages failed"] = errors.ToList();

            if (TestFiles(PermissionFiles, out errors) == false)
                errorReport["File writing failed"] = errors.ToList();

            if (TestContentXml(out errors) == false)
                errorReport["Cache file writing failed"] = errors.ToList();

            if (TestFolderCreation(SystemDirectories.Media, out errors) == false)
                errorReport["Media folder creation failed"] = errors.ToList();

            return errorReport.Any() == false;
        }

        public static bool TestDirectories(string[] directories, out List<string> errorReport)
        {
            errorReport = new List<string>();
            bool succes = true;
            foreach (string dir in PermissionDirs)
            {
                bool result = SaveAndDeleteFile(IOHelper.MapPath(dir + "/configWizardPermissionTest.txt"));

                if (result == false)
                {
                    succes = false;
                    errorReport.Add(dir);
                }
            }

            return succes;
        }

        public static bool TestFiles(string[] files, out List<string> errorReport)
        {
            errorReport = new List<string>();
            bool succes = true;
            foreach (string file in PermissionFiles)
            {
                bool result = OpenFileForWrite(IOHelper.MapPath(file));
                if (result == false)
                {
                    errorReport.Add(file);
                    succes = false;
                }
            }

            return succes;
        }

        public static bool TestFolderCreation(string folder, out List<string> errorReport)
        {
            errorReport = new List<string>();
            try
            {
                string tempDir = IOHelper.MapPath(folder + "/testCreatedByConfigWizard");
                Directory.CreateDirectory(tempDir);
                Directory.Delete(tempDir);
                return true;
            }
            catch
            {
                errorReport.Add(folder);
                return false;
            }
        }

        public static bool TestContentXml(out List<string> errorReport)
        {
            errorReport = new List<string>();
            // Test creating/saving/deleting a file in the same location as the content xml file
            // NOTE: We cannot modify the xml file directly because a background thread is responsible for 
            // that and we might get lock issues.
            try
            {
                var xmlFile = content.Instance.UmbracoXmlDiskCacheFileName + ".tmp";
                SaveAndDeleteFile(xmlFile);
                return true;
            }
            catch
            {
                errorReport.Add(SystemFiles.ContentCacheXml);
                return false;    
            }
        }

        private static bool SaveAndDeleteFile(string file)
        {
            try
            {
                //first check if the directory of the file exists, and if not try to create that first.
                FileInfo fi = new FileInfo(file);
                if (fi.Directory.Exists == false)
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