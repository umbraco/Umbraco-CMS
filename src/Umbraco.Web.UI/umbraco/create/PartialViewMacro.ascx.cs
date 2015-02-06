using System;
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
    public partial class PartialViewMacro : UI.Controls.UmbracoUserControl
    {

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            DataBind();

            LoadTemplates(PartialViewTemplate);

            // Enable new contect item in folders to place items in that folder.
            if (Request["nodeType"] == "partialViewMacrosFolder")
                FileName.Text = Request["nodeId"].EnsureEndsWith('/');
        }

        private void LoadTemplates(ListControl list)
        {
            var fileService = (FileService)Services.FileService;
            var snippets = fileService.GetPartialViewSnippetNames();
            foreach (var snippet in snippets)
            {
                var liText = snippet.SplitPascalCasing().ToFirstUpperInvariant();
                list.Items.Add(new ListItem(liText, snippet));
            }
        }

        protected void SubmitButton_Click(object sender, EventArgs e)
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

        protected void MacroExistsValidator_OnServerValidate(object source, ServerValidateEventArgs args)
        {
            if (CreateMacroCheckBox.Checked)
            {
                //TODO: Shouldn't this use our string functions to create the alias ?
                var fileName = FileName.Text;

                var name = fileName.Contains(".")
                    ? fileName.Substring(0, (fileName.LastIndexOf('.') + 1)).Trim('.')
                    : fileName;
                
                name = name.SplitPascalCasing().ToFirstUpperInvariant();

                var macro = ApplicationContext.Current.Services.MacroService.GetByAlias(name);
                if (macro != null)
                {
                    args.IsValid = false;
                }
            }
        }
    }
}