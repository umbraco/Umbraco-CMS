using System.Globalization;
using Umbraco.Core;
using Umbraco.Web.UI;
using Umbraco.Web;

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
    using umbraco.cms.businesslogic.web;

	/// <summary>
	///		Summary description for nodeType.
	/// </summary>
	public partial class nodeType : System.Web.UI.UserControl
	{


		protected void Page_Load(object sender, EventArgs e)
		{
			sbmt.Text = ui.Text("create");
            pp_name.Text = ui.Text("name");

            if (!IsPostBack)
            {
                string nodeId = Request.GetItemAsString("nodeId");
                if (String.IsNullOrEmpty(nodeId) || nodeId == "init")
                {
                    masterType.Items.Add(new ListItem(ui.Text("none") + "...", "0"));
                    foreach (DocumentType dt in DocumentType.GetAllAsList())
                    {
                        //                    if (dt.MasterContentType == 0)
                        masterType.Items.Add(new ListItem(dt.Text, dt.Id.ToString(CultureInfo.InvariantCulture)));
                    }
                }
                else
                {
                    // there's already a master doctype defined
                    masterType.Visible = false;
                    masterTypePreDefined.Visible = true;
                    masterTypePreDefined.Text = "<h3>" + new DocumentType(int.Parse(nodeId)).Text + "</h3>";
                }
            }
		}

        protected void validationDoctypeName(object sender, ServerValidateEventArgs e) {
            if (DocumentType.GetByAlias(rename.Text) != null)
                e.IsValid = false;
        }

        protected void validationDoctypeAlias(object sender, ServerValidateEventArgs e)
        {
            if (string.IsNullOrEmpty(rename.Text.ToSafeAlias()))
                e.IsValid = false;
        }

		protected void sbmt_Click(object sender, EventArgs e)
		{
			if (Page.IsValid) 
			{
				var createTemplateVal = 0;
			    if (createTemplate.Checked)
					createTemplateVal = 1;

                // check master type
                string masterTypeVal = String.IsNullOrEmpty(Request.GetItemAsString("nodeId")) || Request.GetItemAsString("nodeId") == "init" ? masterType.SelectedValue : Request.GetItemAsString("nodeId");

                var returnUrl = LegacyDialogHandler.Create(
                    new HttpContextWrapper(Context),
                    BasePage.Current.getUser(),
                    Request.GetItemAsString("nodeType"),
                    createTemplateVal,
					rename.Text,
                    int.Parse(masterTypeVal));

				BasePage.Current.ClientTools
					.ChangeContentFrameUrl(returnUrl)
					.ChildNodeCreated()
					.CloseModalWindow();

			}
		
		}
	}
}
