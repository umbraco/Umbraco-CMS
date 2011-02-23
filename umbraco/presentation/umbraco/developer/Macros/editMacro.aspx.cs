using System;
using System.Collections;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.IO;

using umbraco.BasePages;
using umbraco.presentation.cache;
using umbraco.uicontrols;
using umbraco.DataLayer;
using umbraco.cms.presentation.Trees;
using umbraco.cms.businesslogic.macro;
using umbraco.IO;

namespace umbraco.cms.presentation.developer
{
    /// <summary>
    /// Summary description for editMacro.
    /// </summary>
    public partial class editMacro : UmbracoEnsuredPage
    {
        protected PlaceHolder buttons;
        protected Table macroElements;
        protected Macro m_macro;

        public TabPage InfoTabPage;
        public TabPage Parameters;

        protected void Page_Load(object sender, EventArgs e)
        {
            m_macro = new Macro(Convert.ToInt32(Request.QueryString["macroID"]));

            if (!IsPostBack)
            {

                ClientTools
					.SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadMacros>().Tree.Alias)
					.SyncTree(m_macro.Id.ToString(), false);

                macroName.Text = m_macro.Name;
                macroAlias.Text = m_macro.Alias;
                string tempMacroAssembly = m_macro.Assembly == null ? "" : m_macro.Assembly;
                string tempMacroType = m_macro.Type == null ? "" : m_macro.Type;
                macroXslt.Text = m_macro.Xslt;
                macroPython.Text = m_macro.ScriptingFile;
                cachePeriod.Text = m_macro.RefreshRate.ToString();

                macroRenderContent.Checked = m_macro.RenderContent;
                macroEditor.Checked = m_macro.UseInEditor;
                cacheByPage.Checked = m_macro.CacheByPage;
                cachePersonalized.Checked = m_macro.CachePersonalized;

                // Populate either user control or custom control
                if (tempMacroType != string.Empty && tempMacroAssembly != string.Empty)
                {
                    macroAssembly.Text = tempMacroAssembly;
                    macroType.Text = tempMacroType;
                }
                else
                {
                    macroUserControl.Text = tempMacroType;
                }

                // Check for assemblyBrowser
                if (tempMacroType.IndexOf(".ascx") > 0)
                    assemblyBrowserUserControl.Controls.Add(
                        new LiteralControl("<br/><button onClick=\"UmbClientMgr.openModalWindow('" + umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco) + "/developer/macros/assemblyBrowser.aspx?fileName=" + macroUserControl.Text +
                                           "&macroID=" + m_macro.Id.ToString() +
                                           "', 'Browse Properties', true, 475,500); return false;\" class=\"guiInputButton\"><img src=\"../../images/editor/propertiesNew.gif\" align=\"absmiddle\" style=\"width: 18px; height: 17px; padding-right: 5px;\"/> Browse properties</button>"));
                else if (tempMacroType != string.Empty && tempMacroAssembly != string.Empty)
                    assemblyBrowser.Controls.Add(
                        new LiteralControl("<br/><button onClick=\"UmbClientMgr.openModalWindow('" + umbraco.IO.IOHelper.ResolveUrl(umbraco.IO.SystemDirectories.Umbraco) + "/developer/macros/assemblyBrowser.aspx?fileName=" + macroAssembly.Text +
                                           "&macroID=" + m_macro.Id.ToString() + "&type=" + macroType.Text +
                                           "', 'Browse Properties', true, 475,500); return false\" class=\"guiInputButton\"><img src=\"../../images/editor/propertiesNew.gif\" align=\"absmiddle\" style=\"width: 18px; height: 17px; padding-right: 5px;\"/> Browse properties</button>"));

                // Load elements from macro
                macroPropertyBind();

                // Load xslt files from default dir
                populateXsltFiles();

                // Load python files from default dir
                populatePythonFiles();

                // Load usercontrols
                populateUserControls(IOHelper.MapPath(SystemDirectories.Usercontrols) );
                userControlList.Items.Insert(0, new ListItem("Browse usercontrols on server...", string.Empty));
                userControlList.Attributes.Add("onChange",
                    "document.getElementById('" + macroUserControl.ClientID + "').value = this[this.selectedIndex].value;");


            }
            else
            {
                int macroID = Convert.ToInt32(Request.QueryString["macroID"]);
                string tempMacroAssembly = macroAssembly.Text;
                string tempMacroType = macroType.Text;
                string tempCachePeriod = cachePeriod.Text;

                if (tempCachePeriod == string.Empty)
                    tempCachePeriod = "0";

                if (tempMacroAssembly == string.Empty && macroUserControl.Text != string.Empty)
                    tempMacroType = macroUserControl.Text;

                // Save macro
                m_macro.UseInEditor = macroEditor.Checked;
                m_macro.RenderContent = macroRenderContent.Checked;
                m_macro.CacheByPage = cacheByPage.Checked;
                m_macro.CachePersonalized = cachePersonalized.Checked;
                m_macro.RefreshRate = Convert.ToInt32(tempCachePeriod);
                m_macro.Alias = macroAlias.Text;
                m_macro.Name = macroName.Text;
                m_macro.Assembly = tempMacroAssembly;
                m_macro.Type = tempMacroType;
                m_macro.Xslt = macroXslt.Text;
                m_macro.ScriptingFile = macroPython.Text;
                m_macro.Save();

                // Save elements
                foreach (RepeaterItem item in macroProperties.Items)
                {
                    HtmlInputHidden macroPropertyID = (HtmlInputHidden)item.FindControl("macroPropertyID");
                    TextBox macroElementName = (TextBox)item.FindControl("macroPropertyName");
                    TextBox macroElementAlias = (TextBox)item.FindControl("macroPropertyAlias");
                    CheckBox macroElementShow = (CheckBox)item.FindControl("macroPropertyHidden");
                    DropDownList macroElementType = (DropDownList)item.FindControl("macroPropertyType");

                    MacroProperty mp = new MacroProperty(int.Parse(macroPropertyID.Value));
                    mp.Public = macroElementShow.Checked;
                    mp.Type = new MacroPropertyType(int.Parse(macroElementType.SelectedValue));
                    mp.Alias = macroElementAlias.Text;
                    mp.Name = macroElementName.Text;
                    mp.Save();

                }
                // Flush macro from cache!
                if (UmbracoSettings.UseDistributedCalls)
                    dispatcher.Refresh(
                        new Guid("7B1E683C-5F34-43dd-803D-9699EA1E98CA"),
                        macroID);
                else
                    new macro(macroID).removeFromCache();

                // Check for assemblyBrowser
                if (tempMacroType.IndexOf(".ascx") > 0)
                    assemblyBrowserUserControl.Controls.Add(
                        new LiteralControl("<br/><button onClick=\"UmbClientMgr.openModalWindow('developer/macros/assemblyBrowser.aspx?fileName=" + macroUserControl.Text +
                            "&macroID=" + Request.QueryString["macroID"] +
                                "', 'Browse Properties', true, 500, 475); return false\" class=\"guiInputButton\"><img src=\"../../images/editor/propertiesNew.gif\" align=\"absmiddle\" style=\"width: 18px; height: 17px; padding-right: 5px;\"/> Browse properties</button>"));
                else if (tempMacroType != string.Empty && tempMacroAssembly != string.Empty)
                    assemblyBrowser.Controls.Add(
                        new LiteralControl("<br/><button onClick=\"UmbClientMgr.openModalWindow('developer/macros/assemblyBrowser.aspx?fileName=" + macroAssembly.Text +
                            "&macroID=" + Request.QueryString["macroID"] + "&type=" + macroType.Text +
                                "', 'Browse Properties', true, 500, 475); return false\" class=\"guiInputButton\"><img src=\"../../images/editor/propertiesNew.gif\" align=\"absmiddle\" style=\"width: 18px; height: 17px; padding-right: 5px;\"/> Browse properties</button>"));
            }
        }

        private void getXsltFilesFromDir(string orgPath, string path, ArrayList files)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);

            // Populate subdirectories
            DirectoryInfo[] dirInfos = dirInfo.GetDirectories();
            foreach (DirectoryInfo dir in dirInfos)
                getXsltFilesFromDir(orgPath, path + "/" + dir.Name, files);

            FileInfo[] fileInfo = dirInfo.GetFiles("*.xsl*");

            foreach (FileInfo file in fileInfo)
                files.Add((path.Replace(orgPath, string.Empty).Trim('/') + "/" + file.Name).Trim('/'));
        }

        private void populateXsltFiles()
        {
            ArrayList xslts = new ArrayList();
            string xsltDir = IOHelper.MapPath(SystemDirectories.Xslt + "/");
            getXsltFilesFromDir(xsltDir, xsltDir, xslts);
            xsltFiles.DataSource = xslts;
            xsltFiles.DataBind();
            xsltFiles.Items.Insert(0, new ListItem("Browse xslt files on server...", string.Empty));
            xsltFiles.Attributes.Add("onChange",
                "document.getElementById('" + macroXslt.ClientID + "').value = this[this.selectedIndex].value; document.getElementById('" + macroPython.ClientID + "').value =''");
        }

        private void getPythonFilesFromDir(string orgPath, string path, ArrayList files)
        {
            var dirInfo = new DirectoryInfo(path);
            if (!dirInfo.Exists)
                return;

            FileInfo[] fileInfo = dirInfo.GetFiles("*.*");
            foreach (FileInfo file in fileInfo)
                files.Add(path.Replace(orgPath, string.Empty) + file.Name);

            // Populate subdirectories
            var dirInfos = dirInfo.GetDirectories();
            foreach (var dir in dirInfos)
                getPythonFilesFromDir(orgPath, path + "/" + dir.Name + "/", files);
        }

        private void populatePythonFiles()
        {
            ArrayList pythons = new ArrayList();
            string pythonDir = IOHelper.MapPath(SystemDirectories.Python + "/");
            getPythonFilesFromDir(pythonDir, pythonDir, pythons);
            pythonFiles.DataSource = pythons;
            pythonFiles.DataBind();
            pythonFiles.Items.Insert(0, new ListItem("Browse scripting files on server...", string.Empty));
            pythonFiles.Attributes.Add("onChange",
                "document.getElementById('" + macroPython.ClientID + "').value = this[this.selectedIndex].value; document.getElementById('" + macroXslt.ClientID + "').value = ''");
        }

        public void deleteMacroProperty(object sender, EventArgs e)
        {
            HtmlInputHidden macroPropertyID = (HtmlInputHidden)((Control)sender).Parent.FindControl("macroPropertyID");
            SqlHelper.ExecuteNonQuery("delete from cmsMacroProperty where id = @id", SqlHelper.CreateParameter("@id", macroPropertyID.Value));

            macroPropertyBind();
        }

        public void macroPropertyBind()
        {
            macroProperties.DataSource = m_macro.Properties;
            macroProperties.DataBind();
        }

        public object CheckNull(object test)
        {
            if (Convert.IsDBNull(test))
                return 0;
            else
                return test;
        }

        public IRecordsReader GetMacroPropertyTypes()
        {
            // Load dataChildTypes	
            return SqlHelper.ExecuteReader("select id, macroPropertyTypeAlias from cmsMacroPropertyType order by macroPropertyTypeAlias");
        }

        public void macroPropertyCreate(object sender, EventArgs e)
        {
            CheckBox macroPropertyHiddenNew = (CheckBox)((Control)sender).Parent.FindControl("macroPropertyHiddenNew");
            TextBox macroPropertyAliasNew = (TextBox)((Control)sender).Parent.FindControl("macroPropertyAliasNew");
            TextBox macroPropertyNameNew = (TextBox)((Control)sender).Parent.FindControl("macroPropertyNameNew");
            DropDownList macroPropertyTypeNew = (DropDownList)((Control)sender).Parent.FindControl("macroPropertyTypeNew");
            bool _goAhead = true;
            if (macroPropertyAliasNew.Text !=
                ui.Text("general", "new", this.getUser()) + " " + ui.Text("general", "alias", this.getUser()))
            {


                foreach (cms.businesslogic.macro.MacroProperty mp in m_macro.Properties)
                {
                    if (mp.Alias == macroPropertyAliasNew.Text)
                    {
                        _goAhead = false;
                        break;

                    }
                }

                if (_goAhead)
                {
                    MacroProperty mp = new MacroProperty();
                    mp.Macro = m_macro;
                    mp.Public = macroPropertyHiddenNew.Checked;
                    mp.Type = new MacroPropertyType(int.Parse(macroPropertyTypeNew.SelectedValue));
                    mp.Alias = macroPropertyAliasNew.Text;
                    mp.Name = macroPropertyNameNew.Text;
                    mp.Save();

                    m_macro.RefreshProperties();
                    macroPropertyBind();
                }
            }
        }

        public bool macroIsVisible(object IsChecked)
        {
            if (Convert.ToBoolean(IsChecked))
                return true;
            else
                return false;
        }

        public void AddChooseList(Object Sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                DropDownList dropDown = (DropDownList)Sender;
                dropDown.Items.Insert(0, new ListItem("Choose...", string.Empty));
            }
        }

        private void populateUserControls(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);

            string rootDir = IOHelper.MapPath( SystemDirectories.Root );
            
            foreach (FileInfo uc in di.GetFiles("*.ascx"))
            {
                userControlList.Items.Add(
                    new ListItem( 
                            uc.FullName.Substring(rootDir.Length).Replace(IOHelper.DirSepChar, '/')));
                /*
                                        uc.FullName.IndexOf(usercontrolsDir), 
                                        uc.FullName.Length - uc.FullName.IndexOf(usercontrolsDir)).Replace(IOHelper.DirSepChar, '/')));
                */

            }
            foreach (DirectoryInfo dir in di.GetDirectories())
                populateUserControls(dir.FullName);
        }

        #region Web Form Designer generated code

        protected override void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();

            // Tab setup
            InfoTabPage = TabView1.NewTabPage("Macro Properties");
            InfoTabPage.Controls.Add(Pane1);
            InfoTabPage.Controls.Add(Pane1_2);
            InfoTabPage.Controls.Add(Pane1_3);
            InfoTabPage.Controls.Add(Pane1_4);

            Parameters = TabView1.NewTabPage("Parameters");
            Parameters.Controls.Add(Panel2);

            ImageButton save = InfoTabPage.Menu.NewImageButton();
            save.ImageUrl = SystemDirectories.Umbraco + "/images/editor/save.gif";

            ImageButton save2 = Parameters.Menu.NewImageButton();
            save2.ImageUrl = SystemDirectories.Umbraco + "/images/editor/save.gif";

            base.OnInit(e);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        }

        #endregion
    }
 
}
