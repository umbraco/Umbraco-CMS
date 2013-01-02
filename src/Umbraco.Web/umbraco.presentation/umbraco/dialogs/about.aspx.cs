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
using System.Reflection;
using System.Diagnostics;
using Umbraco.Core.Configuration;
using umbraco.IO;

namespace umbraco.dialogs
{
	/// <summary>
	/// Summary description for about.
	/// </summary>
	public partial class about : BasePages.UmbracoEnsuredPage
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
            version.Text = UmbracoVersion.Current.ToString(3);
			thisYear.Text = DateTime.Now.Year.ToString();


			// Get the version of the umbraco.dll by looking at a class in that dll
            // Had to do it like this due to medium trust issues, see: http://haacked.com/archive/2010/11/04/assembly-location-and-medium-trust.aspx
            var versionNumber = new AssemblyName(typeof(Umbraco.Web.ApplicationContextExtensions).Assembly.FullName).Version.ToString();
            version.Text += string.Format(" (Assembly version: {0})", versionNumber);
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
