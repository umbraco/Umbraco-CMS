using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco;
using umbraco.BasePages;
using umbraco.presentation.create;
using UmbracoSettings = Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Web.UI.Umbraco.Create
{
	public partial class PartialViewMacro : System.Web.UI.UserControl
	{
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DataBind();
		}

		protected void SubmitButton_Click(object sender, System.EventArgs e)
		{
			if (Page.IsValid)
			{			
				//Seriously, need to overhaul create dialogs, this is rediculous:
				// http://issues.umbraco.org/issue/U4-1373

				var createMacroVal = 0;
				if (CreateMacroCheckBox.Checked)
					createMacroVal = 1;
				
				var returnUrl = dialogHandler_temp.Create(
					Request.GetItemAsString("nodeType"),
					createMacroVal, //apparently we need to pass this value to 'ParentID'... of course! :P then we'll extract it in PartialViewTasks to create it.
					FileName.Text);
				
				BasePage.Current.ClientTools
					.ChangeContentFrameUrl(returnUrl)
					.ChildNodeCreated()
					.CloseModalWindow();				
			}

		}
	}
}