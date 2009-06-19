namespace umbraco.presentation.dashboard
{
	using System;
	using System.Data;
	using System.Drawing;
	using System.Web;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;

	/// <summary>
	///		Summary description for quickEdit.
	/// </summary>
	public partial class quickEdit : System.Web.UI.UserControl
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
            
            //uses the library function instead, to load the script in the head.. 
            //library.RegisterJavaScriptFile("jqueryAutocomplete", GlobalSettings.Path + "/js/autocomplete/jquery.autocomplete.js");
            //library.RegisterJavaScriptFile("jqueryAutocompleteImplementation", GlobalSettings.Path + "/js/autocomplete/jquery.autocomplete.implementation.js");

            /*
            System.Web.UI.ScriptManager sm = Page.FindControl("umbracoScriptManager") as System.Web.UI.ScriptManager;
            sm.Services.Add(new System.Web.UI.ServiceReference(GlobalSettings.Path + "/webservices/search.asmx"));
            */
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
	}
}
