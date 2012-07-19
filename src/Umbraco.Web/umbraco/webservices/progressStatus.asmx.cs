using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Services;

namespace presentation.umbraco.webservices
{
	/// <summary>
	/// Summary description for progressStatus.
	/// </summary>
	[WebService(Namespace="http://umbraco.org/webservices/")]
	public class progressStatus : System.Web.Services.WebService
	{
		public progressStatus()
		{
			//CODEGEN: This call is required by the ASP.NET Web Services Designer
			InitializeComponent();
		}

		#region Component Designer generated code
		
		//Required by the Web Services Designer 
		private IContainer components = null;
				
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if(disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);		
		}
		
		#endregion

		[WebMethod]
		public int GetStatus(string key) 
		{
			try 
			{
				return int.Parse(Application[key].ToString());
			} 
			catch 
			{
				return 0;
			}
		}

	}
}
