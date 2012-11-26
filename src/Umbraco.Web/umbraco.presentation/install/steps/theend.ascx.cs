using System.IO;
using Umbraco.Core.IO;
using umbraco.BusinessLogic;

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
            // Update configurationStatus
            try
            {

                GlobalSettings.ConfigurationStatus = Umbraco.Core.Configuration.GlobalSettings.Version.ToString(3);
                Application["umbracoNeedConfiguration"] = false;
            }
            catch (Exception)
            {
                //errorLiteral.Text = ex.ToString();
            }

            if (!cms.businesslogic.skinning.Skinning.IsStarterKitInstalled())
                customizeSite.Visible = false;

            var tempFolder = IOHelper.MapPath("~/App_Data/TEMP/PluginCache");
            if(Directory.Exists(tempFolder))
                Directory.Delete(tempFolder, true);
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
