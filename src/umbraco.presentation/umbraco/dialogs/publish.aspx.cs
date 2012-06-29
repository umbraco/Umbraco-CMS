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

using System.Threading;
using umbraco.cms.helpers;
using umbraco.BasePages;

namespace umbraco.dialogs
{
	/// <summary>
	/// Summary description for publish.
	/// </summary>
	public partial class publish : BasePages.UmbracoEnsuredPage
	{
		protected System.Web.UI.WebControls.Literal total;

		private int nodeId;
		private int nodesPublished = 0;
        public static string pageName = "";

		protected void Page_Load(object sender, System.EventArgs e)
		{
			nodeId = int.Parse(helper.Request("id"));
            cms.businesslogic.web.Document d = new cms.businesslogic.web.Document(nodeId);
            pageName = d.Text;

            // add control prefix to variable for support with masterpages
            string prefix = PublishUnpublishedItems.ClientID.Replace(PublishUnpublishedItems.ID, "");
            masterPagePrefix.Text = prefix;

            // by default we only count the published ones
			int TotalNodesToPublish = cms.businesslogic.web.Document.CountSubs(nodeId, true);
            try
            {
                Application.Lock();
                // We add both all nodes and only published nodes to the application variables so we can ajax query depending on checkboxes
                Application["publishTotalAll" + nodeId.ToString()] = cms.businesslogic.CMSNode.CountSubs(nodeId).ToString();
                Application["publishTotal" + nodeId.ToString()] = TotalNodesToPublish.ToString();
                Application["publishDone" + nodeId.ToString()] = "0";
            }
            finally
            {
                Application.UnLock();
            }
			total.Text = TotalNodesToPublish.ToString();

			// Put user code to initialize the page here
			if (!IsPostBack) 
			{
				// Add caption to checkbox
				PublishAll.Text = ui.Text("publish", "publishAll", d.Text, base.getUser());
				ok.Text = ui.Text("content", "publish", base.getUser());
				ok.Attributes.Add("style", "width: 60px");
				ok.Attributes.Add("onClick", "startPublication();");

                // Add checkbox event, so the publish unpublished childs gets enabled
                PublishUnpublishedItems.LabelAttributes.Add("disabled", "true");
                PublishUnpublishedItems.LabelAttributes.Add("id", "publishUnpublishedItemsLabel");
                PublishUnpublishedItems.InputAttributes.Add("disabled", "true");
                PublishAll.InputAttributes.Add("onclick", "togglePublishingModes(this)");
			} 
			else 
			{

                if (PublishAll.Checked)
                {
                    nodesPublished = 0;

                    doPublishSubs(d);

                    //PPH added load balancing...
                    //content.Instance.PublishNode(documents);
                    foreach (cms.businesslogic.web.Document doc in documents)
                    {
                        if (doc.Published)
                        {
                            library.UpdateDocumentCache(doc.Id);
                        }
                    }

                    Application.Lock();
                    Application["publishTotal" + nodeId.ToString()] = 0;
                    Application.UnLock();

                    feedbackMsg.type = umbraco.uicontrols.Feedback.feedbacktype.success;

                    feedbackMsg.Text = ui.Text("publish", "nodePublishAll", d.Text, base.getUser()) + "</p><p><a href='#' onclick='" + ClientTools.Scripts.CloseModalWindow() + "'>" + ui.Text("closeThisWindow") + "</a>";

					ClientTools.ReloadActionNode(true, true);

                    Application.Lock();

                    Application["publishTotal" + nodeId.ToString()] = null;
                    Application["publishDone" + nodeId.ToString()] = null;
                    Application.UnLock();
                }
                else
                {
                    if (d.PublishWithResult(base.getUser()))
                    {
                        library.UpdateDocumentCache(d.Id);
                        feedbackMsg.type = umbraco.uicontrols.Feedback.feedbacktype.success;
						feedbackMsg.Text = ui.Text("publish", "nodePublish", d.Text, base.getUser()) + "</p><p><a href='#' onclick='" + ClientTools.Scripts.CloseModalWindow() + "'>" + ui.Text("closeThisWindow") + "</a>";						
                    }
                    else {
                        feedbackMsg.type = umbraco.uicontrols.Feedback.feedbacktype.notice;
						feedbackMsg.Text = ui.Text("publish", "contentPublishedFailedByEvent", d.Text, base.getUser()) + "</p><p><a href='#' onClick='" + ClientTools.Scripts.CloseModalWindow() + "'>" + ui.Text("closeThisWindow") + "</a>";
                    }
					ClientTools.ReloadActionNode(true, false);
                }

				TheForm.Visible = false;
                theEnd.Visible = true;
			}
		}
        private System.Collections.Generic.List<cms.businesslogic.web.Document> documents = new System.Collections.Generic.List<umbraco.cms.businesslogic.web.Document>();

		private void doPublishSubs(cms.businesslogic.web.Document d) 
		{
            if (d.Published || PublishUnpublishedItems.Checked)
            {
                if (d.PublishWithResult(base.getUser()))
                {
                    // Needed for supporting distributed calls
                    if (UmbracoSettings.UseDistributedCalls)
                        library.UpdateDocumentCache(d.Id);
                    else
                        documents.Add(d);

                    nodesPublished++;
                    Application.Lock();
                    Application["publishDone" + nodeId.ToString()] = nodesPublished.ToString();
                    Application.UnLock();
                    foreach (cms.businesslogic.web.Document dc in d.Children)
                    {
                        doPublishSubs(dc);
                    }
                }
                else {
                    BusinessLogic.Log.Add(umbraco.BusinessLogic.LogTypes.Error, d.Id, "Publishing failed due to event cancelling the publishing");
                }
            }
		}

        protected override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/publication.asmx"));
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/legacyAjaxCalls.asmx"));
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
