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

namespace umbraco.presentation.umbraco.dialogs
{
	/// <summary>
	/// Summary description for viewAuditTrail.
	/// </summary>
	public partial class viewAuditTrail : BasePages.UmbracoEnsuredPage
	{
	    public viewAuditTrail()
	    {
            CurrentApp = BusinessLogic.DefaultApps.content.ToString();

	    }
	
		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
			//nodeName.Text = new cms.businesslogic.CMSNode(int.Parse(helper.Request("nodeID"))).Text;
			auditLog.DataSource = BusinessLogic.Log.Instance.GetAuditLogItems(int.Parse(UmbracoContext.Current.Request["nodeID"]));
			auditLog.DataBind();
			auditLog.BorderWidth = 0;
			auditLog.BorderStyle = BorderStyle.None;
		}

		public string FormatAction(string action) 
		{
			action = action.ToLower();
			if (action == "new")
				action = "create";
			ArrayList actions = BusinessLogic.Actions.Action.GetAll();
			foreach (interfaces.IAction a in actions)
			{
			    return ui.Text(action);
            }
            return	action;		
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
