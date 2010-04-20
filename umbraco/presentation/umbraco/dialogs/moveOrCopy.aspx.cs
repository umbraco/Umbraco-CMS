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
using umbraco.cms.helpers;
using umbraco.BasePages;
using umbraco.presentation;
using umbraco.cms.businesslogic.media;
using umbraco.IO;
using System.Linq;

namespace umbraco.dialogs
{
	/// <summary>
	/// Summary description for moveOrCopy.
	/// </summary>
	public partial class moveOrCopy : UmbracoEnsuredPage
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
            JTree.DataBind();

			// Put user code to initialize the page here
            if (!IsPostBack) {
                //Document Type copy Hack...
                string app = helper.Request("app");

                if (app == "settings") {
                    pane_form.Visible = false;
                    pane_form_notice.Visible = false;


                   

                    pane_settings.Visible = true;

                    ok.Text = ui.Text("general", "ok", this.getUser());
                    ok.Attributes.Add("style", "width: 60px");

                    cms.businesslogic.web.DocumentType dt = new umbraco.cms.businesslogic.web.DocumentType(int.Parse(helper.Request("id")));

                    //Load master types... 
                    masterType.Attributes.Add("style", "width: 350px;");
                    masterType.Items.Add(new ListItem(ui.Text("none") + "...", "0"));
                    foreach (cms.businesslogic.web.DocumentType docT in cms.businesslogic.web.DocumentType.GetAllAsList()) {
                        masterType.Items.Add(new ListItem(docT.Text, docT.Id.ToString()));
                    }

                    masterType.SelectedValue = dt.MasterContentType.ToString();

                    //hack to close window if not a doctype...
                    if (dt == null) {
                        Response.Write("<script type=\"text/javascript\">javascript:parent.window.close()</script>");
                    } else {
                        rename.Text = dt.Text + " (copy)";
                        pane_settings.Text = "Make a copy of the document type '" + dt.Text + "' and save it under a new name";
                    }

                } else {

                    pane_form.Visible = true;
                    pane_form_notice.Visible = true;

                    pane_settings.Visible = false;

                    // Caption and properies on BUTTON
                    ok.Text = ui.Text("general", "ok", this.getUser());
                    ok.Attributes.Add("style", "width: 60px");
                    ok.Attributes.Add("disabled", "true");

                   
                    string currentPath = "";
                    cms.businesslogic.web.Document d = new cms.businesslogic.web.Document(int.Parse(helper.Request("id")));
                    foreach (string s in d.Path.Split(',')) {
                        if (int.Parse(s) > 0)
                            currentPath += "/" + new cms.businesslogic.web.Document(int.Parse(s)).Text;
                    }

                    if (helper.Request("mode") == "cut") {
                        pane_form.Text = ui.Text("moveOrCopy", "moveTo", d.Text, base.getUser());
                        pp_relate.Visible = false;
                    } else {
                        pane_form.Text = ui.Text("moveOrCopy", "copyTo", d.Text, base.getUser());
                        pp_relate.Visible = true;
                    }
                }
            }
			
		}
        //PPH moving multiple nodes and publishing them aswell.
        private void handleChildNodes(cms.businesslogic.web.Document d) {
            //store children array here because iterating over an Array object is very inneficient.
            var c = d.Children;
            foreach (cms.businesslogic.web.Document cd in c) 
            {
                if (cd.Published) {
                    cd.Publish(new umbraco.BusinessLogic.User(0));
                    //using library.publish to support load balancing.
                    umbraco.library.PublishSingleNode(cd.Id);


                    if (cd.HasChildren) {
                        handleChildNodes(cd);
                    }
                }
            }
        }

        //PPH Handle doctype copies..
        private void HandleDocumentTypeCopy() {

            cms.businesslogic.web.DocumentType eDt = new umbraco.cms.businesslogic.web.DocumentType(int.Parse(helper.Request("id")));
                
            //Documentype exists.. create new doc type... 
            if (eDt != null) {
                        string Alias = rename.Text;
                        cms.businesslogic.web.DocumentType dt = cms.businesslogic.web.DocumentType.MakeNew(base.getUser(), Alias.Replace("'", "''"));

                        dt.IconUrl = eDt.IconUrl;
                        dt.Thumbnail = eDt.Thumbnail;
                        dt.Description = eDt.Description;
                        dt.allowedTemplates = eDt.allowedTemplates;
                        dt.DefaultTemplate = eDt.DefaultTemplate;
                        dt.AllowedChildContentTypeIDs = eDt.AllowedChildContentTypeIDs;

                        dt.MasterContentType = int.Parse(masterType.SelectedValue);

                        Hashtable oldNewTabIds = new Hashtable();
                        foreach (cms.businesslogic.web.DocumentType.TabI tab in eDt.getVirtualTabs.ToList())
                        {
                            int tId = dt.AddVirtualTab(tab.Caption);
                            oldNewTabIds.Add(tab.Id, tId);
                        }

                        foreach (cms.businesslogic.propertytype.PropertyType pt in eDt.PropertyTypes) {

                            cms.businesslogic.propertytype.PropertyType nPt = umbraco.cms.businesslogic.propertytype.PropertyType.MakeNew(pt.DataTypeDefinition, dt, pt.Name, pt.Alias);
                            nPt.ValidationRegExp = pt.ValidationRegExp;
                            nPt.SortOrder = pt.SortOrder;
                            nPt.Mandatory = pt.Mandatory;
                            nPt.Description = pt.Description;
                            
                            if(pt.TabId > 0 && oldNewTabIds[pt.TabId] != null){
                                int newTabId = (int)oldNewTabIds[pt.TabId];
                                nPt.TabId = newTabId;
                            }
                        }

                        string returnUrl = SystemDirectories.Umbraco + "/settings/editNodeTypeNew.aspx?id=" + dt.Id.ToString();
                
                dt.Save();


                pane_settings.Visible = false;
                panel_buttons.Visible = false;

                feedback.Text = "Document type copied";
                feedback.type = umbraco.uicontrols.Feedback.feedbacktype.success;

				ClientTools.ChangeContentFrameUrl(returnUrl);
                
                }                
           }
        
        public void HandleMoveOrCopy(object sender, EventArgs e) {
            if (UmbracoContext.Current.Request["app"] == "settings")
                HandleDocumentTypeCopy();
            else
                HandleDocumentMoveOrCopy(); 
        }

        protected override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/cmsnode.asmx"));
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/legacyAjaxCalls.asmx"));
        }

        private void HandleDocumentMoveOrCopy() 
		{
			if (helper.Request("copyTo") != "" && helper.Request("id") != "") 
			{
				// Check if the current node is allowed at new position
				bool nodeAllowed = false;

				cms.businesslogic.Content currentNode = new cms.businesslogic.Content(int.Parse(helper.Request("id")));
                int oldParent = -1;
                if (currentNode.Level > 1)
                   oldParent = currentNode.Parent.Id;
				cms.businesslogic.Content newNode = new cms.businesslogic.Content(int.Parse(helper.Request("copyTo")));

				// Check on contenttypes
				if (int.Parse(helper.Request("copyTo")) == -1)
					nodeAllowed = true;
				else 
				{
					foreach (int i in newNode.ContentType.AllowedChildContentTypeIDs.ToList())
						if (i == currentNode.ContentType.Id) 
						{
							nodeAllowed = true;
							break;
						}
                    if (!nodeAllowed) {
                        feedback.Text = ui.Text("moveOrCopy", "notAllowedByContentType", base.getUser());
                        feedback.type = umbraco.uicontrols.Feedback.feedbacktype.error;
                    } else {
                        // Check on paths
                        if (((string)("," + newNode.Path + ",")).IndexOf("," + currentNode.Id + ",") > -1) {
                            nodeAllowed = false;
                            feedback.Text = ui.Text("moveOrCopy", "notAllowedByPath", base.getUser());
                            feedback.type = umbraco.uicontrols.Feedback.feedbacktype.error;
                        }
                    }
				}


				if (nodeAllowed) 
				{
                    pane_form.Visible = false;
                    pane_form_notice.Visible = false;
                    panel_buttons.Visible = false;

                    string newNodeCaption = newNode.Id == -1 ? ui.Text(helper.Request("app")) : newNode.Text;

                    string[] nodes = {currentNode.Text, newNodeCaption };

                    if (UmbracoContext.Current.Request["mode"] == "cut")
                    {
                        if (UmbracoContext.Current.Request["app"] == "content")
                        {
                            //PPH changed this to document instead of cmsNode to handle republishing.
                            cms.businesslogic.web.Document d = new umbraco.cms.businesslogic.web.Document(int.Parse(helper.Request("id")));
                            d.Move(int.Parse(helper.Request("copyTo")));
                            if (d.Published)
                            {
                                d.Publish(new umbraco.BusinessLogic.User(0));
                                //using library.publish to support load balancing.
                                //umbraco.library.PublishSingleNode(d.Id);
                                umbraco.library.UpdateDocumentCache(d.Id);

                                //PPH added handling of load balanced moving of multiple nodes...
                                if (d.HasChildren)
                                {
                                    handleChildNodes(d);
                                }

                                //Using the general Refresh content method instead as it supports load balancing. 
                                //we only need to do this if the node is actually published.
                                library.RefreshContent();
                            }
                            d.Save(); //stub to save stuff to the db.
                        }
                        else
                        {
                            Media m = new Media(int.Parse(UmbracoContext.Current.Request["id"]));
                            m.Move(int.Parse(UmbracoContext.Current.Request["copyTo"]));
                        }                                 

                        feedback.Text = ui.Text("moveOrCopy", "moveDone", nodes, base.getUser()) + "</p><p><a href='#' onclick='" + ClientTools.Scripts.CloseModalWindow() + "'>" + ui.Text("closeThisWindow") + "</a>";
                        feedback.type = umbraco.uicontrols.Feedback.feedbacktype.success;

                        // refresh tree
						ClientTools.MoveNode(currentNode.Id.ToString(), newNode.Path);

                    } 
					else 
					{
						cms.businesslogic.web.Document d = new cms.businesslogic.web.Document(int.Parse(helper.Request("id")));
						d.Copy(int.Parse(helper.Request("copyTo")), this.getUser(), RelateDocuments.Checked);
						feedback.Text = ui.Text("moveOrCopy", "copyDone", nodes, base.getUser()) + "</p><p><a href='#' onclick='" + ClientTools.Scripts.CloseModalWindow() + "'>" + ui.Text("closeThisWindow") + "</a>";
                        feedback.type = umbraco.uicontrols.Feedback.feedbacktype.success;
						ClientTools.CopyNode(currentNode.Id.ToString(), newNode.Path);
                    }
				} 
			}
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
		}
		#endregion
	}
}
