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

            //tp.Controls.Add(new LiteralControl("<div style=\"padding: 5px;\"><iframe src=\"../../TreeInit.aspx?app=media&isDialog=true&dialogMode=id&contextMenu=false&functionToCall=parent.dialogHandler\" style=\"LEFT: 9px; OVERFLOW: auto; WIDTH: 200px; POSITION: relative; TOP: 0px; HEIGHT: 250px; BACKGROUND-COLOR: white\"></iframe>&nbsp;<iframe src=\"../../dialogs/imageViewer.aspx\" id=\"imageViewer\" style=\"LEFT: 9px; OVERFLOW: auto; WIDTH: 250px; POSITION: relative; TOP: 0px; HEIGHT: 250px; BACKGROUND-COLOR: white\"></iframe></div>"));
            
            uicontrols.TabPage tp2 = tv_options.NewTabPage(ui.Text("create") + " " + ui.Text("new"));
            tp2.HasMenu = false;
            tp2.Controls.Add(pane_upload);

            //tp2.Controls.Add(new LiteralControl("<iframe frameborder=\"0\" src=\"../../dialogs/uploadImage.aspx\" style=\"LEFT: 0px; OVERFLOW: auto; WIDTH: 500px; POSITION: relative; TOP: 0px; HEIGHT: 220px; BACKGROUND-COLOR: white; border: none\"></iframe>"));
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
