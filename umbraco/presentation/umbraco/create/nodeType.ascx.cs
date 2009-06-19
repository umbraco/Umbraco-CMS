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

	/// <summary>
	///		Summary description for nodeType.
	/// </summary>
	public partial class nodeType : System.Web.UI.UserControl
	{


		protected void Page_Load(object sender, System.EventArgs e)
		{
			sbmt.Text = ui.Text("create");
            if (!IsPostBack)
            {
                masterType.Attributes.Add("style", "width: 350px;");
                masterType.Items.Add(new ListItem(ui.Text("none") + "...", "0"));
                foreach (cms.businesslogic.web.DocumentType dt in cms.businesslogic.web.DocumentType.GetAllAsList())
                {
//                    if (dt.MasterContentType == 0)
                        masterType.Items.Add(new ListItem(dt.Text, dt.Id.ToString()));
                }
            }
		}

        protected void validationDoctypeName(object sender, ServerValidateEventArgs e) {
            if (cms.businesslogic.web.DocumentType.GetByAlias(rename.Text) != null)
                e.IsValid = false;
            
        }

		protected void sbmt_Click(object sender, System.EventArgs e)
		{
			if (Page.IsValid) 
			{
				int createTemplateVal = 0;
				int nodeId = -1;
				if (createTemplate.Checked)
					createTemplateVal = 1;

				string returnUrl = umbraco.presentation.create.dialogHandler_temp.Create(
					umbraco.helper.Request("nodeType"),
                    int.Parse(masterType.SelectedValue),
					createTemplateVal,
					rename.Text);

				BasePage.Current.ClientTools
					.ChangeContentFrameUrl(returnUrl)
					.ChildNodeCreated()
					.CloseModalWindow();

			}
		
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
