using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Xml;
using Umbraco.Core.IO;
using umbraco.cms.helpers;
using umbraco.BasePages;
using umbraco.presentation;
using umbraco.cms.businesslogic.media;
using System.Linq;
using umbraco.cms.businesslogic;
using umbraco.cms.presentation.user;
using umbraco.interfaces;
using System.Collections.Generic;

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
            if (!IsPostBack)
            {
                pp_relate.Text = ui.Text("moveOrCopy", "relateToOriginal");

                //Document Type copy Hack...                

                if (CurrentApp == "settings")
                {
                    pane_form.Visible = false;
                    pane_form_notice.Visible = false;

                    pane_settings.Visible = true;

                    ok.Text = ui.Text("general", "ok", this.getUser());
                    ok.Attributes.Add("style", "width: 60px");

                    var dt = new cms.businesslogic.web.DocumentType(int.Parse(helper.Request("id")));

                    //Load master types... 
                    masterType.Attributes.Add("style", "width: 350px;");
                    masterType.Items.Add(new ListItem(ui.Text("none") + "...", "0"));
                    foreach (cms.businesslogic.web.DocumentType docT in cms.businesslogic.web.DocumentType.GetAllAsList())
                    {
                        masterType.Items.Add(new ListItem(docT.Text, docT.Id.ToString()));
                    }

                    masterType.SelectedValue = dt.MasterContentType.ToString();

                    rename.Text = dt.Text + " (copy)";
                    pane_settings.Text = "Make a copy of the document type '" + dt.Text + "' and save it under a new name";

                }
                else
                {

                    pane_form.Visible = true;
                    pane_form_notice.Visible = true;

                    pane_settings.Visible = false;

                    // Caption and properies on BUTTON
                    ok.Text = ui.Text("general", "ok", this.getUser());
                    ok.Attributes.Add("style", "width: 60px");
                    ok.Attributes.Add("disabled", "true");


                    var currentPath = "";
                    var d = new CMSNode(int.Parse(helper.Request("id")));
                    foreach (var s in d.Path.Split(','))
                    {
                        if (int.Parse(s) > 0)
                            currentPath += "/" + new CMSNode(int.Parse(s)).Text;
                    }
                    
                    var validAction = true;
                    // only validate permissions in content
                    if (CurrentApp == "content" && d.HasChildren)
                    {
                        validAction = ValidAction(helper.Request("mode") == "cut" ? 'M' : 'O');
                    }


                    if (helper.Request("mode") == "cut")
                    {
                        pane_form.Text = ui.Text("moveOrCopy", "moveTo", d.Text, base.getUser());
                        pp_relate.Visible = false;
                    }
                    else
                    {
                        pane_form.Text = ui.Text("moveOrCopy", "copyTo", d.Text, base.getUser());
                        pp_relate.Visible = true;
                    }

                    if (!validAction)
                    {
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "notvalid", "notValid();", true);

                    }
                }
            }
			
		}

        private static bool ValidAction(char actionLetter)
        {
            var d = new CMSNode(int.Parse(helper.Request("id")));
            var currentAction = BusinessLogic.Actions.Action.GetPermissionAssignable().First(a => a.Letter == actionLetter);
            return CheckPermissions(d, currentAction,actionLetter);
           
        }

        private static bool CheckPermissions(CMSNode node, IAction currentAction, char actionLetter)
        {                       
            var currUserPermissions = new UserPermissions(CurrentUser);
            var lstCurrUserActions = currUserPermissions.GetExistingNodePermission(node.Id);

            if (!lstCurrUserActions.Contains(currentAction))
                return false;
            if (node.HasChildren)
            {
                return node.Children.Cast<CMSNode>().All(c => CheckPermissions(c, currentAction, actionLetter));
            }
            return true;
        }

        //PPH moving multiple nodes and publishing them aswell.
	    private static void HandleChildNodes(cms.businesslogic.web.Document d)
	    {	     
	        var c = d.Children;
	        foreach (var cd in c)
	        {
	            if (cd.Published)
	            {
	                cd.Publish(new BusinessLogic.User(0));
	                //using library.publish to support load balancing.
	                library.UpdateDocumentCache(cd);


	                if (cd.HasChildren)
	                {
	                    HandleChildNodes(cd);
	                }
	            }
	        }
	    }

	    //PPH Handle doctype copies..
	    private void HandleDocumentTypeCopy()
	    {

	        var eDt = new cms.businesslogic.web.DocumentType(int.Parse(helper.Request("id")));

            var alias = rename.Text;
            var dt = cms.businesslogic.web.DocumentType.MakeNew(getUser(), alias.Replace("'", "''"));

            dt.IconUrl = eDt.IconUrl;
            dt.Thumbnail = eDt.Thumbnail;
            dt.Description = eDt.Description;
            dt.allowedTemplates = eDt.allowedTemplates;
            dt.DefaultTemplate = eDt.DefaultTemplate;
            dt.AllowedChildContentTypeIDs = eDt.AllowedChildContentTypeIDs;

            dt.MasterContentType = int.Parse(masterType.SelectedValue);

            var oldNewTabIds = new Hashtable();
            foreach (var tab in eDt.getVirtualTabs.ToList())
            {
                if (tab.ContentType == eDt.Id)
                {
                    var tId = dt.AddVirtualTab(tab.Caption);
                    oldNewTabIds.Add(tab.Id, tId);
                }
            }

            foreach (var pt in eDt.PropertyTypes)
            {
                if (pt.ContentTypeId == eDt.Id)
                {
                    var nPt = cms.businesslogic.propertytype.PropertyType.MakeNew(pt.DataTypeDefinition, dt, pt.Name, pt.Alias);
                    nPt.ValidationRegExp = pt.ValidationRegExp;
                    nPt.SortOrder = pt.SortOrder;
                    nPt.Mandatory = pt.Mandatory;
                    nPt.Description = pt.Description;

                    if (pt.TabId > 0 && oldNewTabIds[pt.TabId] != null)
                    {
                        var newTabId = (int)oldNewTabIds[pt.TabId];
                        nPt.TabId = newTabId;
                    }
                }
            }

            var returnUrl = SystemDirectories.Umbraco + "/settings/editNodeTypeNew.aspx?id=" + dt.Id.ToString();

            dt.Save();


            pane_settings.Visible = false;
            panel_buttons.Visible = false;

            feedback.Text = "Document type copied";
            feedback.type = uicontrols.Feedback.feedbacktype.success;

            ClientTools.ChangeContentFrameUrl(returnUrl);
	    }

	    public void HandleMoveOrCopy(object sender, EventArgs e)
	    {
	        if (Request["app"] == "settings")
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
			if (helper.Request("copyTo") != "" && helper.Request("id") != "") 
			{
				// Check if the current node is allowed at new position
				var nodeAllowed = false;

				var currentNode = new cms.businesslogic.Content(int.Parse(helper.Request("id")));
                var oldParent = -1;
                if (currentNode.Level > 1)
                   oldParent = currentNode.Parent.Id;
				var newNode = new cms.businesslogic.Content(int.Parse(helper.Request("copyTo")));

				// Check on contenttypes
			    if (int.Parse(helper.Request("copyTo")) == -1)
			    {
			        nodeAllowed = true;
			    }
			    else
			    {
			        if (newNode.ContentType.AllowedChildContentTypeIDs.ToList().Any(i => i == currentNode.ContentType.Id))
			        {
			            nodeAllowed = true;
			        }
			        if (!nodeAllowed)
			        {
			            feedback.Text = ui.Text("moveOrCopy", "notAllowedByContentType", base.getUser());
			            feedback.type = uicontrols.Feedback.feedbacktype.error;
			        }
			        else
			        {
			            // Check on paths
			            if (("," + newNode.Path + ",").IndexOf("," + currentNode.Id + ",") > -1)
			            {
			                nodeAllowed = false;
			                feedback.Text = ui.Text("moveOrCopy", "notAllowedByPath", getUser());
			                feedback.type = uicontrols.Feedback.feedbacktype.error;
			            }
			        }
			    }


			    if (nodeAllowed) 
				{
                    pane_form.Visible = false;
                    pane_form_notice.Visible = false;
                    panel_buttons.Visible = false;

                    var newNodeCaption = newNode.Id == -1 ? ui.Text(helper.Request("app")) : newNode.Text;

                    string[] nodes = {currentNode.Text, newNodeCaption };

                    if (Request["mode"] == "cut")
                    {
                        if (Request["app"] == "content")
                        {
                            //PPH changed this to document instead of cmsNode to handle republishing.
                            var d = new cms.businesslogic.web.Document(int.Parse(helper.Request("id")));
                            d.Move(int.Parse(helper.Request("copyTo")));
                            if (d.Published)
                            {
                                d.Publish(new BusinessLogic.User(0));
                                //using library.publish to support load balancing.
                                //umbraco.library.PublishSingleNode(d.Id);
                                library.UpdateDocumentCache(d);

                                //PPH added handling of load balanced moving of multiple nodes...
                                if (d.HasChildren)
                                {
                                    HandleChildNodes(d);
                                }

                                //Using the general Refresh content method instead as it supports load balancing. 
                                //we only need to do this if the node is actually published.
                                library.RefreshContent();
                            }
                            d.Save(); //stub to save stuff to the db.
                        }
                        else
                        {
                            var m = new Media(int.Parse(Request["id"]));
                            m.Move(int.Parse(Request["copyTo"]));
                            m.XmlGenerate(new XmlDocument());
                            library.ClearLibraryCacheForMedia(m.Id);
                        }                                 

                        feedback.Text = ui.Text("moveOrCopy", "moveDone", nodes, getUser()) + "</p><p><a href='#' onclick='" + ClientTools.Scripts.CloseModalWindow() + "'>" + ui.Text("closeThisWindow") + "</a>";
                        feedback.type = uicontrols.Feedback.feedbacktype.success;

                        // refresh tree
						ClientTools.MoveNode(currentNode.Id.ToString(), newNode.Path);

                    } 
					else 
					{
						var d = new cms.businesslogic.web.Document(int.Parse(helper.Request("id")));
						d.Copy(int.Parse(helper.Request("copyTo")), getUser(), RelateDocuments.Checked);
						feedback.Text = ui.Text("moveOrCopy", "copyDone", nodes, getUser()) + "</p><p><a href='#' onclick='" + ClientTools.Scripts.CloseModalWindow() + "'>" + ui.Text("closeThisWindow") + "</a>";
                        feedback.type = uicontrols.Feedback.feedbacktype.success;
						ClientTools.CopyNode(currentNode.Id.ToString(), newNode.Path);
                    }
				} 
			}
		}

	}
}
