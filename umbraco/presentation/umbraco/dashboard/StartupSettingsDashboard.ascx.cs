namespace dashboardUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
            switch (hideCheckBox.Checked)
            {
                case (true):
                    // update dashboard.config to remove dashboard entry
                    break;
                case (false):
                    // update dashboard.config to add dashboard entry, should never be hit
                    break;
                default:
                    break;
            }
        }
    }
}