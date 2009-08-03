using System;

namespace umbraco.js
{
	/// <summary>
	/// Summary description for contextmenu.
	/// </summary>
	public partial class contextmenu : BasePages.UmbracoEnsuredPage
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
            Response.ContentType = "text/javascript";
			Response.Write(new BusinessLogic.Actions.Action().ReturnJavascript(System.Threading.Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName));
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
