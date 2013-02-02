using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace umbraco.presentation.install.utills
{
    [Obsolete("This class is no longer used and will be removed in future version. The page Umbraco.Web.UI.Install.DatabaseRestService supercedes this.")]
    public partial class p : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            LogHelper.Info<p>(string.Format("Hitting Page_Load on InstallerRestService.aspx for the requested '{0}' feed", Request.QueryString["feed"]));

            // Stop Caching in IE
            Response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);

            // Stop Caching in Firefox
            Response.Cache.SetNoStore();

            string feed = Request.QueryString["feed"];
            string url = "http://our.umbraco.org/html/twitter";

            if (feed == "progress")
            {
                Response.ContentType = "application/json";
                Response.Write(Helper.getProgress());
            }
            else
            {
                if (feed == "blogs")
                    url = "http://our.umbraco.org/html/blogs";

                if (feed == "sitebuildervids")
                    url = "http://umbraco.org/feeds/videos/site-builder-foundation-html";

                if (feed == "developervids")
                    url = "http://umbraco.org/feeds/videos/developer-foundation-html";

                string XmlResponse = library.GetXmlDocumentByUrl(url).Current.OuterXml;

                if (!XmlResponse.Contains("System.Net.WebException"))
                {
                    Response.Write(library.GetXmlDocumentByUrl(url).Current.OuterXml);
                }
                else
                {
                    Response.Write("We can't connect to umbraco.tv right now.  Click <strong>Set up your new website</strong> above to continue.");
                }
            }
        }


        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static string installOrUpgrade()
        {
            LogHelper.Info<p>("Running 'installOrUpgrade' service");

            var result = ApplicationContext.Current.DatabaseContext.CreateDatabaseSchemaAndDataOrUpgrade();

            // Remove legacy umbracoDbDsn configuration setting if it exists and connectionstring also exists
            if (ConfigurationManager.ConnectionStrings[Umbraco.Core.Configuration.GlobalSettings.UmbracoConnectionName] != null)
            {
                Umbraco.Core.Configuration.GlobalSettings.RemoveSetting(Umbraco.Core.Configuration.GlobalSettings.UmbracoConnectionName);
            }
            else
            {
                var ex = new ArgumentNullException(string.Format("ConfigurationManager.ConnectionStrings[{0}]", Umbraco.Core.Configuration.GlobalSettings.UmbracoConnectionName), "Install / upgrade did not complete successfully, umbracoDbDSN was not set in the connectionStrings section");
                LogHelper.Error<p>("", ex);
                throw ex;
            }

            var js = new JavaScriptSerializer();
            var jsonResult = js.Serialize(result);
            return jsonResult;
        }
    }

    [Obsolete("This is no longer used and will be removed from the codebase in the future. Has been superceded by Umbraco.Web.Install.InstallHelper but this is marked internal it is not to be used in external code.")]
    public static class Helper
    {

        public static void RedirectToNextStep(Page page)
        {
            _default d = (_default)page;
            d.GotoNextStep(d.step.Value);
        }

        public static void RedirectToLastStep(Page page)
        {
            _default d = (_default)page;
            d.GotoLastStep();
        }


        private static int m_Percentage = -1;
        public static int Percentage { get { return m_Percentage; } set { m_Percentage = value; } }

        public static string Description { get; set; }
        public static string Error { get; set; }


        public static void clearProgress()
        {
            Percentage = -1;
            Description = string.Empty;
            Error = string.Empty;
        }

        public static void setProgress(int percent, string description, string error)
        {
            if (percent > 0)
                Percentage = percent;

            Description = description;
            Error = error;
        }

        public static string getProgress()
        {
            ProgressResult pr = new ProgressResult(Percentage, Description, Error);
            JavaScriptSerializer js = new JavaScriptSerializer();
            return js.Serialize(pr);
        }
    }

    [Obsolete("This class is no longer used and will be removed from the codebase in future versions. It is superceded by Umbraco.Web.Install.ProgressResult but is internal and not to be used in external code.")]
    public class ProgressResult
    {
        public string Error { get; set; }
        public int Percentage { get; set; }
        public string Description { get; set; }
        public ProgressResult()
        {

        }

        public ProgressResult(int percentage, string description, string error)
        {
            Percentage = percentage;
            Description = description;
            Error = error;
        }

    }

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