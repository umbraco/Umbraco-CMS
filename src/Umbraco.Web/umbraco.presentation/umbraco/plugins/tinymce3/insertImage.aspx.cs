using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.BusinessLogic;
using umbraco.businesslogic.Exceptions;

namespace umbraco.presentation.plugins.tinymce3
{
    public partial class insertImage : BasePages.UmbracoEnsuredPage
    {
        protected uicontrols.TabView tbv = new uicontrols.TabView();


        protected void Page_Load(object sender, EventArgs e)
        {
			ClientLoader.DataBind();

            pp_src.Text = ui.Text("url");
            pp_title.Text = ui.Text("name");
            pp_dimensions.Text = ui.Text("dimensions");

            pane_src.Style.Add("height", "105px");
            
            lt_heightLabel.Text = ui.Text("height");
            lt_widthLabel.Text = ui.Text("width");

            Title = ui.Text("insertimage");

            // Put user code to initialize the page here 
            var tp = tv_options.NewTabPage(ui.Text("choose"));
            tp.HasMenu = false;
            tp.Controls.Add(pane_select);
          
            var tp2 = tv_options.NewTabPage(ui.Text("create") + " " + ui.Text("new"));
            tp2.HasMenu = false;
            tp2.Controls.Add(pane_upload);
    }

        
        protected override void OnInit(EventArgs e)
        {
            tbv.ID = "tabview1";
            tbv.AutoResize = false;
            tbv.Width = 500;
            tbv.Height = 290;

            //this could be used for media or content so we need to at least validate that the user has access to one or the other
            if (!ValidateUserApp(DefaultApps.content.ToString()) && !ValidateUserApp(DefaultApps.media.ToString()))
                throw new UserAuthorizationException("The current user doesn't have access to the section/app");

            base.OnInit(e);
        }

    }
}
