using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.IO;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using umbraco.BasePages;
using umbraco.uicontrols;
using umbraco.DataLayer;
using umbraco.cms.presentation.Trees;
using System.Linq;

namespace umbraco.cms.presentation.developer
{
	/// <summary>
	/// Summary description for editMacro.
	/// </summary>
	public partial class editMacro : UmbracoEnsuredPage
	{
		public editMacro()
		{
			CurrentApp = BusinessLogic.DefaultApps.developer.ToString();
		}

		protected PlaceHolder Buttons;
		protected Table MacroElements;

		public TabPage InfoTabPage;
		public TabPage Parameters;

	    private IMacro _macro;

		protected void Page_Load(object sender, EventArgs e)
		{
		    _macro = Services.MacroService.GetById(Convert.ToInt32(Request.QueryString["macroID"]));

			if (IsPostBack == false)
			{
				ClientTools
					.SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadMacros>().Tree.Alias)
                    .SyncTree("-1,init," + _macro.Id, false);

                string tempMacroAssembly = _macro.ControlAssembly ?? "";
                string tempMacroType = _macro.ControlType ?? "";

                PopulateFieldsOnLoad(_macro, tempMacroAssembly, tempMacroType);

				// Check for assemblyBrowser
				if (tempMacroType.IndexOf(".ascx", StringComparison.Ordinal) > 0)
					assemblyBrowserUserControl.Controls.Add(
						new LiteralControl("<br/><button onClick=\"UmbClientMgr.openModalWindow('" + IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/developer/macros/assemblyBrowser.aspx?fileName=" + macroUserControl.Text +
                                           "&macroID=" + _macro.Id.ToInvariantString() +
										   "', 'Browse Properties', true, 475,500); return false;\" class=\"guiInputButton\"><img src=\"../../images/editor/propertiesNew.gif\" align=\"absmiddle\" style=\"width: 18px; height: 17px; padding-right: 5px;\"/> Browse properties</button>"));
				else if (tempMacroType != string.Empty && tempMacroAssembly != string.Empty)
					assemblyBrowser.Controls.Add(
						new LiteralControl("<br/><button onClick=\"UmbClientMgr.openModalWindow('" + IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/developer/macros/assemblyBrowser.aspx?fileName=" + macroAssembly.Text +
                                           "&macroID=" + _macro.Id.ToInvariantString() + "&type=" + macroType.Text +
										   "', 'Browse Properties', true, 475,500); return false\" class=\"guiInputButton\"><img src=\"../../images/editor/propertiesNew.gif\" align=\"absmiddle\" style=\"width: 18px; height: 17px; padding-right: 5px;\"/> Browse properties</button>"));

				// Load elements from macro
				macroPropertyBind();

				// Load xslt files from default dir
				PopulateXsltFiles();

				// Load razor script files from default dir
                PopulateMacroScriptFiles();

				// Load usercontrols
				PopulateUserControls(IOHelper.MapPath(SystemDirectories.UserControls));
				userControlList.Items.Insert(0, new ListItem("Browse usercontrols on server...", string.Empty));

			}
		}

		/// <summary>
		/// Populates the control (textbox) values on page load
		/// </summary>
		/// <param name="macro"></param>
		/// <param name="macroAssemblyValue"></param>
		/// <param name="macroTypeValue"></param>
		protected virtual void PopulateFieldsOnLoad(IMacro macro, string macroAssemblyValue, string macroTypeValue)
		{
			macroName.Text = macro.Name;
			macroAlias.Text = macro.Alias;
			macroXslt.Text = macro.XsltPath;
			macroPython.Text = macro.ScriptPath;
		    cachePeriod.Text = macro.CacheDuration.ToInvariantString();
			macroRenderContent.Checked = macro.DontRender == false;
			macroEditor.Checked = macro.UseInEditor;
			cacheByPage.Checked = macro.CacheByPage;
			cachePersonalized.Checked = macro.CacheByMember;

			// Populate either user control or custom control
			if (macroTypeValue != string.Empty && macroAssemblyValue != string.Empty)
			{
				macroAssembly.Text = macroAssemblyValue;
				macroType.Text = macroTypeValue;
			}
			else
			{
				macroUserControl.Text = macroTypeValue;
			}
		}

		/// <summary>
		/// Sets the values on the Macro object from the values posted back before saving the macro
		/// </summary>
		protected virtual void SetMacroValuesFromPostBack(IMacro macro, int macroCachePeriod, string macroAssemblyValue, string macroTypeValue)
		{
			macro.UseInEditor = macroEditor.Checked;
			macro.DontRender = macroRenderContent.Checked == false;
			macro.CacheByPage = cacheByPage.Checked;
			macro.CacheByMember = cachePersonalized.Checked;
			macro.CacheDuration = macroCachePeriod;
			macro.Alias = macroAlias.Text;
			macro.Name = macroName.Text;
			macro.ControlAssembly = macroAssemblyValue;
			macro.ControlType = macroTypeValue;
			macro.XsltPath = macroXslt.Text;
			macro.ScriptPath = macroPython.Text;
		}

		private static void GetXsltFilesFromDir(string orgPath, string path, ArrayList files)
		{
			var dirInfo = new DirectoryInfo(path);

		    if (dirInfo.Exists == false) return;

			// Populate subdirectories
			var dirInfos = dirInfo.GetDirectories();
			foreach (var dir in dirInfos)
				GetXsltFilesFromDir(orgPath, path + "/" + dir.Name, files);

			var fileInfo = dirInfo.GetFiles("*.xsl*");

			foreach (var file in fileInfo)
				files.Add((path.Replace(orgPath, string.Empty).Trim('/') + "/" + file.Name).Trim('/'));
		}

		private void PopulateXsltFiles()
		{
			var xslts = new ArrayList();
			var xsltDir = IOHelper.MapPath(SystemDirectories.Xslt + "/");
			GetXsltFilesFromDir(xsltDir, xsltDir, xslts);
			xsltFiles.DataSource = xslts;
			xsltFiles.DataBind();
			xsltFiles.Items.Insert(0, new ListItem("Browse xslt files on server...", string.Empty));
		}

		private static void GetMacroScriptFilesFromDir(string orgPath, string path, ArrayList files)
		{
			var dirInfo = new DirectoryInfo(path);
			if (dirInfo.Exists == false)
				return;

			var fileInfo = dirInfo.GetFiles("*.*").Where(f => f.Name.ToLowerInvariant() != "web.config".ToLowerInvariant());
			foreach (var file in fileInfo)
				files.Add(path.Replace(orgPath, string.Empty) + file.Name);

			// Populate subdirectories
			var dirInfos = dirInfo.GetDirectories();
			foreach (var dir in dirInfos)
				GetMacroScriptFilesFromDir(orgPath, path + "/" + dir.Name + "/", files);
		}

        private void PopulateMacroScriptFiles()
		{
			var razors = new ArrayList();
			var razorDir = IOHelper.MapPath(SystemDirectories.MacroScripts + "/");
			GetMacroScriptFilesFromDir(razorDir, razorDir, razors);
			pythonFiles.DataSource = razors;
			pythonFiles.DataBind();
			pythonFiles.Items.Insert(0, new ListItem("Browse scripting files on server...", string.Empty));
		}

		public void deleteMacroProperty(object sender, EventArgs e)
		{
			var macroPropertyId = (HtmlInputHidden)((Control)sender).Parent.FindControl("macroPropertyID");

		    var property = _macro.Properties.Single(x => x.Id == int.Parse(macroPropertyId.Value));
		    _macro.Properties.Remove(property);

		    Services.MacroService.Save(_macro);

			macroPropertyBind();
		}

		public void macroPropertyBind()
		{
			macroProperties.DataSource = _macro.Properties.OrderBy(x => x.SortOrder);
			macroProperties.DataBind();
		}

		public object CheckNull(object test)
		{
		    return Convert.IsDBNull(test) ? 0 : test;
		}

	    [Obsolete("No longer used and will be removed in the future.")]
		public IRecordsReader GetMacroPropertyTypes()
        {
            return null;
        }

        protected IEnumerable<IParameterEditor> GetMacroParameterEditors()
        {
            return ParameterEditorResolver.Current.ParameterEditors;
        }

		public void macroPropertyCreate(object sender, EventArgs e)
		{
            //enable add validators
            var val1 = (RequiredFieldValidator)((Control)sender).Parent.FindControl("RequiredFieldValidator1");
            var val2 = (RequiredFieldValidator)((Control)sender).Parent.FindControl("RequiredFieldValidator4");
            var val3 = (RequiredFieldValidator)((Control)sender).Parent.FindControl("RequiredFieldValidator5");
		    val1.Enabled = true;
            val2.Enabled = true;
            val3.Enabled = true;

            Page.Validate();

            if (Page.IsValid == false)
            {
                return;
            }

			var macroPropertyAliasNew = (TextBox)((Control)sender).Parent.FindControl("macroPropertyAliasNew");
			var macroPropertyNameNew = (TextBox)((Control)sender).Parent.FindControl("macroPropertyNameNew");
			var macroPropertyTypeNew = (DropDownList)((Control)sender).Parent.FindControl("macroPropertyTypeNew");
			
			if (macroPropertyAliasNew.Text != ui.Text("general", "new", UmbracoUser) + " " + ui.Text("general", "alias", UmbracoUser))
			{
                if (_macro.Properties.ContainsKey(macroPropertyAliasNew.Text.Trim()))
                {
                    //don't continue
                    return;
                }

			    _macro.Properties.Add(new MacroProperty(
			                              macroPropertyAliasNew.Text.Trim(),
			                              macroPropertyNameNew.Text.Trim(),
                                          _macro.Properties.Any() ? _macro.Properties.Max(x => x.SortOrder) + 1  : 0,
			                              macroPropertyTypeNew.SelectedValue));

			    Services.MacroService.Save(_macro);

                macroPropertyBind();
			}
		}

		public bool macroIsVisible(object isChecked)
		{
		    return Convert.ToBoolean(isChecked);
		}

	    public void AddChooseList(Object sender, EventArgs e)
		{
			if (IsPostBack == false)
			{
				var dropDown = (DropDownList)sender;
				dropDown.Items.Insert(0, new ListItem("Choose...", string.Empty));
			}
		}

		private void PopulateUserControls(string path)
		{
			var directoryInfo = new DirectoryInfo(path);
		    if (directoryInfo.Exists == false) return;

			var rootDir = IOHelper.MapPath(SystemDirectories.UserControls);

			foreach (var uc in directoryInfo.GetFiles("*.ascx"))
			{
			    userControlList.Items.Add(
			        new ListItem(SystemDirectories.UserControls +
			                     uc.FullName.Substring(rootDir.Length).Replace(IOHelper.DirSepChar, '/')));

			}
			foreach (var dir in directoryInfo.GetDirectories())
				PopulateUserControls(dir.FullName);
		}

		protected override void OnInit(EventArgs e)
		{			
			base.OnInit(e);
            EnsureChildControls();
		}

	    protected override void CreateChildControls()
	    {
	        base.CreateChildControls();

            // Tab setup
            InfoTabPage = TabView1.NewTabPage("Macro Properties");
            InfoTabPage.Controls.Add(Pane1);
            InfoTabPage.Controls.Add(Pane1_2);
            InfoTabPage.Controls.Add(Pane1_3);
            InfoTabPage.Controls.Add(Pane1_4);

            Parameters = TabView1.NewTabPage("Parameters");
            Parameters.Controls.Add(Panel2);

            var save = TabView1.Menu.NewButton();
            save.ButtonType = MenuButtonType.Primary;
            save.Text = ui.Text("save");
            save.ID = "save";
            save.Click += Save_Click;
	    }

        void Save_Click(object sender, EventArgs e)
        {

            Page.Validate();

            ClientTools
                .SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadMacros>().Tree.Alias)
                .SyncTree("-1,init," + _macro.Id.ToInvariantString(), true); //true forces the reload

            var tempMacroAssembly = macroAssembly.Text;
            var tempMacroType = macroType.Text;
            var tempCachePeriod = cachePeriod.Text;
            if (tempCachePeriod == string.Empty)
                tempCachePeriod = "0";
            if (tempMacroAssembly == string.Empty && macroUserControl.Text != string.Empty)
                tempMacroType = macroUserControl.Text;

            SetMacroValuesFromPostBack(_macro, Convert.ToInt32(tempCachePeriod), tempMacroAssembly, tempMacroType);

            // Save elements
            foreach (RepeaterItem item in macroProperties.Items)
            {
                var macroPropertyId = (HtmlInputHidden)item.FindControl("macroPropertyID");
                var macroElementName = (TextBox)item.FindControl("macroPropertyName");
                var macroElementAlias = (TextBox)item.FindControl("macroPropertyAlias");
                var macroElementSortOrder = (TextBox)item.FindControl("macroPropertySortOrder");
                var macroElementType = (DropDownList)item.FindControl("macroPropertyType");

                var prop = _macro.Properties.Single(x => x.Id == int.Parse(macroPropertyId.Value));
                var sortOrder = 0;
                int.TryParse(macroElementSortOrder.Text, out sortOrder);

                _macro.Properties.UpdateProperty(
                    prop.Alias,
                    macroElementName.Text.Trim(),
                    sortOrder,
                    macroElementType.SelectedValue,                    
                    macroElementAlias.Text.Trim());

            }

            Services.MacroService.Save(_macro);

            ClientTools.ShowSpeechBubble(speechBubbleIcon.save, "Macro saved", "");

            // Check for assemblyBrowser
            if (tempMacroType.IndexOf(".ascx", StringComparison.Ordinal) > 0)
                assemblyBrowserUserControl.Controls.Add(
                    new LiteralControl("<br/><button onClick=\"UmbClientMgr.openModalWindow('developer/macros/assemblyBrowser.aspx?fileName=" + macroUserControl.Text +
                        "&macroID=" + Request.QueryString["macroID"] +
                            "', 'Browse Properties', true, 500, 475); return false\" class=\"guiInputButton\"><img src=\"../../images/editor/propertiesNew.gif\" align=\"absmiddle\" style=\"width: 18px; height: 17px; padding-right: 5px;\"/> Browse properties</button>"));
            else if (tempMacroType != string.Empty && tempMacroAssembly != string.Empty)
                assemblyBrowser.Controls.Add(
                    new LiteralControl("<br/><button onClick=\"UmbClientMgr.openModalWindow('developer/macros/assemblyBrowser.aspx?fileName=" + macroAssembly.Text +
                        "&macroID=" + Request.QueryString["macroID"] + "&type=" + macroType.Text +
                            "', 'Browse Properties', true, 500, 475); return false\" class=\"guiInputButton\"><img src=\"../../images/editor/propertiesNew.gif\" align=\"absmiddle\" style=\"width: 18px; height: 17px; padding-right: 5px;\"/> Browse properties</button>"));

            macroPropertyBind();
        }

	    /// <summary>
		/// TabView1 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.TabView TabView1;

		/// <summary>
		/// Pane1 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.Pane Pane1;

		/// <summary>
		/// macroPane control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.HtmlControls.HtmlTable macroPane;

		/// <summary>
		/// macroName control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.TextBox macroName;

		/// <summary>
		/// macroAlias control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.TextBox macroAlias;

		/// <summary>
		/// Pane1_2 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.Pane Pane1_2;

		/// <summary>
		/// macroXslt control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.TextBox macroXslt;

		/// <summary>
		/// xsltFiles control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.DropDownList xsltFiles;

		/// <summary>
		/// macroUserControl control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.TextBox macroUserControl;

		/// <summary>
		/// userControlList control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.DropDownList userControlList;

		/// <summary>
		/// assemblyBrowserUserControl control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.PlaceHolder assemblyBrowserUserControl;

		/// <summary>
		/// macroAssembly control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.TextBox macroAssembly;

		/// <summary>
		/// macroType control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.TextBox macroType;

		/// <summary>
		/// assemblyBrowser control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.PlaceHolder assemblyBrowser;

		/// <summary>
		/// macroPython control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.TextBox macroPython;

		/// <summary>
		/// pythonFiles control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.DropDownList pythonFiles;

		/// <summary>
		/// Pane1_3 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.Pane Pane1_3;

		/// <summary>
		/// Table1 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.HtmlControls.HtmlTable Table1;

		/// <summary>
		/// macroEditor control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.CheckBox macroEditor;

		/// <summary>
		/// macroRenderContent control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.CheckBox macroRenderContent;

		/// <summary>
		/// Pane1_4 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.Pane Pane1_4;

		/// <summary>
		/// Table3 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.HtmlControls.HtmlTable Table3;

		/// <summary>
		/// cachePeriod control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.TextBox cachePeriod;

		/// <summary>
		/// cacheByPage control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.CheckBox cacheByPage;

		/// <summary>
		/// cachePersonalized control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.CheckBox cachePersonalized;

		/// <summary>
		/// Panel2 control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::umbraco.uicontrols.Pane Panel2;

		/// <summary>
		/// macroProperties control.
		/// </summary>
		/// <remarks>
		/// Auto-generated field.
		/// To modify move field declaration from designer file to code-behind file.
		/// </remarks>
		protected global::System.Web.UI.WebControls.Repeater macroProperties;
	}

}
