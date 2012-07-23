using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClientDependency.Core;
using umbraco.BasePages;
using umbraco.cms.businesslogic.media;

namespace umbraco.uicontrols.FolderBrowser
{
    [ClientDependency(ClientDependencyType.Css, "FolderBrowser/css/folderbrowser.css", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "ui/jquery.js", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "ui/base2.js", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "ui/knockout.js", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "FolderBrowser/js/folderbrowser.js", "UmbracoClient")]
    [ToolboxData("<{0}:FolderBrowser runat=server></{0}:FolderBrowser>")]
    public class FolderBrowser : WebControl
    {
        protected Panel panel;

        protected int ParentId
        {
            get
            {
                // Try and parse from querystring
                if(!string.IsNullOrEmpty(Context.Request.QueryString["id"]))
                {
                    int id;
                    if(Int32.TryParse(Context.Request.QueryString["id"], out id))
                        return id;
                }

                // Get users root media folder id
                var currentUser = UmbracoEnsuredPage.CurrentUser;
                if (currentUser != null)
                    return currentUser.StartMediaId;

                // Nothing else to check so just return -1
                return -1;
            }
        }

        protected Media ParentNode
        {
            get
            {
                return new Media(ParentId);
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            EnsureChildControls();

            //disable view state for this control
            this.EnableViewState = false;
        }

        /// <summary>
        /// Create the native .net child controls for this control
        /// </summary>
        protected override void CreateChildControls()
        {
            // Create the panel surround
            panel = new Panel
            {
                ID = "FolderBrowser", 
                CssClass = "umbFolderBrowser"
            };

            panel.Attributes.Add("data-parentid", ParentId.ToString());

            var sb = new StringBuilder();

            // Create the breadcrumb
            var breadCrumb = new List<Media>();
            breadCrumb.Add(ParentNode);

            var parent = ParentNode;
            while(parent.Id != -1)
            {
                parent = new Media(parent.ParentId);
                breadCrumb.Add(parent);
            }

            breadCrumb.Reverse();

            sb.Append("<ul class='breadcrumb'><li><strong>You are here:</strong></li>");
            foreach (var media in breadCrumb)
            {
                if(media.Id == ParentId)
                    if (media.Id == -1)
                        sb.AppendFormat("<li>Media</li>");
                    else
                        sb.AppendFormat("<li>{0}</li>", media.Text);
                else
                    if (media.Id == -1)
                        sb.AppendFormat("<li><a href='dashboard.aspx?app=media'>Media</a></li>");
                    else
                        sb.AppendFormat("<li><a href='editMedia.aspx?id={1}'>{0}</a></li>", media.Text, media.Id);
            }
            sb.Append("</ul>");

            // Create thumbnails container
            sb.Append("<ul class='items' data-bind='foreach: items'>" +
                      "<li><a href='#'><img src='' /><span data-bind='text: name'></span></a></li>" +
                      "</ul>");

            panel.Controls.Add(new LiteralControl(sb.ToString()));

            Controls.Add(panel);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            this.RenderContents(writer);
        }
    }
}
