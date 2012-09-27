using System;
using System.Collections.Generic;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.skinning;
using umbraco.cms.businesslogic.template;
using umbraco.cms.presentation.Trees;
using umbraco.DataLayer;
using umbraco.IO;
using umbraco.uicontrols;

namespace umbraco.cms.presentation.settings.views
{
    public partial class editView : BasePages.UmbracoEnsuredPage
    {
        private Template _template;

        protected global::ClientDependency.Core.Controls.CssInclude CssInclude1;
        protected global::ClientDependency.Core.Controls.JsInclude JsInclude;

        protected global::umbraco.uicontrols.UmbracoPanel Panel1;
        protected global::umbraco.uicontrols.Pane Pane7;
        protected global::umbraco.uicontrols.PropertyPanel pp_name;
        protected global::System.Web.UI.WebControls.TextBox NameTxt;
        protected global::umbraco.uicontrols.PropertyPanel pp_alias;
        protected global::System.Web.UI.WebControls.TextBox AliasTxt;
        protected global::umbraco.uicontrols.PropertyPanel pp_masterTemplate;
        protected global::System.Web.UI.WebControls.DropDownList MasterTemplate;
        protected global::umbraco.uicontrols.PropertyPanel pp_source;
        protected global::umbraco.uicontrols.CodeArea editorSource;
        protected global::System.Web.UI.WebControls.Repeater rpt_codeTemplates;
        protected global::System.Web.UI.WebControls.Repeater rpt_macros;


        public editView()
        {
            CurrentApp = BusinessLogic.DefaultApps.settings.ToString();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            MasterTemplate.Attributes.Add("onchange", "changeMasterPageFile()");

            if (!IsPostBack)
            {
                MasterTemplate.Items.Add(new ListItem(ui.Text("none"), "0"));

                foreach (Template t in Template.GetAllAsList())
                {
                    if (t.Id != _template.Id)
                    {
                        var li = new ListItem(t.Text, t.Id.ToString());

                        li.Attributes.Add("id", t.Alias.Replace(" ", ""));

                        if (t.Id == _template.MasterTemplate)
                        {
                            try
                            {
                                li.Selected = true;
                            }
                            catch
                            {
                            }
                        }
                        MasterTemplate.Items.Add(li);
                    }
                }

                NameTxt.Text = _template.GetRawText();
                AliasTxt.Text = _template.Alias;
                editorSource.Text = _template.Design;


                ClientTools
                    .SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadTemplates>().Tree.Alias)
                    .SyncTree("-1,init," + _template.Path.Replace("-1,", ""), false);
            }
        }



        #region Web Form Designer generated code
        protected override void OnInit(EventArgs e)
        {
            _template = new Template(int.Parse(Request.QueryString["templateID"]));
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
            base.OnInit(e);
            Panel1.hasMenu = true;

            MenuIconI save = Panel1.Menu.NewIcon();
            save.ImageURL = SystemDirectories.Umbraco + "/images/editor/save.gif";
            save.OnClickCommand = "doSubmit()";
            save.AltText = ui.Text("save");
            save.ID = "save";

            Panel1.Text = ui.Text("edittemplate");
            pp_name.Text = ui.Text("name", base.getUser());
            pp_alias.Text = ui.Text("alias", base.getUser());
            pp_masterTemplate.Text = ui.Text("mastertemplate", base.getUser());

            // Editing buttons
            Panel1.Menu.InsertSplitter();
            MenuIconI umbField = Panel1.Menu.NewIcon();
            umbField.ImageURL = UmbracoPath + "/images/editor/insField.gif";
            umbField.OnClickCommand =
                ClientTools.Scripts.OpenModalWindow(
                    IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/dialogs/umbracoField.aspx?objectId=" +
                    editorSource.ClientID + "&tagName=UMBRACOGETDATA", ui.Text("template", "insertPageField"), 640, 550);
            umbField.AltText = ui.Text("template", "insertPageField");


            // TODO: Update icon
            MenuIconI umbDictionary = Panel1.Menu.NewIcon();
            umbDictionary.ImageURL = GlobalSettings.Path + "/images/editor/dictionaryItem.gif";
            umbDictionary.OnClickCommand =
                ClientTools.Scripts.OpenModalWindow(
                    IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/dialogs/umbracoField.aspx?objectId=" +
                    editorSource.ClientID + "&tagName=UMBRACOGETDICTIONARY", ui.Text("template", "insertDictionaryItem"),
                    640, 550);
            umbDictionary.AltText = "Insert umbraco dictionary item";

            //uicontrols.MenuIconI umbMacro = Panel1.Menu.NewIcon();
            //umbMacro.ImageURL = UmbracoPath + "/images/editor/insMacro.gif";
            //umbMacro.AltText = ui.Text("template", "insertMacro");
            //umbMacro.OnClickCommand = umbraco.BasePages.ClientTools.Scripts.OpenModalWindow(umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco) + "/dialogs/editMacro.aspx?objectId=" + editorSource.ClientID, ui.Text("template", "insertMacro"), 470, 530);

            Panel1.Menu.NewElement("div", "splitButtonMacroPlaceHolder", "sbPlaceHolder", 40);

            if (UmbracoSettings.UseAspNetMasterPages)
            {
                Panel1.Menu.InsertSplitter();

                MenuIconI umbContainer = Panel1.Menu.NewIcon();
                umbContainer.ImageURL = UmbracoPath + "/images/editor/masterpagePlaceHolder.gif";
                umbContainer.AltText = ui.Text("template", "insertContentAreaPlaceHolder");
                umbContainer.OnClickCommand =
                    ClientTools.Scripts.OpenModalWindow(
                        IOHelper.ResolveUrl(SystemDirectories.Umbraco) +
                        "/dialogs/insertMasterpagePlaceholder.aspx?&id=" + _template.Id,
                        ui.Text("template", "insertContentAreaPlaceHolder"), 470, 320);

                MenuIconI umbContent = Panel1.Menu.NewIcon();
                umbContent.ImageURL = UmbracoPath + "/images/editor/masterpageContent.gif";
                umbContent.AltText = ui.Text("template", "insertContentArea");
                umbContent.OnClickCommand =
                    ClientTools.Scripts.OpenModalWindow(
                        IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/dialogs/insertMasterpageContent.aspx?id=" +
                        _template.Id, ui.Text("template", "insertContentArea"), 470, 300);
            }


            //Spit button
            Panel1.Menu.InsertSplitter();
            Panel1.Menu.NewElement("div", "splitButtonPlaceHolder", "sbPlaceHolder", 40);

            // Help
            Panel1.Menu.InsertSplitter();

            MenuIconI helpIcon = Panel1.Menu.NewIcon();
            helpIcon.OnClickCommand =
                ClientTools.Scripts.OpenModalWindow(
                    IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/settings/modals/showumbracotags.aspx?alias=" +
                    _template.Alias, ui.Text("template", "quickGuide"), 600, 580);
            helpIcon.ImageURL = UmbracoPath + "/images/editor/help.png";
            helpIcon.AltText = ui.Text("template", "quickGuide");
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {

        }



        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/codeEditorSave.asmx"));
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/legacyAjaxCalls.asmx"));
        }
        #endregion

    }
}
