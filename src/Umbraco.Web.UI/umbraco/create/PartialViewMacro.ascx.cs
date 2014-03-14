using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.BasePages;
using Umbraco.Core;
using Umbraco.Core.IO;
using umbraco.presentation.create;

namespace Umbraco.Web.UI.Umbraco.Create
{
	public partial class PartialViewMacro : UserControl
	{
        

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DataBind();

		    LoadTemplates(PartialViewTemplate);
		}

        private static void LoadTemplates(ListControl list)
        {
            var path = IOHelper.MapPath(SystemDirectories.Umbraco + "/PartialViewMacros/Templates/");
            list.Items.Clear();

            // always add the options of empty snippets
            list.Items.Add(new ListItem("Empty", "Empty.cshtml"));
            list.Items.Add(new ListItem("Empty (For Use With Custom Views)", "Empty (ForUseWithCustomViews).cshtml"));

            if (System.IO.Directory.Exists(path))
            {
                const string extension = ".cshtml";

                //Already adding Empty as the first item, so don't add it again
                foreach (var fileInfo in new System.IO.DirectoryInfo(path).GetFiles("*" + extension).Where(f => f.Name.StartsWith("Empty") == false))
                {
                    var filename = System.IO.Path.GetFileName(fileInfo.FullName);

                    var liText = filename.Replace(extension, "").SplitPascalCasing().ToFirstUpperInvariant();
                    list.Items.Add(new ListItem(liText, filename));
                }
            }
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
                
                string returnUrl = dialogHandler_temp.Create(Request.GetItemAsString("nodeType"),
                    createMacroVal, //apparently we need to pass this value to 'ParentID'... of course! :P then we'll extract it in PartialViewTasks to create it.
                    PartialViewTemplate.SelectedValue + "|||" + FileName.Text);
				
				BasePage.Current.ClientTools
					.ChangeContentFrameUrl(returnUrl)
					.ChildNodeCreated()
					.CloseModalWindow();				
			}
		}
	}
}