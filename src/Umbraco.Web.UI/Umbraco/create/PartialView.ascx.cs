using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.BasePages;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Services;
using umbraco.presentation.create;

namespace Umbraco.Web.UI.Umbraco.Create
{
	public partial class PartialView : UI.Controls.UmbracoUserControl
	{
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DataBind();

		    LoadTemplates(PartialViewTemplate);

            // Enable new item in folders to place items in that folder.
		    if (Request["nodeType"] == "partialViewsFolder")
		        FileName.Text = Request["nodeId"].EnsureEndsWith('/');
		}

        private void LoadTemplates(ListControl list)
        {
            var fileService = (FileService)Services.FileService;
            var snippets = fileService.GetPartialViewSnippetNames(
                //ignore these
                "Gallery", 
                "ListChildPagesFromChangeableSource", 
                "ListChildPagesOrderedByProperty",
                "ListImagesFromMediaFolder");

            foreach (var snippet in snippets)
            {
                var liText = snippet.SplitPascalCasing().ToFirstUpperInvariant();
                list.Items.Add(new ListItem(liText, snippet));
            }
        }

		protected void SubmitButton_Click(object sender, System.EventArgs e)
		{
			if (Page.IsValid)
			{			
				//Seriously, need to overhaul create dialogs, this is rediculous:
				// http://issues.umbraco.org/issue/U4-1373

				var createMacroVal = 0;

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