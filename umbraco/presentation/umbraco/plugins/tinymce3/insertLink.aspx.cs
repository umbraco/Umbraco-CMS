using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace umbraco.presentation.plugins.tinymce3
{
    public partial class insertLink : BasePages.UmbracoEnsuredPage
    {
        //protected uicontrols.TabView tbv = new uicontrols.TabView();
        public insertLink()
        {
            CurrentApp = BusinessLogic.DefaultApps.content.ToString();

        }
        protected void Page_Load(object sender, System.EventArgs e)
        {
			ClientLoader.DataBind();

            uicontrols.TabPage tp = tv_options.NewTabPage(ui.Text("content"));
            tp.HasMenu = false;
            tp.Controls.Add(pane_content);


            
            uicontrols.TabPage tp2 = tv_options.NewTabPage(ui.Text("media"));
            tp2.HasMenu = false;
            tp2.Controls.Add(pane_media);



        }

        protected override void Render(HtmlTextWriter writer)
        {
            // clear form action
            Page.Form.Attributes.Add("onsubmit", "insertAction();return false;");
//            Page.Form.Action = "#";
            // this context item is needed to prevent the urlrewriterformwriter class to change the action
  //          HttpContext.Current.Items["UrlRewriterFormWriterDisableAction"] = "true";
    //        HttpContext.Current.Items["ActionAlreadyWritten"] = "true";

            base.Render(writer);
        }

        public BusinessLogic.User GetUser()
        {
            return base.getUser();
        }

        override protected void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            //tbv.ID = "tabview1";
            //tbv.Width = 300;
            //tbv.Height = 320;
            //tbv.AutoResize = false;

            base.OnInit(e);
        }
		
    }
}
