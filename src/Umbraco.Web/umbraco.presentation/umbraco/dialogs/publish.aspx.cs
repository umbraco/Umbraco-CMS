using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using System.Threading;
using Umbraco.Core.Logging;
using umbraco.cms.businesslogic.web;
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

			if (d.Level > 1 && d.PathPublished == false)
			{
				TheForm.Visible = false;
				theEnd.Visible = true;
				feedbackMsg.type = uicontrols.Feedback.feedbacktype.notice;
				feedbackMsg.Text = ui.Text("publish", "contentPublishedFailedByParent", d.Text, base.getUser()) + "</p><p><a href='#' onClick='" + ClientTools.Scripts.CloseModalWindow() + "'>" + ui.Text("closeThisWindow") + "</a>";
	               
				return;
			}

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
                    foreach (cms.businesslogic.web.Document doc in _documents)
                    {
                        if (doc.Published)
                        {
                            library.UpdateDocumentCache(doc);
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
                        library.UpdateDocumentCache(d);
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
        private readonly List<Document> _documents = new List<Document>();

		private void doPublishSubs(Document d) 
		{
            if (d.Published || PublishUnpublishedItems.Checked)
            {
                if (d.PublishWithResult(UmbracoUser))
                {
                    // Needed for supporting distributed calls
                    if (UmbracoSettings.UseDistributedCalls)
                        library.UpdateDocumentCache(d);
                    else
                        _documents.Add(d);

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
                    LogHelper.Debug<publish>(string.Format("Publishing node {0} failed due to event cancelling the publishing", d.Id));
                }
            }
		}

        protected override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/publication.asmx"));
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/legacyAjaxCalls.asmx"));
        }


        /// <summary>
        /// masterPagePrefix control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Literal masterPagePrefix;

        /// <summary>
        /// JsInclude1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::ClientDependency.Core.Controls.JsInclude JsInclude1;

        /// <summary>
        /// TheForm control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Panel TheForm;

        /// <summary>
        /// PublishAll control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.CheckBox PublishAll;

        /// <summary>
        /// PublishUnpublishedItems control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.CheckBox PublishUnpublishedItems;

        /// <summary>
        /// ok control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Button ok;

        /// <summary>
        /// ProgBar1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.ProgressBar ProgBar1;

        /// <summary>
        /// theEnd control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Panel theEnd;

        /// <summary>
        /// feedbackMsg control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.Feedback feedbackMsg;
	}
}
