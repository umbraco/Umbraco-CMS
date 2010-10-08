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

    public partial class StartupDashboard : System.Web.UI.UserControl
    {
        /// <summary>
        /// Check to see if Runway is installed
        ///  if so, hide the appropriate panel in the UI
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                var templates = umbraco.cms.businesslogic.template.Template.GetAllAsList();
                foreach (var t in templates)
                {
                    if (t.Alias == "RunwayMaster")
                    {
                        this.skinPanel.Visible = false;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Add(LogTypes.Error, 0, "Dashboard Error: " + ex.Message);
            }
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
                "dashboardAlias='StartupDashboardSection'>" +
                "</Action>");

                XmlNode n = doc.DocumentElement;

                try
                {
                    switch (hideCheckBox.Checked)
                    {
                        case (true):
                            // update dashboard.config to remove dashboard entry
                            umbraco.cms.businesslogic.packager.PackageAction.UndoPackageAction("StartupDashboard", "addDashboardSection", n);
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