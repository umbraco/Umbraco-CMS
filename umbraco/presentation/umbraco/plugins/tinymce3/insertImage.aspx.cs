using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace umbraco.presentation.plugins.tinymce3
{
    public partial class insertImage : BasePages.UmbracoEnsuredPage
    {
        protected uicontrols.TabView tbv = new uicontrols.TabView();


        protected void Page_Load(object sender, System.EventArgs e)
        {
			ClientLoader.DataBind();

            pp_src.Text = ui.Text("url");
            pp_title.Text = ui.Text("name");
            pp_dimensions.Text = ui.Text("dimensions");

            lt_constrainLabel.Text = ui.Text("constrainProportions");
            lt_heightLabel.Text = ui.Text("height");
            lt_widthLabel.Text = ui.Text("width");

            Title = ui.Text("insertimage");

            // Put user code to initialize the page here 
            uicontrols.TabPage tp = tv_options.NewTabPage(ui.Text("choose"));
            tp.HasMenu = false;
            tp.Controls.Add(pane_select);
          
            uicontrols.TabPage tp2 = tv_options.NewTabPage(ui.Text("create") + " " + ui.Text("new"));
            tp2.HasMenu = false;
            tp2.Controls.Add(pane_upload);
    }

        #region Web Form Designer generated code
        override protected void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            tbv.ID = "tabview1";
            tbv.AutoResize = false;
            tbv.Width = 500;
            tbv.Height = 290;

            InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {

        }
        #endregion
    }
}
