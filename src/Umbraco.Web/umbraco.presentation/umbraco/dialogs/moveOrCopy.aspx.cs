using System;
using System.Collections;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using Umbraco.Core.IO;
using umbraco.BasePages;
using umbraco.cms.businesslogic.web;
using umbraco.presentation;
using umbraco.cms.businesslogic.media;
using System.Linq;
using umbraco.cms.businesslogic;
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

                    var documentType = new DocumentType(int.Parse(Request.GetItemAsString("id")));

                    //Load master types... 
                    masterType.Attributes.Add("style", "width: 350px;");
                    masterType.Items.Add(new ListItem(ui.Text("none") + "...", "0"));
                    foreach (var docT in DocumentType.GetAllAsList())
                    {
                        masterType.Items.Add(new ListItem(docT.Text, docT.Id.ToString()));
                    }

                    masterType.SelectedValue = documentType.MasterContentType.ToString();

                    rename.Text = documentType.Text + " (copy)";
                    pane_settings.Text = "Make a copy of the document type '" + documentType.Text + "' and save it under a new name";

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

                    var cmsNode = new CMSNode(int.Parse(Request.GetItemAsString("id")));

                    var validAction = true;
                    if (CurrentApp == Constants.Applications.Content && cmsNode.HasChildren)
                        validAction = ValidAction(Request.GetItemAsString("mode") == "cut" ? 'M' : 'O');


                    if (Request.GetItemAsString("mode") == "cut")
                    {
                        pane_form.Text = ui.Text("moveOrCopy", "moveTo", cmsNode.Text, UmbracoUser);
                        pp_relate.Visible = false;
                    }
                    else
                    {
                        pane_form.Text = ui.Text("moveOrCopy", "copyTo", cmsNode.Text, UmbracoUser);
                        pp_relate.Visible = true;
                    }

                    if (validAction == false)
                    {
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "notvalid", "notValid();", true);
                    }
                }
            }

        }

        private bool ValidAction(char actionLetter)
        {
            var cmsNode = new CMSNode(int.Parse(helper.Request("id")));
            var currentAction = BusinessLogic.Actions.Action.GetPermissionAssignable().First(a => a.Letter == actionLetter);
            return CheckPermissions(cmsNode, currentAction);
        }

        private bool CheckPermissions(CMSNode node, IAction currentAction)
        {                       
            var currUserPermissions = new UserPermissions(CurrentUser);
            var lstCurrUserActions = currUserPermissions.GetExistingNodePermission(node.Id);

            if (lstCurrUserActions.Contains(currentAction) == false)
                return false;

            if (node.HasChildren)
            {
                foreach (CMSNode child in node.Children)
                    if (CheckPermissions(child, currentAction) == false)
                        return false;
            }
            return true;
        }

        

        //PPH Handle doctype copies..
        private void HandleDocumentTypeCopy()
        {
            var documentType = new DocumentType(int.Parse(helper.Request("id")));

            //Documentype exists.. create new doc type... 
            var alias = rename.Text;
            var newDocumentType = DocumentType.MakeNew(base.getUser(), alias.Replace("'", "''"));

            newDocumentType.IconUrl = documentType.IconUrl;
            newDocumentType.Thumbnail = documentType.Thumbnail;
            newDocumentType.Description = documentType.Description;
            newDocumentType.allowedTemplates = documentType.allowedTemplates;
            newDocumentType.DefaultTemplate = documentType.DefaultTemplate;
            newDocumentType.AllowedChildContentTypeIDs = documentType.AllowedChildContentTypeIDs;
            newDocumentType.AllowAtRoot = documentType.AllowAtRoot;

            newDocumentType.MasterContentType = int.Parse(masterType.SelectedValue);

            var oldNewTabIds = new Hashtable();
            foreach (var tab in documentType.getVirtualTabs.Where(t => t.ContentType == documentType.Id))
            {
                int tabId = newDocumentType.AddVirtualTab(tab.Caption);
                oldNewTabIds.Add(tab.Id, tabId);
            }

            foreach (var propertyType in documentType.PropertyTypes.Where(p => p.ContentTypeId == documentType.Id))
            {
                var newPropertyType = cms.businesslogic.propertytype.PropertyType.MakeNew(propertyType.DataTypeDefinition, newDocumentType, propertyType.Name, propertyType.Alias);
                newPropertyType.ValidationRegExp = propertyType.ValidationRegExp;
                newPropertyType.SortOrder = propertyType.SortOrder;
                newPropertyType.Mandatory = propertyType.Mandatory;
                newPropertyType.Description = propertyType.Description;

                if (propertyType.TabId > 0 && oldNewTabIds[propertyType.TabId] != null)
                {
                    var newTabId = (int)oldNewTabIds[propertyType.TabId];
                    newPropertyType.TabId = newTabId;
                }
            }

            var returnUrl = string.Format("{0}/settings/editNodeTypeNew.aspx?id={1}", SystemDirectories.Umbraco, newDocumentType.Id);

            newDocumentType.Save();

            pane_settings.Visible = false;
            panel_buttons.Visible = false;

            feedback.Text = "Document type copied";
            feedback.type = uicontrols.Feedback.feedbacktype.success;

            ClientTools.ChangeContentFrameUrl(returnUrl);
        }

        public void HandleMoveOrCopy(object sender, EventArgs e)
        {
            if (Request["app"] == Constants.Applications.Settings)
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
            if (Request.GetItemAsString("copyTo") != "" && helper.Request("id") != "")
            {
                // Check if the current node is allowed at new position
                var nodeAllowed = false;

                var currentNode = new cms.businesslogic.Content(int.Parse(Request.GetItemAsString("id")));

                var newNode = new cms.businesslogic.Content(int.Parse(Request.GetItemAsString("copyTo")));

                // Check on contenttypes
                if (int.Parse(Request.GetItemAsString("copyTo")) == -1)
                {
                    nodeAllowed = true;
                }
                else
                {
                    if (newNode.ContentType.AllowedChildContentTypeIDs.Where(c => c == currentNode.ContentType.Id).Any())
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
                        if ((string.Format(",{0},", newNode.Path)).IndexOf(string.Format(",{0},", currentNode.Id)) > -1)
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

                    var newNodeCaption = newNode.Id == -1 ? ui.Text(Request.GetItemAsString("app")) : newNode.Text;

                    string[] nodes = { currentNode.Text, newNodeCaption };

                    if (Request["mode"] == "cut")
                    {
                        if (Request["app"] == Constants.Applications.Content)
                        {
                            //PPH changed this to document instead of cmsNode to handle republishing.
                            var documentId = int.Parse(Request.GetItemAsString("id"));
                            var document = new Document(documentId);
                            document.Move(int.Parse(Request.GetItemAsString("copyTo")));

                            //NOTE: This seems excessive to have to re-load all content from the database here!?
                            library.RefreshContent();
                        }
                        else
                        {
                            var media = new Media(int.Parse(Request["id"]));
                            media.Move(int.Parse(Request["copyTo"]));
                            media = new Media(int.Parse(Request["id"]));
                            media.XmlGenerate(new XmlDocument());
                            media.Save();
                        }

                        feedback.Text = ui.Text("moveOrCopy", "moveDone", nodes, UmbracoUser) + "</p><p><a href='#' onclick='" + ClientTools.Scripts.CloseModalWindow() + "'>" + ui.Text("closeThisWindow") + "</a>";
                        feedback.type = uicontrols.Feedback.feedbacktype.success;

                        // refresh tree
                        ClientTools.MoveNode(currentNode.Id.ToString(), newNode.Path);
                    }
                    else
                    {
                        var document = new Document(int.Parse(Request.GetItemAsString("id")));
                        document.Copy(int.Parse(Request.GetItemAsString("copyTo")), UmbracoUser, RelateDocuments.Checked);
                        feedback.Text = ui.Text("moveOrCopy", "copyDone", nodes, UmbracoUser) + "</p><p><a href='#' onclick='" + ClientTools.Scripts.CloseModalWindow() + "'>" + ui.Text("closeThisWindow") + "</a>";
                        feedback.type = uicontrols.Feedback.feedbacktype.success;
                        ClientTools.CopyNode(currentNode.Id.ToString(), newNode.Path);
                    }
                }
            }
        }

    }
}
