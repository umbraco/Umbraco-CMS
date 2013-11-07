using System.Web;
using System.Web.UI;
using Umbraco.Core.Logging;
using Umbraco.Web.UI;
using Umbraco.Web;
using System;
using Umbraco.Web.UI.Controls;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using UmbracoUserControl = Umbraco.Web.UI.Controls.UmbracoUserControl;

namespace umbraco.cms.presentation.create.controls
{
    /// <summary>
	///		Summary description for simple.
	/// </summary>
	public partial class simple : UmbracoUserControl
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
			sbmt.Text = ui.Text("create");
            rename.Attributes["placeholder"] = ui.Text("name");

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
				int nodeId;
			    if (int.TryParse(Request.QueryString["nodeId"], out nodeId) == false)
			        nodeId = -1;


                var returnUrl = LegacyDialogHandler.Create(
                    new HttpContextWrapper(Context),
                    new User(Security.CurrentUser),
                    Request.GetItemAsString("nodeType"),
					nodeId,
					rename.Text.Trim());

				BasePage.Current.ClientTools
					.ChangeContentFrameUrl(returnUrl)
					.ChildNodeCreated()
					.CloseModalWindow();
            }
		
		}
	}
}
