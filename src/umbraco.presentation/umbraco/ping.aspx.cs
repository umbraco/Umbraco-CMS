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

namespace umbraco.presentation
{
	/// <summary>
	/// Summary description for ping.
	/// </summary>
	public partial class ping : System.Web.UI.Page
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
            /*
			if (GlobalSettings.DebugMode)
				BusinessLogic.Log.Add(
					BusinessLogic.LogTypes.Ping, 
					BusinessLogic.User.GetUser(0),
					-1,
					"");
             */
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
