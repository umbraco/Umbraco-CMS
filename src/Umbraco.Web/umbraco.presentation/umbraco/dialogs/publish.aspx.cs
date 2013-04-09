using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core.Logging;
using umbraco.cms.businesslogic.web;
using umbraco.BusinessLogic;
using Umbraco.Core.Logging;
using umbraco.BasePages;

namespace umbraco.dialogs
{
	/// <summary>
	/// Summary description for publish.
	/// </summary>
	public partial class publish : UmbracoEnsuredPage
	{
		protected Literal total;

		private int _nodeId;
		private int _nodesPublished = 0;
        private readonly List<cms.businesslogic.web.Document> _documents = new List<cms.businesslogic.web.Document>();
        public static string pageName = "";

	    public publish()
	    {
	        CurrentApp = DefaultApps.content.ToString();
	    }

		protected void Page_Load(object sender, EventArgs e)
		{
			_nodeId = int.Parse(helper.Request("id"));
            var d = new cms.businesslogic.web.Document(_nodeId);
            pageName = d.Text;

			if (d.Level > 1 && d.PathPublished == false)
			{
				TheForm.Visible = false;
				theEnd.Visible = true;
				feedbackMsg.type = uicontrols.Feedback.feedbacktype.notice;
				feedbackMsg.Text = ui.Text("publish", "contentPublishedFailedByParent", d.Text, getUser()) + "</p><p><a href='#' onClick='" + ClientTools.Scripts.CloseModalWindow() + "'>" + ui.Text("closeThisWindow") + "</a>";
	               
				return;
			}

            // add control prefix to variable for support with masterpages
            var prefix = PublishUnpublishedItems.ClientID.Replace(PublishUnpublishedItems.ID, "");
            masterPagePrefix.Text = prefix;

            // by default we only count the published ones
			var totalNodesToPublish = cms.businesslogic.web.Document.CountSubs(_nodeId, true);
            try
            {
                Application.Lock();
                // We add both all nodes and only published nodes to the application variables so we can ajax query depending on checkboxes
                Application["publishTotalAll" + _nodeId.ToString()] = cms.businesslogic.CMSNode.CountSubs(_nodeId).ToString();
                Application["publishTotal" + _nodeId.ToString()] = totalNodesToPublish.ToString();
                Application["publishDone" + _nodeId.ToString()] = "0";
            }
            finally
            {
                Application.UnLock();
            }
			total.Text = totalNodesToPublish.ToString();

			// Put user code to initialize the page here
			if (!IsPostBack) 
			{
				// Add caption to checkbox
				PublishAll.Text = ui.Text("publish", "publishAll", d.Text, getUser());
				ok.Text = ui.Text("content", "publish", getUser());
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
                    _nodesPublished = 0;

                    DoPublishSubs(d);

                    Application.Lock();
                    Application["publishTotal" + _nodeId.ToString()] = 0;
                    Application.UnLock();

                    feedbackMsg.type = uicontrols.Feedback.feedbacktype.success;

                    feedbackMsg.Text = ui.Text("publish", "nodePublishAll", d.Text, getUser()) + "</p><p><a href='#' onclick='" + ClientTools.Scripts.CloseModalWindow() + "'>" + ui.Text("closeThisWindow") + "</a>";

					ClientTools.ReloadActionNode(true, true);

                    Application.Lock();

                    Application["publishTotal" + _nodeId.ToString()] = null;
                    Application["publishDone" + _nodeId.ToString()] = null;
                    Application.UnLock();
                }
                else
                {
                    if (d.PublishWithResult(getUser()))
                    {                        
                        feedbackMsg.type = uicontrols.Feedback.feedbacktype.success;
						feedbackMsg.Text = ui.Text("publish", "nodePublish", d.Text, getUser()) + "</p><p><a href='#' onclick='" + ClientTools.Scripts.CloseModalWindow() + "'>" + ui.Text("closeThisWindow") + "</a>";						
                    }
                    else {
                        feedbackMsg.type = uicontrols.Feedback.feedbacktype.notice;
						feedbackMsg.Text = ui.Text("publish", "contentPublishedFailedByEvent", d.Text, getUser()) + "</p><p><a href='#' onClick='" + ClientTools.Scripts.CloseModalWindow() + "'>" + ui.Text("closeThisWindow") + "</a>";
                    }
					ClientTools.ReloadActionNode(true, false);
                }

				TheForm.Visible = false;
                theEnd.Visible = true;
			}
		}        

		private void DoPublishSubs(cms.businesslogic.web.Document d) 
		{
            if (d.Published || PublishUnpublishedItems.Checked)
            {
                if (d.PublishWithResult(UmbracoUser))
                {
                    

                    _nodesPublished++;
                    Application.Lock();
                    Application["publishDone" + _nodeId.ToString()] = _nodesPublished.ToString();
                    Application.UnLock();
                    foreach (var dc in d.Children)
                    {
                        DoPublishSubs(dc);
                    }
                }
                else
                {
                    LogHelper.Warn<publish>("Publishing failed due to event cancelling the publishing for document " + d.Id);
                }
            }
		}

	    protected override void OnPreRender(EventArgs e)
	    {
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
