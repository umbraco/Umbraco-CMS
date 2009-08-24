using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace umbraco.presentation.umbraco.dialogs
{
    public partial class mediaPicker : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
           

           
        }

        protected override void OnInit(EventArgs e)
        {            
            uicontrols.TabPage tp = tv_options.NewTabPage(ui.Text("choose"));
            tp.HasMenu = false;
            tp.Controls.Add(pane_select);

            uicontrols.TabPage tp2 = tv_options.NewTabPage(ui.Text("create") + " " + ui.Text("new"));
            tp2.HasMenu = false;
            tp2.Controls.Add(pane_upload);

            base.OnInit(e);
        }
    }
}
