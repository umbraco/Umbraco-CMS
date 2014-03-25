using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Web.UI;
using umbraco.cms.businesslogic.macro;
using umbraco.scripting;
using umbraco.BasePages;

namespace umbraco.presentation.create
{
    public partial class DLRScripting : System.Web.UI.UserControl
    {
        protected System.Web.UI.WebControls.ListBox nodeType;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            sbmt.Text = ui.Text("create");
            if (!Page.IsPostBack)
            {
                foreach (MacroEngineLanguage lang in MacroEngineFactory.GetSupportedUILanguages())
                {
                    filetype.Items.Add(new ListItem(string.Format(".{0} ({1})", lang.Extension.ToLowerInvariant(), lang.EngineName), lang.Extension));
                }
                filetype.SelectedIndex = 0;
            }
            LoadTemplates(template, filetype.SelectedValue);
        }

        protected void MacroExistsValidator_OnServerValidate(object source, ServerValidateEventArgs args)
        {
            if (createMacro.Checked)
            {
                //TODO: Shouldn't this use our string functions to create the alias ?
                var fileName = rename.Text + "." + filetype.SelectedValue;
                var name = fileName
                    .Substring(0, (fileName.LastIndexOf('.') + 1)).Trim('.')
                    .SplitPascalCasing().ToFirstUpperInvariant();

                var macro = ApplicationContext.Current.Services.MacroService.GetByAlias(name);
                if (macro != null)
                {
                    args.IsValid = false;
                }
            }
        }

        protected void sbmt_Click(object sender, System.EventArgs e)
        {
            if (Page.IsValid)
            {
                var createMacroVal = 0;
                if (createMacro.Checked)
                    createMacroVal = 1;

                var returnUrl = LegacyDialogHandler.Create(
                    new HttpContextWrapper(Context),
                    BasePage.Current.getUser(),
                    helper.Request("nodeType"),
                    createMacroVal,
                    template.SelectedValue + "|||" + rename.Text + "." + filetype.SelectedValue);

                BasePage.Current.ClientTools
                    .ChangeContentFrameUrl(returnUrl)
                    .ChildNodeCreated()
                    .CloseModalWindow();
            }
        }

        public void loadTemplates(object sender, EventArgs e)
        {
            LoadTemplates(template, filetype.SelectedValue);
        }

        private void LoadTemplates(ListBox list, string scriptType)
        {
            string path = SystemDirectories.Umbraco + "/scripting/templates/" + scriptType + "/";
            string abPath = IOHelper.MapPath(path);
            list.Items.Clear();

            // always add the option of an empty one
            list.Items.Add(scriptType == "cshtml"
                               ? new ListItem("Empty template", "cshtml/EmptyTemplate.cshtml")
                               : new ListItem("Empty template", ""));

            if (System.IO.Directory.Exists(abPath))
            {
                string extension = "." + scriptType;

                //Already adding Empty Template as the first item, so don't add it again
                foreach (System.IO.FileInfo fi in new System.IO.DirectoryInfo(abPath).GetFiles("*" + extension).Where(fi => fi.Name != "EmptyTemplate.cshtml"))
                {
                    string filename = System.IO.Path.GetFileName(fi.FullName);

                    var liText = filename.Replace(extension, "").SplitPascalCasing().ToFirstUpperInvariant();
                    list.Items.Add(new ListItem(liText, scriptType + "/" + filename));
                }
            }
        }

        protected global::System.Web.UI.WebControls.CustomValidator MacroExistsValidator;

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

        /// <summary>
        /// UpdatePanel1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.UpdatePanel UpdatePanel1;

        /// <summary>
        /// filetype control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.ListBox filetype;

        /// <summary>
        /// template control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.ListBox template;

        /// <summary>
        /// createMacro control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.CheckBox createMacro;

        /// <summary>
        /// sbmt control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Button sbmt;

        /// <summary>
        /// Textbox1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox Textbox1;
    }
}