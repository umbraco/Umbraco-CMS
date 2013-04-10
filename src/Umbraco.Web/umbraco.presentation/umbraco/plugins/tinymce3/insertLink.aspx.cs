using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core;
using umbraco.BusinessLogic;
using umbraco.businesslogic.Exceptions;

namespace umbraco.presentation.plugins.tinymce3
{
    public partial class insertLink : BasePages.UmbracoEnsuredPage
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            //this could be used for media or content so we need to at least validate that the user has access to one or the other
            if (!ValidateUserApp(DefaultApps.content.ToString()) && !ValidateUserApp(DefaultApps.media.ToString()))
                throw new UserAuthorizationException("The current user doesn't have access to the section/app");
        }

        protected void Page_Load(object sender, System.EventArgs e)
        {
            ClientLoader.DataBind();

            uicontrols.TabPage tp = tv_options.NewTabPage(ui.Text("content"));
            tp.HasMenu = false;
            tp.Controls.Add(pane_content);


            if (CurrentUser.GetApplications().Find(t => t.alias == Constants.Applications.Media) != null)
            {
                uicontrols.TabPage tp2 = tv_options.NewTabPage(ui.Text("media"));
                tp2.HasMenu = false;
                tp2.Controls.Add(pane_media);
            } else
            {
                pane_media.Visible = false;
            }


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

        public User GetUser()
        {
            return base.getUser();
        }


    }
}
