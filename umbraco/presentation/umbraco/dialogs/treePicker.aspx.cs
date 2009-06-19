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
using umbraco.cms.presentation.Trees;

namespace umbraco.dialogs
{
	/// <summary>
	/// Summary description for treePicker.
	/// </summary>
	public partial class treePicker : BasePages.UmbracoEnsuredPage
	{
		protected void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
		}

        protected string TreeInitUrl
        {
            get
            {
				TreeRequestParams treeParams = TreeRequestParams.FromQueryStrings();
				return TreeService.GetInitUrl(null, treeParams.TreeType, false, true,
					TreeDialogModes.id, treeParams.Application, "", "");
            }
        }


		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{			
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
