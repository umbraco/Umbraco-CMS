using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.BasePages;
using Umbraco.Core;
using Umbraco.Core.IO;
using umbraco.presentation.create;

namespace Umbraco.Web.UI.Umbraco.Create
{
	public partial class PartialView : UserControl
	{
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DataBind();

		    LoadTemplates(PartialViewTemplate);
		}

        private static void LoadTemplates(ListControl list)
        {
            var partialViewTemplatePath = new
            {
                path = IOHelper.MapPath(SystemDirectories.Umbraco + "/PartialViews/Templates/"),
                include = new string[]{}
            };
            //include these templates from the partial view macro templates!
            var partialViewMacrosTemplatePath = new
            {
                path = IOHelper.MapPath(SystemDirectories.Umbraco + "/PartialViewMacros/Templates/"),
                include = new[]
                {
                    "Breadcrumb", 
                    "EditProfile", 
                    "Empty (ForUseWithCustomViews)", 
                    "Empty", 
                    "ListAncestorsFromCurrentPage",
                    "ListChildPagesFromCurrentPage",
                    "ListChildPagesOrderedByDate",
                    "ListChildPagesOrderedByName",
                    "ListChildPagesWithDoctype",
                    "ListDescendantsFromCurrentPage",
                    "Login",
                    "LoginStatus",
                    "MultinodeTree-picker",
                    "Navigation",
                    "RegisterMember",
                    "SiteMap"
                }
            };
            var paths = new[] { partialViewTemplatePath, partialViewMacrosTemplatePath };
            const string extension = ".cshtml";
            var namesAdded = new List<string>();

            list.Items.Clear();

            // always add the options of empty snippets
            list.Items.Add(new ListItem("Empty", "Empty.cshtml"));
            list.Items.Add(new ListItem("Empty (For Use With Custom Views)", "Empty (ForUseWithCustomViews).cshtml"));
            
            foreach (var pathFilter in paths)
            {
                if (Directory.Exists(pathFilter.path) == false) continue;
                
                var p = pathFilter;
                foreach (var fileInfo in new DirectoryInfo(pathFilter.path).GetFiles("*" + extension)
                    //check if we've already added this name
                    .Where(f => namesAdded.InvariantContains(f.Name) == false)
                    //Already adding Empty as the first item, so don't add it again
                    .Where(f => f.Name.StartsWith("Empty") == false)
                    //don't add if not in the inclusion list
                    .Where(f => p.include.Length == 0 || (p.include.Length > 0 && p.include.InvariantContains(f.Name) == false)))
                {
                    var filename = Path.GetFileName(fileInfo.FullName);
                    namesAdded.Add(filename);
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