namespace umbraco.presentation.install.steps
{
    using System;
    using System.Data;
    using System.Drawing;
    using System.Web;
    using System.Web.UI.WebControls;
    using System.Web.UI.HtmlControls;

    /// <summary>
    ///		Summary description for theend.
    /// </summary>
    public partial class theend : System.Web.UI.UserControl
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {
//            BasePages.BasePage.doLogin(BusinessLogic.User.GetUser(0));
            
            // hide next button
            Page.FindControl("next").Visible = false;

            // Update configurationStatus
            try
            {
                GlobalSettings.ConfigurationStatus = GlobalSettings.CurrentVersion;
                Application["umbracoNeedConfiguration"] = false;
            }
            catch (Exception ex)
            {
                updateUmbracoSettingsFailed.Visible = true;
                errorLiteral.Text = ex.ToString();
            }

            if (cms.businesslogic.packager.InstalledPackage.isPackageInstalled("ae41aad0-1c30-11dd-bd0b-0800200c9a66")) {
                viewSite.Visible = true;
            }


        }

        #region Web Form Designer generated code
        override protected void OnInit(EventArgs e)
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
