using Umbraco.Core.Logging;
using Umbraco.Web.UI;

namespace umbraco.cms.presentation.create.controls
{
	using System;
	using System.Data;
	using System.Drawing;
	using System.Web;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;
	using umbraco.cms.helpers;
	using umbraco.BasePages;
    using umbraco.BusinessLogic;

	/// <summary>
	///		Summary description for simple.
	/// </summary>
	public partial class simple : System.Web.UI.UserControl
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
			sbmt.Text = ui.Text("create");
			// Put user code to initialize the page here
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
		///		Required method for Designer support - do not modify
		///		the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{

		}
		#endregion

		protected void sbmt_Click(object sender, System.EventArgs e)
		{
			if (Page.IsValid) 
			{
				var nodeId = -1;
				if (helper.Request("nodeId") != "init")
					nodeId = int.Parse(helper.Request("nodeId"));

                var returnUrl = LegacyDialogHandler.Create(
                    new HttpContextWrapper(Context),
                    BasePage.Current.getUser(),
                    helper.Request("nodeType"),
					nodeId,
					rename.Text);

				BasePage.Current.ClientTools
					.ChangeContentFrameUrl(returnUrl)
					.ChildNodeCreated()
					.CloseModalWindow();

                
            }
		
		}
	}
}
