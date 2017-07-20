using Umbraco.Core.Services;
using System;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Web.UI;
using Umbraco.Web;
using Umbraco.Web.UI.Controls;
using Umbraco.Web._Legacy.UI;

namespace umbraco.presentation.umbraco.create
{
    public partial class script : UmbracoUserControl
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            sbmt.Text = Services.TextService.Localize("create");

            // Enable new item in folders to place items in that folder.
            if (Request["nodeType"] == "scriptsFolder")
                rename.Text = Request["nodeId"].EnsureEndsWith('/');
        }

        protected void SubmitClick(object sender, System.EventArgs e)
        {
            int createFolder = 0;
            if (scriptType.SelectedValue == "")
            {
                createFolder = 1;
                ContainsValidator.Enabled = true;
                Page.Validate();
            }

            if (Page.IsValid)
            {
                string returnUrl = LegacyDialogHandler.Create(
                    new HttpContextWrapper(Context),
                    Security.CurrentUser,
                    Request.GetItemAsString("nodeType"),
                    createFolder,
                    rename.Text + '\u00A4' + scriptType.SelectedValue);

                ClientTools
                    .ChangeContentFrameUrl(returnUrl)
                    .ReloadActionNode(false, true)
                    .CloseModalWindow();

            }
        }

        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);

            ContainsValidator.Enabled = false;

            string[] fileTypes = UmbracoConfig.For.UmbracoSettings().Content.ScriptFileTypes.ToArray();

            scriptType.Items.Add(new ListItem(Services.TextService.Localize("folder"), ""));
            scriptType.Items.FindByText(Services.TextService.Localize("folder")).Selected = true;

            foreach (string str in fileTypes)
            {
                scriptType.Items.Add(new ListItem("." + str + " file", str));
            }
        }

        /// <summary>
        /// rename control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox rename;

        /// <summary>
        /// RequiredFieldValidator1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.RequiredFieldValidator RequiredFieldValidator1;
        protected global::System.Web.UI.WebControls.RegularExpressionValidator EndsWithValidator;
        protected global::System.Web.UI.WebControls.RegularExpressionValidator ContainsValidator;

        /// <summary>
        /// scriptType control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.ListBox scriptType;

        /// <summary>
        /// CreateMacroCheckBox control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.CheckBox CreateMacroCheckBox;

        /// <summary>
        /// sbmt control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Button sbmt;

    }
}
