using System;
using System.Collections.Generic;
using System.IO;
using umbraco;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Web.Install.Models;

namespace Umbraco.Web.Install.InstallSteps
{
    [InstallSetupStep(InstallationType.NewInstall | InstallationType.Upgrade,
        "Permissions", 0, "Ensuring your file permissions are set correctly",
        PerformsAppRestart = true)]
    internal class FilePermissionsStep : InstallSetupStep<object>
    {
        public override InstallSetupResult Execute(object model)
        {
            //first validate file permissions
            var permissionsOk = true;
            var report = new List<string>();

            // Test default dir permissions
            foreach (var dir in FilePermissionHelper.PermissionDirs)
            {
                var result = SaveAndDeleteFile(IOHelper.MapPath(dir + "/configWizardPermissionTest.txt"));
                if (!result)
                {
                    permissionsOk = false;
                    report.Add("Directory: ./" + dir);
                }
            }

            // Test default file permissions
            foreach (var file in FilePermissionHelper.PermissionFiles)
            {
                var result = OpenFileForWrite(IOHelper.MapPath(file));
                if (!result)
                {
                    permissionsOk = false;
                    report.Add("File: " + file);
                }
            }

            // Test package dir permissions
            string packageResult = "";
            foreach (var dir in FilePermissionHelper.PackagesPermissionsDirs)
            {
                var result =
                    SaveAndDeleteFile(IOHelper.MapPath(dir + "/configWizardPermissionTest.txt"));
                if (!result)
                {
                    permissionsOk = false;
                    report.Add("Directory: " + dir);
                }
            }

            // Test umbraco.xml file
            try
            {
                content.Instance.PersistXmlToFile();
            }
            catch (Exception ee)
            {
                permissionsOk = false;
                string tempFile = SystemFiles.ContentCacheXml;

                if (tempFile.Substring(0, 1) == "/")
                    tempFile = tempFile.Substring(1, tempFile.Length - 1);

                report.Add(string.Format("File ./{0}. Error: {1}", tempFile, ee));
            }

            // Test creation of folders
            try
            {
                string tempDir = IOHelper.MapPath(SystemDirectories.Media + "/testCreatedByConfigWizard");
                Directory.CreateDirectory(tempDir);
                Directory.Delete(tempDir);
            }
            catch
            {
                permissionsOk = false;
                report.Add("Folder creation failed");
            }

            if (permissionsOk == false)
            {
                throw new InstallException("Permission check failed", "permissionsReport", new { errors = report });    
            }
            
            return null;
        }

        private bool OpenFileForWrite(string file)
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

        private static bool SaveAndDeleteFile(string file)
        {
            try
            {
                //first check if the directory of the file exists, and if not try to create that first.
                var fi = new FileInfo(file);
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

        public override bool RequiresExecution()
        {
            return true;
        }
    }
}