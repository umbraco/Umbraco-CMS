using System;
using System.IO;
using System.Web.UI;
using umbraco.IO;

namespace umbraco.presentation.install.steps
{
    /// <summary>
    ///		Summary description for validatePermissions.
    /// </summary>
    public partial class validatePermissions : UserControl
    {
        private string[] permissionDirs = {SystemDirectories.Css, SystemDirectories.Config, SystemDirectories.Data, SystemDirectories.Media, SystemDirectories.Masterpages, SystemDirectories.Xslt, SystemDirectories.Usercontrols, SystemDirectories.Preview};
        private string[] permissionFiles = { };
        private string[] packagesPermissionsDirs = {SystemDirectories.Bin, SystemDirectories.Umbraco, SystemDirectories.Usercontrols, SystemDirectories.Packages};

        protected void Page_Load(object sender, EventArgs e)
        {
            bool permissionsOK = true;
            bool packageOK = true;
            bool foldersOK = true;
            bool cacheOK = true;
            string valResult = "";

            // Test default dir permissions
            foreach (string dir in permissionDirs)
            {
                bool result = SaveAndDeleteFile( IOHelper.MapPath( dir + "/configWizardPermissionTest.txt") );
                
                if (!result)
                {
                    permissionsOK = false;
                    permSummary.Text += "<li>Directory: ./" + dir + "</li>";
                }

                // Print
                valResult += " " + dir + " : " + successOrFailure(result) + "!<br/>";
            }

            // Test default file permissions
            foreach (string file in permissionFiles)
            {
                bool result = OpenFileForWrite( IOHelper.MapPath(file) );
                if (!result)
                {
                    permissionsOK = false;
                    permSummary.Text += "<li>File: " + file + "</li>";
                }

                // Print
                valResult += " " + file + " : " + successOrFailure(result) + "!<br/>";
            }
            permissionResults.Text = valResult;

            // Test package dir permissions
            string packageResult = "";
            foreach (string dir in packagesPermissionsDirs)
            {
                bool result =
                    SaveAndDeleteFile( IOHelper.MapPath(dir + "/configWizardPermissionTest.txt"));
                if (!result)
                {
                    packageOK = false;
                    permSummary.Text += "<li>Directory: " + dir + "</li>";
                }

                // Print
                packageResult += " ./" + dir + " : " + successOrFailure(result) + "!<br/>";
            }
            packageResults.Text = packageResult;

            // Test umbraco.xml file
            try
            {
                content.Instance.SaveContentToDisk(content.Instance.XmlContent);
                xmlResult.Text = "Success!";
            }
            catch (Exception ee)
            {
                cacheOK = false;
                xmlResult.Text = "Failed!";
                string tempFile = SystemFiles.ContentCacheXml;

                if (tempFile.Substring(0, 1) == "/")
                    tempFile = tempFile.Substring(1, tempFile.Length - 1);

                permSummary.Text += string.Format("<li>File ./{0}<br/><strong>Error message: </strong>{1}</li>", tempFile, ee);
            }

            // Test creation of folders
            try
            {
                string tempDir = IOHelper.MapPath( SystemDirectories.Media + "/testCreatedByConfigWizard" );
                Directory.CreateDirectory(tempDir);
                Directory.Delete(tempDir);
                foldersResult.Text = "Success!";
            }
            catch
            {
                foldersOK = false;
                foldersResult.Text = "Failure!";
            }

            // update config files
            if (permissionsOK)
            {
                foreach (
                    FileInfo configFile in  new DirectoryInfo( IOHelper.MapPath( SystemDirectories.Config )).GetFiles("*.xml"))
                {
                    try {
                        if (File.Exists(configFile.FullName.Replace(".xml", ".config")))
                            File.Delete(configFile.FullName.Replace(".xml", ".config"));

                        configFile.MoveTo(configFile.FullName.Replace(".xml", ".config"));
                    }catch{}
                    
                }
            }

            // Generate summary
            howtoResolve.Visible = true;
            if (permissionsOK && cacheOK && packageOK && foldersOK)
            {
                perfect.Visible = true;
                howtoResolve.Visible = false;
            }
            else if (permissionsOK && cacheOK && foldersOK)
                noPackages.Visible = true;
            else if (permissionsOK && cacheOK)
            {
                folderWoes.Visible = true;
                grant.Visible = false;
                noFolders.Visible = true;
            }
            else
            {
                error.Visible = true;
                if (!foldersOK)
                    folderWoes.Visible = true;
            }
        }

        private string successOrFailure(bool result)
        {
            if (result)
                return "Success";
            else
                return "Failure";
        }

        private bool SaveAndDeleteFile(string file)
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

        #region Web Form Designer generated code

        protected override void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        ///		Required method for Designer support - do not modify
        ///		the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        }

        #endregion
    }
}