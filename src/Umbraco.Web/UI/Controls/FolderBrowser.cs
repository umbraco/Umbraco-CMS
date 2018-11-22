using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClientDependency.Core;
using Umbraco.Core;
using Umbraco.Web.UI.Bundles;
using umbraco.BasePages;
using Umbraco.Core.IO;

namespace Umbraco.Web.UI.Controls
{
    [ClientDependency(ClientDependencyType.Css, "ContextMenu/Css/jquery.contextMenu.css", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Css, "FolderBrowser/Css/folderbrowser.css", "UmbracoClient")]    
    [ClientDependency(ClientDependencyType.Javascript, "ContextMenu/Js/jquery.contextMenu.js", "UmbracoClient", Priority = 5)]
    [ClientDependency(ClientDependencyType.Javascript, "FileUploader/js/jquery.fileUploader.js", "UmbracoClient", Priority = 6)]
    [ClientDependency(ClientDependencyType.Javascript, "FolderBrowser/js/folderbrowser.js", "UmbracoClient", Priority = 10)]
    [ToolboxData("<{0}:FolderBrowser runat=server></{0}:FolderBrowser>")]
    public class FolderBrowser : WebControl
    {
        protected Panel Panel;

        protected int ParentId
        {
            get
            {
                // Try and parse from querystring
                if (!string.IsNullOrEmpty(Context.Request.QueryString["id"]))
                {
                    int id;
                    if (Int32.TryParse(Context.Request.QueryString["id"], out id))
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
            //Ensure the bundles are added
            Controls.Add(new JsApplicationLib());
            Controls.Add(new JsJQueryCore());

            // Create the panel surround
            Panel = new Panel
            {
                ID = "FolderBrowser",
                CssClass = "umbFolderBrowser"
            };

            Panel.Attributes.Add("data-parentid", ParentId.ToString());

            var sb = new StringBuilder();

            // Create the breadcrumb
            var breadCrumb = new List<global::umbraco.cms.businesslogic.media.Media> { ParentNode };

            var parent = ParentNode;
            while (parent.Id != -1)
            {
                parent = new global::umbraco.cms.businesslogic.media.Media(parent.ParentId);
                breadCrumb.Add(parent);
            }

            breadCrumb.Reverse();

            sb.Append("<ul class='breadcrumb'><li><strong>You are here:</strong></li>");
            foreach (var media in breadCrumb)
            {
                if (media.Id == ParentId)
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

            // Path for tree refresh
            Panel.Attributes.Add("data-nodepath", ParentNode.Path);

            // Create size changer
            sb.Append("<div class='thumb-sizer'>" +
                      "<input type='radio' name='thumb_size' id='thumb_size_small' value='small' data-bind='checked: thumbSize' />" +
                      "<label for='thumb_size_small'><img src='images/thumbs_smll.png' alt='Small thumbnails' /></label>" +
                      "<input type='radio' name='thumb_size' id='thumb_size_medium' value='medium' data-bind='checked: thumbSize' />" +
                      "<label for='thumb_size_medium'><img src='images/thumbs_med.png' alt='Medium thumbnails' /></label>" +
                      "<input type='radio' name='thumb_size' id='thumb_size_large' value='large' data-bind='checked: thumbSize' />" +
                      "<label for='thumb_size_large'><img src='images/thumbs_lrg.png' alt='Large thumbnails' /></label>" +
                      "</div>");

            // Create the filter input
            sb.Append("<div class='filter'><input type='text' placeholder='filter...' data-bind=\"value: filterTerm, valueUpdate: 'afterkeydown'\" id='filterTerm'/></div>");

            // Create throbber to display whilst loading items
            sb.Append("<img src='images/throbber.gif' alt='' class='throbber' data-bind=\"visible: filtered().length == 0\" />");

            // Create thumbnails container
            sb.Append("<ul class='items' data-bind='foreach: filtered'>" +
                      "<li data-bind=\"attr: { 'data-id': Id, 'data-order': $index() }, css: { selected: selected() }, event: { mousedown: toggleSelected, contextmenu: toggleSelected, dblclick: edit }\"><div><span class='img'><img data-bind='attr: { src: ThumbnailUrl }' /></span><span data-bind='text: Name'></span></div></li>" +
                      "</ul>");

            Panel.Controls.Add(new LiteralControl(sb.ToString()));

            Controls.Add(Panel);

            Page.ClientScript.RegisterStartupScript(typeof(FolderBrowser),
                "RegisterFolderBrowsers",
                string.Format("$(function () {{ $(\".umbFolderBrowser\").folderBrowser({{ umbracoPath : '{0}', basePath : '{1}', reqver : '{2}' }}); " +
                 "$(\".umbFolderBrowser #filterTerm\").keypress(function(event) {{ return event.keyCode != 13; }});}});",
                IOHelper.ResolveUrl(SystemDirectories.Umbraco),
                IOHelper.ResolveUrl(SystemDirectories.Base),
                UmbracoEnsuredPage.umbracoUserContextID.EncryptWithMachineKey() ),
                true);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            this.RenderContents(writer);
        }
    }
}
