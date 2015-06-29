using System;
using System.Globalization;
using Umbraco.Core.Configuration;

namespace umbraco.dialogs
{
	/// <summary>
	/// Summary description for about.
	/// </summary>
	public partial class about : BasePages.UmbracoEnsuredPage
	{

		protected void Page_Load(object sender, EventArgs e)
		{
		    // Put user code to initialize the page here
			thisYear.Text = DateTime.Now.Year.ToString(CultureInfo.InvariantCulture);
		    version.Text = string.IsNullOrEmpty(UmbracoVersion.CurrentComment) 
                ? string.Format("{0} (Assembly version: {1})", UmbracoVersion.Current.ToString(3), UmbracoVersion.AssemblyVersion) 
                : string.Format("{0}-{1} (Assembly version: {2})", UmbracoVersion.Current.ToString(3), UmbracoVersion.CurrentComment, UmbracoVersion.AssemblyVersion);
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
