using System;
using System.IO;
using Umbraco.Core.IO;
using Umbraco.Web.Install;
using umbraco;

namespace Umbraco.Web.UI.Install.Steps
{
    public partial class ValidatePermissions : StepUserControl
    {       
        protected void Page_Load(object sender, EventArgs e)
        {
            var permissionsOk = true;
            var packageOk = true;
            var foldersOk = true;
            var cacheOk = true;
            var valResult = "";

            // Test default dir permissions
            foreach (var dir in FilePermissionHelper.PermissionDirs)
            {
                var result = SaveAndDeleteFile(IOHelper.MapPath(dir + "/configWizardPermissionTest.txt"));

                if (!result)
                {
                    permissionsOk = false;
                    permSummary.Text += "<li>Directory: ./" + dir + "</li>";
                }

                // Print
                valResult += " " + dir + " : " + SuccessOrFailure(result) + "!<br/>";
            }

            // Test default file permissions
            foreach (var file in FilePermissionHelper.PermissionFiles)
            {
                var result = OpenFileForWrite(IOHelper.MapPath(file));
                if (!result)
                {
                    permissionsOk = false;
                    permSummary.Text += "<li>File: " + file + "</li>";
                }

                // Print
                valResult += " " + file + " : " + SuccessOrFailure(result) + "!<br/>";
            }
            permissionResults.Text = valResult;

            // Test package dir permissions
            string packageResult = "";
            foreach (var dir in FilePermissionHelper.PackagesPermissionsDirs)
            {
                var result =
                    SaveAndDeleteFile(IOHelper.MapPath(dir + "/configWizardPermissionTest.txt"));
                if (!result)
                {
                    packageOk = false;
                    permSummary.Text += "<li>Directory: " + dir + "</li>";
                }

                // Print
                packageResult += " ./" + dir + " : " + SuccessOrFailure(result) + "!<br/>";
            }
            packageResults.Text = packageResult;

            // Test umbraco.xml file
            try
            {
                content.Instance.PersistXmlToFile();
                xmlResult.Text = "Success!";
            }
            catch (Exception ee)
            {
                cacheOk = false;
                xmlResult.Text = "Failed!";
                string tempFile = SystemFiles.ContentCacheXml;

                if (tempFile.Substring(0, 1) == "/")
                    tempFile = tempFile.Substring(1, tempFile.Length - 1);

                permSummary.Text += string.Format("<li>File ./{0}<br/><strong>Error message: </strong>{1}</li>", tempFile, ee);
            }

            // Test creation of folders
            try
            {
                string tempDir = IOHelper.MapPath(SystemDirectories.Media + "/testCreatedByConfigWizard");
                Directory.CreateDirectory(tempDir);
                Directory.Delete(tempDir);
                foldersResult.Text = "Success!";
            }
            catch
            {
                foldersOk = false;
                foldersResult.Text = "Failure!";
            }

            // update config files
            if (permissionsOk)
            {
                foreach (
                    var configFile in new DirectoryInfo(IOHelper.MapPath(SystemDirectories.Config)).GetFiles("*.xml"))
                {
                    try
                    {
                        if (File.Exists(configFile.FullName.Replace(".xml", ".config")))
                            File.Delete(configFile.FullName.Replace(".xml", ".config"));

                        configFile.MoveTo(configFile.FullName.Replace(".xml", ".config"));
                    }
                    catch { }

                }
            }

            // Generate summary
            howtoResolve.Visible = true;
            if (permissionsOk && cacheOk && packageOk && foldersOk)
            {
                perfect.Visible = true;
                howtoResolve.Visible = false;
            }
            else if (permissionsOk && cacheOk && foldersOk)
                noPackages.Visible = true;
            else if (permissionsOk && cacheOk)
            {
                folderWoes.Visible = true;
                grant.Visible = false;
                noFolders.Visible = true;
            }
            else
            {
                error.Visible = true;
                if (!foldersOk)
                    folderWoes.Visible = true;
            }
        }

        private static string SuccessOrFailure(bool result)
        {
            return result ? "Success" : "Failure";
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

    }
}