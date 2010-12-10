using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.DataLayer.Utility.Installer;
using umbraco.DataLayer;

namespace umbraco.presentation.install.utills
{
    public partial class p : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

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

                Response.Write(library.GetXmlDocumentByUrl(url).Current.OuterXml);
            }
        }


        [System.Web.Services.WebMethod]
        [System.Web.Script.Services.ScriptMethod]
        public static string installOrUpgrade()
        {
            Helper.setProgress(5, "Opening database connection...", "");

            IInstallerUtility m_Installer = BusinessLogic.Application.SqlHelper.Utility.CreateInstaller();

            // Build the new connection string
            //DbConnectionStringBuilder connectionStringBuilder = CreateConnectionString();
            Helper.setProgress( 5, "Connecting...", "");

            // Try to connect to the database
            Exception error = null;
            try
            {
                ISqlHelper sqlHelper = DataLayerHelper.CreateSqlHelper(GlobalSettings.DbDSN);
                m_Installer = sqlHelper.Utility.CreateInstaller();

                if (!m_Installer.CanConnect)
                    throw new Exception("The installer cannot connect to the database.");
                else
                    Helper.setProgress( 20, "Connection opened", "");
            }
            catch (Exception ex)
            {
                error = new Exception("Database connection initialisation failed.", ex);
                Helper.setProgress( -5, "Database connection initialisation failed.", error.Message);

                return error.Message;
            }


            if (m_Installer.CanConnect)
            {
                if (m_Installer.IsLatestVersion)
                    Helper.setProgress( 100, "Database is up-to-date", "");
                else
                {
                    if (m_Installer.IsEmpty)
                    {
                        Helper.setProgress( 35, "Installing tables...", "");
                        //do install
                        m_Installer.Install();

                        Helper.setProgress( 100, "Installation completed!", "");

                        m_Installer = null;

                        return "installed";
                    }
                    else if (m_Installer.CurrentVersion == DatabaseVersion.None || m_Installer.CanUpgrade)
                    {
                        Helper.setProgress( 35, "Updating database tables...", "");
                        m_Installer.Install();

                        Helper.setProgress( 100, "Upgrade completed!", "");

                        m_Installer = null;

                        return "upgraded";
                    }
                }
            }

            return "no connection;";
        }
    }
}