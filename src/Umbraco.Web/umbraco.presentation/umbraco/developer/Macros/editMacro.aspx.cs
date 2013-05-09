using System;
using System.Collections;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.IO;
using Umbraco.Core.IO;
using umbraco.BasePages;
using umbraco.presentation.cache;
using umbraco.uicontrols;
using umbraco.DataLayer;
using umbraco.cms.presentation.Trees;
using umbraco.cms.businesslogic.macro;

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
					.SyncTree("-1,init," + m_macro.Id.ToString(), false);

				string tempMacroAssembly = m_macro.Assembly ?? "";
				string tempMacroType = m_macro.Type ?? "";

				PopulateFieldsOnLoad(m_macro, tempMacroAssembly, tempMacroType);

				// Check for assemblyBrowser
				if (tempMacroType.IndexOf(".ascx") > 0)
					assemblyBrowserUserControl.Controls.Add(
						new LiteralControl("<br/><button onClick=\"UmbClientMgr.openModalWindow('" + IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/developer/macros/assemblyBrowser.aspx?fileName=" + macroUserControl.Text +
										   "&macroID=" + m_macro.Id.ToString() +
										   "', 'Browse Properties', true, 475,500); return false;\" class=\"guiInputButton\"><img src=\"../../images/editor/propertiesNew.gif\" align=\"absmiddle\" style=\"width: 18px; height: 17px; padding-right: 5px;\"/> Browse properties</button>"));
				else if (tempMacroType != string.Empty && tempMacroAssembly != string.Empty)
					assemblyBrowser.Controls.Add(
						new LiteralControl("<br/><button onClick=\"UmbClientMgr.openModalWindow('" + IOHelper.ResolveUrl(SystemDirectories.Umbraco) + "/developer/macros/assemblyBrowser.aspx?fileName=" + macroAssembly.Text +
										   "&macroID=" + m_macro.Id.ToString() + "&type=" + macroType.Text +
										   "', 'Browse Properties', true, 475,500); return false\" class=\"guiInputButton\"><img src=\"../../images/editor/propertiesNew.gif\" align=\"absmiddle\" style=\"width: 18px; height: 17px; padding-right: 5px;\"/> Browse properties</button>"));

				// Load elements from macro
				macroPropertyBind();

				// Load xslt files from default dir
				PopulateXsltFiles();

				// Load python files from default dir
				PopulatePythonFiles();

				// Load usercontrols
				PopulateUserControls(IOHelper.MapPath(SystemDirectories.UserControls));
				userControlList.Items.Insert(0, new ListItem("Browse usercontrols on server...", string.Empty));

			}
			else
			{
				int macroID = Convert.ToInt32(Request.QueryString["macroID"]);

                ClientTools
                    .SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadMacros>().Tree.Alias)
                    .SyncTree("-1,init," + m_macro.Id.ToString(), true); //true forces the reload

				string tempMacroAssembly = macroAssembly.Text;
				string tempMacroType = macroType.Text;
				string tempCachePeriod = cachePeriod.Text;
				if (tempCachePeriod == string.Empty)
					tempCachePeriod = "0";
				if (tempMacroAssembly == string.Empty && macroUserControl.Text != string.Empty)
					tempMacroType = macroUserControl.Text;

				SetMacroValuesFromPostBack(m_macro, Convert.ToInt32(tempCachePeriod), tempMacroAssembly, tempMacroType);
				
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
					macro.GetMacro(macroID).removeFromCache();

				base.speechBubble(speechBubbleIcon.save, "Macro saved", "");


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

		/// <summary>
		/// Populates the control (textbox) values on page load
		/// </summary>
		/// <param name="macro"></param>
		/// <param name="macroAssemblyValue"></param>
		/// <param name="macroTypeValue"></param>
		protected virtual void PopulateFieldsOnLoad(Macro macro, string macroAssemblyValue, string macroTypeValue)
		{
			macroName.Text = macro.Name;
			macroAlias.Text = macro.Alias;
			macroXslt.Text = macro.Xslt;
			macroPython.Text = macro.ScriptingFile;
			cachePeriod.Text = macro.RefreshRate.ToString();
			macroRenderContent.Checked = macro.RenderContent;
			macroEditor.Checked = macro.UseInEditor;
			cacheByPage.Checked = macro.CacheByPage;
			cachePersonalized.Checked = macro.CachePersonalized;

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
		protected virtual void SetMacroValuesFromPostBack(Macro macro, int macroCachePeriod, string macroAssemblyValue, string macroTypeValue)
		{
			macro.UseInEditor = macroEditor.Checked;
			macro.RenderContent = macroRenderContent.Checked;
			macro.CacheByPage = cacheByPage.Checked;
			macro.CachePersonalized = cachePersonalized.Checked;
			macro.RefreshRate = macroCachePeriod;
			macro.Alias = macroAlias.Text;
			macro.Name = macroName.Text;
			macro.Assembly = macroAssemblyValue;
			macro.Type = macroTypeValue;
			macro.Xslt = macroXslt.Text;
			macro.ScriptingFile = macroPython.Text;
		}

		private void GetXsltFilesFromDir(string orgPath, string path, ArrayList files)
		{
			DirectoryInfo dirInfo = new DirectoryInfo(path);

			// Populate subdirectories
			DirectoryInfo[] dirInfos = dirInfo.GetDirectories();
			foreach (DirectoryInfo dir in dirInfos)
				GetXsltFilesFromDir(orgPath, path + "/" + dir.Name, files);

			FileInfo[] fileInfo = dirInfo.GetFiles("*.xsl*");

			foreach (FileInfo file in fileInfo)
				files.Add((path.Replace(orgPath, string.Empty).Trim('/') + "/" + file.Name).Trim('/'));
		}

		private void PopulateXsltFiles()
		{
			ArrayList xslts = new ArrayList();
			string xsltDir = IOHelper.MapPath(SystemDirectories.Xslt + "/");
			GetXsltFilesFromDir(xsltDir, xsltDir, xslts);
			xsltFiles.DataSource = xslts;
			xsltFiles.DataBind();
			xsltFiles.Items.Insert(0, new ListItem("Browse xslt files on server...", string.Empty));
		}

		private void GetPythonFilesFromDir(string orgPath, string path, ArrayList files)
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
				GetPythonFilesFromDir(orgPath, path + "/" + dir.Name + "/", files);
		}

		private void PopulatePythonFiles()
		{
			ArrayList pythons = new ArrayList();
			string pythonDir = IOHelper.MapPath(SystemDirectories.MacroScripts + "/");
			GetPythonFilesFromDir(pythonDir, pythonDir, pythons);
			pythonFiles.DataSource = pythons;
			pythonFiles.DataBind();
			pythonFiles.Items.Insert(0, new ListItem("Browse scripting files on server...", string.Empty));
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
					mp.Public = true;
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

		private void PopulateUserControls(string path)
		{
			DirectoryInfo di = new DirectoryInfo(path);

			string rootDir = IOHelper.MapPath(SystemDirectories.UserControls);

			foreach (FileInfo uc in di.GetFiles("*.ascx"))
			{
				userControlList.Items.Add(
					new ListItem(SystemDirectories.UserControls +
							uc.FullName.Substring(rootDir.Length).Replace(IOHelper.DirSepChar, '/')));
				/*
										uc.FullName.IndexOf(usercontrolsDir), 
										uc.FullName.Length - uc.FullName.IndexOf(usercontrolsDir)).Replace(IOHelper.DirSepChar, '/')));
				*/

			}
			foreach (DirectoryInfo dir in di.GetDirectories())
				PopulateUserControls(dir.FullName);
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
			save.ID = "save";

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
