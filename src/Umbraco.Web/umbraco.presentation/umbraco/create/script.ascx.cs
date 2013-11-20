using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Umbraco.Core.Configuration;
using Umbraco.Web.UI;
using umbraco.cms.helpers;
using umbraco.BasePages;
using Umbraco.Core.IO;

namespace umbraco.presentation.umbraco.create
{
	public partial class script : System.Web.UI.UserControl
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			sbmt.Text = ui.Text("create");
		}

		private void sbmt_Click(object sender, System.EventArgs e)
		{
			if (Page.IsValid)
			{
				string path = helper.Request("nodeID");
                string relativepath = path + "/";

				if (path == "init")
				{
                    relativepath = "";
				}

				int createFolder = 0;
				if (scriptType.SelectedValue == "")
					createFolder = 1;

                string returnUrl = LegacyDialogHandler.Create(
                    new HttpContextWrapper(Context),
                    BasePage.Current.getUser(),
					helper.Request("nodeType"),
					createFolder,
                    relativepath + "¤" + rename.Text + "¤" + scriptType.SelectedValue);

				BasePage.Current.ClientTools
                    .ChangeContentFrameUrl(returnUrl)
                    .ReloadActionNode(false, true)
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
			this.sbmt.Click += new System.EventHandler(this.sbmt_Click);
			this.Load += new System.EventHandler(this.Page_Load);

            string[] fileTypes = UmbracoConfig.For.UmbracoSettings().Content.ScriptFileTypes.ToArray();

            scriptType.Items.Add(new ListItem(ui.Text("folder"), ""));
		    scriptType.Items.FindByText(ui.Text("folder")).Selected = true;

			foreach (string str in fileTypes)
			{
				scriptType.Items.Add(new ListItem("." + str + " file", str));
			}
		}
		#endregion

	}
}