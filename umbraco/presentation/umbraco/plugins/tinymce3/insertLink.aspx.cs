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

        protected void Page_Load(object sender, System.EventArgs e)
        {
            uicontrols.TabPage tp = tv_options.NewTabPage(ui.Text("content"));
            tp.HasMenu = false;
            tp.Controls.Add(pane_content);

            //tp.Controls.Add(new LiteralControl("<iframe frameborder=\"0\" src=\"../../TreeInit.aspx?app=content&isDialog=true&dialogMode=locallink&functionToCall=parent.dialogHandler&contextMenu=false\" style=\"LEFT: 0px; OVERFLOW: auto; WIDTH: 280px; POSITION: relative; TOP: 0px; HEIGHT: 220px; BACKGROUND-COLOR: white; border: none\"></iframe>"));
            
            uicontrols.TabPage tp2 = tv_options.NewTabPage(ui.Text("media"));
            tp2.HasMenu = false;
            tp2.Controls.Add(pane_media);

            //tp2.Controls.Add(new LiteralControl("<iframe frameborder=\"0\" src=\"../../TreeInit.aspx?app=media&isDialog=true&dialogMode=fulllink&functionToCall=parent.dialogHandler&contextMenu=false\" style=\"LEFT: 0px; OVERFLOW: auto; WIDTH: 280px; POSITION: relative; TOP: 0px; HEIGHT: 220px; BACKGROUND-COLOR: white; border: none\"></iframe>"));

            //umbracoLink.Controls.Add(tbv);
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
