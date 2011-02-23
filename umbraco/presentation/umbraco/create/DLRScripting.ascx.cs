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
            if (!Page.IsPostBack) {
                foreach (MacroEngineLanguage lang in MacroEngineFactory.GetSupportedUILanguages()) {
                    filetype.Items.Add(new ListItem(string.Format("{0} by {1}", helper.SpaceCamelCasing(lang.Extension), lang.EngineName), lang.Extension));
                }
                filetype.SelectedIndex = 0;
            }
            _loadTemplates(template, filetype.SelectedValue);
        }

        protected void sbmt_Click(object sender, System.EventArgs e)
        {
            if (Page.IsValid)
            {
                int createMacroVal = 0;
                if (createMacro.Checked)
                    createMacroVal = 1;

                string returnUrl = dialogHandler_temp.Create(UmbracoContext.Current.Request["nodeType"],
                    createMacroVal, template.SelectedValue + "|||" + rename.Text + "." + filetype.SelectedValue);

                BasePage.Current.ClientTools
                    .ChangeContentFrameUrl(returnUrl)
                    .ChildNodeCreated()
                    .CloseModalWindow();
            }
        }

        public void loadTemplates(object sender, EventArgs e)
        {
            _loadTemplates(template, filetype.SelectedValue);
        }

        private void _loadTemplates(ListBox list, string scriptType)
        {
            string path = IO.SystemDirectories.Umbraco + "/scripting/templates/" + scriptType + "/";
            string abPath = IO.IOHelper.MapPath(path);
            list.Items.Clear();

            if (System.IO.Directory.Exists(abPath))
            {
                string extension = "." + scriptType;

                foreach (System.IO.FileInfo fi in new System.IO.DirectoryInfo(abPath).GetFiles("*" + extension))
                {
                    string filename = System.IO.Path.GetFileName(fi.FullName);

                    list.Items.Add(new ListItem(helper.SpaceCamelCasing(filename.Replace(extension, "")), scriptType + "/" + filename));
                }
            }
            else
            {
                list.Items.Add(new ListItem("Empty template", ""));
            }
        }
    }
}