using System;
using System.Collections;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using umbraco.BasePages;
using System.Linq;
using umbraco.cms.presentation.user;
using umbraco.interfaces;
using Umbraco.Web;
using Umbraco.Core;

namespace umbraco.dialogs
{
    /// <summary>
    /// Summary description for moveOrCopy.
    /// </summary>
    public partial class moveOrCopy : UmbracoEnsuredPage
    {

        protected override void OnInit(EventArgs e)
        {
            CurrentApp = Request["app"];

            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            JTree.DataBind();

            // Put user code to initialize the page here
            if (IsPostBack == false)
            {
                pp_relate.Text = ui.Text("moveOrCopy", "relateToOriginal");

                //Document Type copy Hack...                

                if (CurrentApp == Constants.Applications.Settings)
                {
                    pane_form.Visible = false;
                    pane_form_notice.Visible = false;
                    pane_settings.Visible = true;

                    ok.Text = ui.Text("general", "ok", UmbracoUser);
                    ok.Attributes.Add("style", "width: 60px");

                    var documentType = Services.ContentTypeService.GetContentType(int.Parse(Request.GetItemAsString("id")));

                    //Load master types... 
                    masterType.Attributes.Add("style", "width: 350px;");
                    masterType.Items.Add(new ListItem(ui.Text("none") + "...", "0"));

                    foreach (var docT in Services.ContentTypeService.GetAllContentTypes().OrderBy(x => x.Name))
                    {
                        masterType.Items.Add(new ListItem(docT.Name, docT.Id.ToString(CultureInfo.InvariantCulture)));
                    }

                    masterType.SelectedValue = (documentType.ParentId > 0 ? documentType.ParentId : 0).ToString(CultureInfo.InvariantCulture);

                    rename.Text = documentType.Name + " (copy)";
                    pane_settings.Text = "Make a copy of the document type '" + documentType.Name + "' and save it under a new name";

                }
                else
                {
                    pane_form.Visible = true;
                    pane_form_notice.Visible = true;

                    pane_settings.Visible = false;

                    // Caption and properies on BUTTON
                    ok.Text = ui.Text("general", "ok", UmbracoUser);
                    ok.Attributes.Add("style", "width: 60px");
                    ok.Attributes.Add("disabled", "true");

                    IContentBase currContent;
                    if (CurrentApp == "content")
                    {
                        currContent = Services.ContentService.GetById(Request.GetItemAs<int>("id"));
                    }
                    else
                    {
                        currContent = Services.MediaService.GetById(Request.GetItemAs<int>("id"));
                    }

                    // Preselect the parent of the seslected item.
                    if (currContent.ParentId > 0)
                        JTree.SelectedNodePath = currContent.Path.Substring(0, currContent.Path.LastIndexOf(','));

                    var validAction = true;
                    if (CurrentApp == Constants.Applications.Content && Umbraco.Core.Models.ContentExtensions.HasChildren(currContent, Services))
                    {
                        validAction = ValidAction(currContent, Request.GetItemAsString("mode") == "cut" ? 'M' : 'O');
                    }

                    if (Request.GetItemAsString("mode") == "cut")
                    {
                        pane_form.Text = ui.Text("moveOrCopy", "moveTo", currContent.Name, UmbracoUser);
                        pp_relate.Visible = false;
                    }
                    else
                    {
                        pane_form.Text = ui.Text("moveOrCopy", "copyTo", currContent.Name, UmbracoUser);
                        pp_relate.Visible = true;
                    }

                    if (validAction == false)
                    {
                        panel_buttons.Visible = false;
                        ScriptManager.RegisterStartupScript(this, GetType(), "notvalid", "notValid();", true);
                    }
                }
            }

        }

        private bool ValidAction(IContentBase cmsNode, char actionLetter)
        {
            var currentAction = BusinessLogic.Actions.Action.GetPermissionAssignable().First(a => a.Letter == actionLetter);
            return CheckPermissions(cmsNode, currentAction);
        }

        /// <summary>
        /// Checks if the current user has permissions to execute this action against this node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="currentAction"></param>
        /// <returns></returns>
        /// <remarks>
        /// This used to do a recursive check for all descendent nodes but this is not required and is a massive CPU hog.
        /// See: http://issues.umbraco.org/issue/U4-2632, https://groups.google.com/forum/?fromgroups=#!topic/umbraco-dev/L1D4LwVSP2Y
        /// </remarks>
        private bool CheckPermissions(IContentBase node, IAction currentAction)
        {
            var currUserPermissions = new UserPermissions(CurrentUser);
            var lstCurrUserActions = currUserPermissions.GetExistingNodePermission(node.Id);

            return lstCurrUserActions.Contains(currentAction);
        }

        private void HandleDocumentTypeCopy()
        {
            var contentTypeService = ApplicationContext.Current.Services.ContentTypeService;
            var contentType = contentTypeService.GetContentType(
                int.Parse(Request.GetItemAsString("id")));

            //set the master
            //http://issues.umbraco.org/issue/U4-2843
            //http://issues.umbraco.org/issue/U4-3552
            var parentId = int.Parse(masterType.SelectedValue);
            
            var alias = rename.Text.Trim().Replace("'", "''");
            var clone = contentTypeService.Copy(contentType, alias, rename.Text.Trim(), parentId);

            var returnUrl = string.Format("{0}/settings/editNodeTypeNew.aspx?id={1}", SystemDirectories.Umbraco, clone.Id);

            pane_settings.Visible = false;
            panel_buttons.Visible = false;

            feedback.Text = "Document type copied";
            feedback.type = uicontrols.Feedback.feedbacktype.success;

            ClientTools.ChangeContentFrameUrl(returnUrl);
        }

        public void HandleMoveOrCopy(object sender, EventArgs e)
        {
            if (CurrentApp == Constants.Applications.Settings)
                HandleDocumentTypeCopy();
            else
                HandleDocumentMoveOrCopy();
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/cmsnode.asmx"));
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/legacyAjaxCalls.asmx"));
        }

        private void HandleDocumentMoveOrCopy()
        {
            if (Request.GetItemAsString("copyTo") != "" && Request.GetItemAsString("id") != "")
            {
                // Check if the current node is allowed at new position
                var nodeAllowed = false;

                IContentBase currContent;
                IContentBase parentContent = null;
                IContentTypeBase parentContentType = null;
                if (CurrentApp == "content")
                {
                    currContent = Services.ContentService.GetById(Request.GetItemAs<int>("id"));
                    if (Request.GetItemAs<int>("copyTo") != -1)
                    {
                        parentContent = Services.ContentService.GetById(Request.GetItemAs<int>("copyTo"));
                        if (parentContent != null)
                        {
                            parentContentType = Services.ContentTypeService.GetContentType(parentContent.ContentTypeId);
                        }   
                    }    
                }
                else
                {
                    currContent = Services.MediaService.GetById(Request.GetItemAs<int>("id"));
                    if (Request.GetItemAs<int>("copyTo") != -1)
                    {
                        parentContent = Services.MediaService.GetById(Request.GetItemAs<int>("copyTo"));
                        if (parentContent != null)
                        {
                            parentContentType = Services.ContentTypeService.GetMediaType(parentContent.ContentTypeId);
                        }
                    }                    
                }

                // Check on contenttypes
                if (parentContentType == null)
                {
                    //check if this is allowed at root
                    IContentTypeBase currContentType;
                    if (CurrentApp == "content")
                    {
                        currContentType = Services.ContentTypeService.GetContentType(currContent.ContentTypeId);
                    }
                    else
                    {
                        currContentType = Services.ContentTypeService.GetMediaType(currContent.ContentTypeId);
                    }
                    nodeAllowed = currContentType.AllowedAsRoot;
                    if (!nodeAllowed)
                    {
                        feedback.Text = ui.Text("moveOrCopy", "notAllowedAtRoot", UmbracoUser);
                        feedback.type = uicontrols.Feedback.feedbacktype.error;
                    }
                }
                else
                {
                    var allowedChildContentTypeIds = parentContentType.AllowedContentTypes.Select(x => x.Id).ToArray();
                    if (allowedChildContentTypeIds.Any(x => x.Value == currContent.ContentTypeId))
                    {
                        nodeAllowed = true;
                    }

                    if (nodeAllowed == false)
                    {
                        feedback.Text = ui.Text("moveOrCopy", "notAllowedByContentType", UmbracoUser);
                        feedback.type = uicontrols.Feedback.feedbacktype.error;
                    }
                    else
                    {
                        // Check on paths
                        if ((string.Format(",{0},", parentContent.Path)).IndexOf(string.Format(",{0},", currContent.Id)) > -1)
                        {
                            nodeAllowed = false;
                            feedback.Text = ui.Text("moveOrCopy", "notAllowedByPath", UmbracoUser);
                            feedback.type = uicontrols.Feedback.feedbacktype.error;
                        }
                    }
                }

                if (nodeAllowed)
                {
                    pane_form.Visible = false;
                    pane_form_notice.Visible = false;
                    panel_buttons.Visible = false;

                    var newNodeCaption = parentContent == null 
                        ? ui.Text(CurrentApp) 
                        : parentContent.Name;

                    string[] nodes = { currContent.Name, newNodeCaption };

                    if (Request["mode"] == "cut")
                    {
                        if (CurrentApp == Constants.Applications.Content)
                        {
                            var doc = (IContent)currContent;
                            var copyToId = Request.GetItemAs<int>("copyTo");
                            Services.ContentService.Move(doc, copyToId, UmbracoUser.Id);

                        }
                        else
                        {
                            var media = (IMedia)currContent;
                            var copyToId = Request.GetItemAs<int>("copyTo");
                            Services.MediaService.Move(media, copyToId, UmbracoUser.Id);
                        }

                        feedback.Text = ui.Text("moveOrCopy", "moveDone", nodes, UmbracoUser) + "</p><p><a href='#' onclick='" + ClientTools.Scripts.CloseModalWindow() + "'>" + ui.Text("closeThisWindow") + "</a>";
                        feedback.type = uicontrols.Feedback.feedbacktype.success;

                        // refresh tree
                        ClientTools.MoveNode(currContent.Id.ToString(), currContent.Path);
                    }
                    else
                    {
                        //NOTE: We ONLY support Copy on content not media for some reason.
                        
                        var newContent = (IContent)currContent;
                        Services.ContentService.Copy(newContent, Request.GetItemAs<int>("copyTo"), RelateDocuments.Checked, UmbracoUser.Id);

                        feedback.Text = ui.Text("moveOrCopy", "copyDone", nodes, UmbracoUser) + "</p><p><a href='#' onclick='" + ClientTools.Scripts.CloseModalWindow() + "'>" + ui.Text("closeThisWindow") + "</a>";
                        feedback.type = uicontrols.Feedback.feedbacktype.success;

                        // refresh tree
                        ClientTools.CopyNode(currContent.Id.ToString(), newContent.Path);
                    }
                }
            }
        }

        /// <summary>
        /// JsInclude1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.JsInclude JsInclude1;

        /// <summary>
        /// feedback control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Feedback feedback;

        /// <summary>
        /// pane_form control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Pane pane_form;

        /// <summary>
        /// JTree control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.controls.Tree.TreeControl JTree;

        /// <summary>
        /// pp_relate control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.PropertyPanel pp_relate;

        /// <summary>
        /// RelateDocuments control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.CheckBox RelateDocuments;

        /// <summary>
        /// pane_form_notice control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder pane_form_notice;

        /// <summary>
        /// pane_settings control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Pane pane_settings;

        /// <summary>
        /// PropertyPanel1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.PropertyPanel PropertyPanel1;

        /// <summary>
        /// masterType control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.ListBox masterType;

        /// <summary>
        /// rename control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox rename;

        /// <summary>
        /// RequiredFieldValidator1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.RequiredFieldValidator RequiredFieldValidator1;

        /// <summary>
        /// panel_buttons control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Panel panel_buttons;

        /// <summary>
        /// ok control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Button ok;

    }
}
