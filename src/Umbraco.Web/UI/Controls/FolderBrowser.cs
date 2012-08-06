using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClientDependency.Core;
using umbraco.BasePages;
using umbraco.IO;
using umbraco.cms.businesslogic.media;

namespace Umbraco.Web.UI.Controls
{
    [ClientDependency(ClientDependencyType.Css, "ContextMenu/Css/jquery.contextMenu.css", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Css, "FolderBrowser/Css/folderbrowser.css", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "ui/jquery.js", "UmbracoClient", Priority = 1)]
    [ClientDependency(ClientDependencyType.Javascript, "ui/base2.js", "UmbracoClient", Priority = 1)]
    [ClientDependency(ClientDependencyType.Javascript, "ui/knockout.js", "UmbracoClient", Priority = 2)]
    [ClientDependency(ClientDependencyType.Javascript, "ui/knockout.mapping.js", "UmbracoClient", Priority = 3)]
    [ClientDependency(ClientDependencyType.Javascript, "ContextMenu/Js/jquery.contextMenu.js", "UmbracoClient", Priority = 3)]
    [ClientDependency(ClientDependencyType.Javascript, "FileUploader/js/jquery.fileUploader.js", "UmbracoClient", Priority = 4)]
    [ClientDependency(ClientDependencyType.Javascript, "FolderBrowser/js/folderbrowser.js", "UmbracoClient", Priority = 10)]
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

        protected global::umbraco.cms.businesslogic.media.Media ParentNode
        {
            get
            {
                return new global::umbraco.cms.businesslogic.media.Media(ParentId);
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
            var breadCrumb = new List<global::umbraco.cms.businesslogic.media.Media>();
            breadCrumb.Add(ParentNode);

            var parent = ParentNode;
            while(parent.Id != -1)
            {
                parent = new global::umbraco.cms.businesslogic.media.Media(parent.ParentId);
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

            // Create the filter input
            sb.Append("<div class='filter'>Filter: <input type='text' data-bind=\"value: filterTerm, valueUpdate: 'afterkeydown'\" /></div>");

            // Create thumbnails container
            sb.Append("<ul class='items' data-bind='foreach: items'>" +
                      "<li data-bind=\"attr: { 'data-id': Id, 'data-order': $index() }, css: { selected: selected() }, event: { mousedown: toggleSelected }\"><div><span class='img'><img data-bind='attr: { src: ThumbnailUrl }' /></span><span data-bind='text: Name'></span></div></li>" +
                      "</ul>");

            panel.Controls.Add(new LiteralControl(sb.ToString()));

            Controls.Add(panel);

            Page.ClientScript.RegisterStartupScript(typeof(FolderBrowser),
                "RegisterFolderBrowsers",
                string.Format("$(function () {{ $(\".umbFolderBrowser\").folderBrowser({{ umbracoPath : '{0}', basePath : '{1}' }}); }});",
                IOHelper.ResolveUrl(SystemDirectories.Umbraco),
                IOHelper.ResolveUrl(SystemDirectories.Base)),
                true);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            this.RenderContents(writer);
        }
    }
}
