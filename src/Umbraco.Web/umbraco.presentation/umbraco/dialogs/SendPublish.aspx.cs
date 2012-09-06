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
using umbraco.BusinessLogic.Actions;
using umbraco.cms.businesslogic.web;

namespace umbraco.dialogs
{
	/// <summary>
	/// Runs all action handlers for the ActionToPublish action for the document with
    /// the corresponding document id passed in by query string
	/// </summary>
	public partial class SendPublish : BasePages.UmbracoEnsuredPage
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
            if (!string.IsNullOrEmpty(Request.QueryString["id"]))
            {
                int docID;
                if (int.TryParse(Request.QueryString["id"], out docID))
                {
                    Document _document = new Document(docID);
                    if (_document != null)
                        BusinessLogic.Actions.Action.RunActionHandlers(_document, ActionToPublish.Instance);
                }
                
            }
            
		}

		
	}
}
