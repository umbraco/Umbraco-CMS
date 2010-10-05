namespace dashboardUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using umbraco.BusinessLogic;

    public partial class StartupSettingsDashboard : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Hides the dashboard when checked
        ///  updates dashboard.config when checked
        /// </summary>
        protected void hideCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<Action runat='uninstall'  " +
                        "alias='addDashboardSection' " +
                        "dashboardAlias='StartupSettingsDashboardSection'>" +
                        "<section alias='StartupSettingsDashboardSection'>" +
                        "<areas>" +
                        "<area>settings</area>" +
                        "</areas>" +
                        "<tab caption='Get Started'>" +
                        "<control>/umbraco/dashboard/startupsettingsdashboard.ascx</control>" +
                        "</tab>" +
                        "</section>" +
                        "</Action>");

            XmlNode n = doc.DocumentElement;

            try
            {
                switch (hideCheckBox.Checked)
                {
                    case (true):
                        // update dashboard.config to remove dashboard entry
                        umbraco.cms.businesslogic.packager.PackageAction.UndoPackageAction("StartupSettingsDashboard", "addDashboardSection", n);
                        break;
                    default:
                        break;
                }

            }
            catch (Exception ex)
            {
                Log.Add(LogTypes.Error, 0, "Dashboard Error: " + ex.Message);
            }
        }
    }
}