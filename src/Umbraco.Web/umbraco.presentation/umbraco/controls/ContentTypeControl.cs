using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Collections;
using System.IO;
using Umbraco.Core.IO;
using System.Linq;

namespace umbraco.controls
{
	/// <summary>
	/// Summary description for ContentTypeControl.
	/// </summary>
	public class ContentTypeControl : uicontrols.TabView
	{
		public event System.EventHandler OnSave;
		public event System.EventHandler OnPropertyTypeCreate;
		public event System.EventHandler OnPropertyTypeDelete;
		private cms.businesslogic.ContentType docType;
		protected TextBox NameTxt = new TextBox();
		protected TextBox AliasTxt = new TextBox();
		protected DropDownList IconDDL = new DropDownList();
		protected TextBox TabTxt = new TextBox();
		protected uicontrols.Pane GenericPropertyTypes;
		private uicontrols.Pane TabsPane = new uicontrols.Pane();
		private NodeTypeAddPropertyTypeControl AddPropertyTypeCtrl;
		private System.Collections.ArrayList tabDDLs = new ArrayList();
		private HtmlGenericControl br = new HtmlGenericControl();
		private NodeTypeEditorControl NodeTypeEditorCtrl;
		private uicontrols.Pane pp;
		private uicontrols.TabPage Panel1;
		private uicontrols.TabPage Panel2;
		private uicontrols.TabPage Panel3;
		private uicontrols.TabPage Panel4;
		private BasePages.BasePage prnt;
		private ListBox AllowedContentTypes;
		private ArrayList extraPropertyPanes = new ArrayList();

		public void addPropertyPaneToGeneralTab(uicontrols.Pane pp) {
			extraPropertyPanes.Add(pp);
		}
		public ContentTypeControl(cms.businesslogic.ContentType ct, string id) {
			this.ID = id;
			this.OnSave +=new EventHandler(tmp_OnSave);
			this.OnPropertyTypeCreate += new EventHandler(this.tmp_OnSave);
			this.OnPropertyTypeDelete += new EventHandler(this.tmp_OnSave);
			docType = ct;
			this.Width = Unit.Pixel(600);
			this.Height = Unit.Pixel(600);
            string UmbracoPath = SystemDirectories.Umbraco;
			Panel1 = this.NewTabPage("Generelt");
			uicontrols.MenuImageButton Save = Panel1.Menu.NewImageButton();
			Save.Click += new System.Web.UI.ImageClickEventHandler(save_click);
			Save.ID = "Panel1Save";
			Save.ImageUrl = UmbracoPath + "/images/editor/save.gif";
		

			Panel2 = this.NewTabPage("Faneblade");
			Save = Panel2.Menu.NewImageButton();
			Save.ID = "Panel2Save";
			Save.Click += new System.Web.UI.ImageClickEventHandler(save_click);
			Save.ImageUrl = UmbracoPath + "/images/editor/save.gif";


			Panel3 = this.NewTabPage("Struktur");
			Save = Panel3.Menu.NewImageButton();
			Save.ID = "Panel3Save";
			Save.Click += new System.Web.UI.ImageClickEventHandler(save_click);
			Save.ImageUrl = UmbracoPath + "/images/editor/save.gif";
			
			Panel4 = this.NewTabPage("Generiske egenskaber");
			Save = Panel4.Menu.NewImageButton();
			Save.ID = "Panel4Save";
			Save.Click += new System.Web.UI.ImageClickEventHandler(save_click);
			Save.ImageUrl = UmbracoPath + "/images/editor/save.gif";

			Panel1.Attributes.Add("align","center");
			Panel2.Attributes.Add("align","center");
			Panel3.Attributes.Add("align","center");
			Panel4.Attributes.Add("align","center");

			br = new HtmlGenericControl();
			br.TagName = "br";
			Panel1.Controls.Add(br);

			br = new HtmlGenericControl();
			br.TagName = "br";
			Panel2.Controls.Add(br);

			br = new HtmlGenericControl();
			br.TagName = "br";
			Panel3.Controls.Add(br);

			br = new HtmlGenericControl();
			br.TagName = "br";
			Panel4.Controls.Add(br);
		
		}

		protected void tmp_OnSave(object sender, System.EventArgs e) 
		{
			
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad (e);
			Page_Load(new object(), e);
		}

		protected void Page_Load(object sender, System.EventArgs e)
		{
			prnt = (BasePages.BasePage) this.Page;
			// Setup tab 1
			setupGeneralInfoTab();
			setupTabTab();
			setupGenericPropertyTypesTab();

			AllowedContentTypes = new ListBox();
			AllowedContentTypes.ID = "AllowedContentTypes";
			AllowedContentTypes.SelectionMode = ListSelectionMode.Multiple;
		

			int[] allowedIds = docType.AllowedChildContentTypeIDs;

			foreach (cms.businesslogic.ContentType ct in docType.GetAll()) 
			{
				ListItem li = new ListItem(ct.Text,ct.Id.ToString());
				AllowedContentTypes.Items.Add(li);
				if (!Page.IsPostBack) 
				{foreach (int i in allowedIds) if (i == ct.Id) li.Selected= true;}
			}

			pp = new uicontrols.Pane();
			pp.addProperty("Tilladte indholdstyper",AllowedContentTypes);
			Panel3.Controls.Add(pp);
		}

		

		public void save_click(object sender, System.Web.UI.ImageClickEventArgs e) 
		{
			OnSave(this, new System.EventArgs());
			docType.Text = NameTxt.Text;
			docType.Alias = AliasTxt.Text;
			docType.IconUrl = IconDDL.SelectedValue;


			string xtra = "";
			if (TabTxt.Text.Trim() != "") 
			{
				docType.AddVirtualTab(TabTxt.Text);
				xtra = ui.Text("speechBubbles", "contentTypeTabCreated");
				populateTabDDLs();
				TabsPane.Controls.Clear();
				LoadExistingTabsOnTabsPane();
				TabTxt.Text = "";
			}

			// Save allowed ChildTypes
			SaveAllowedChildTypes();

			NodeTypeEditorCtrl.Save();
			prnt.ClientTools.ShowSpeechBubble( BasePages.BasePage.speechBubbleIcon.save, ui.Text("speechBubbles", "contentTypeSavedHeader"),"" + xtra);
		}

		private void SaveAllowedChildTypes() {
			ArrayList tmp = new ArrayList();
			foreach (ListItem li in AllowedContentTypes.Items) 
			{
				if (li.Selected)
				tmp.Add(int.Parse(li.Value));
			}
			int[] ids = new int[tmp.Count];
			for (int i = 0;i <tmp.Count;i++) ids[i] = (int) tmp[i];
			docType.AllowedChildContentTypeIDs = ids;
		}

		private void setupGeneralInfoTab() 
		{
			uicontrols.Pane pp = new uicontrols.Pane();
					
			DirectoryInfo dirInfo = new DirectoryInfo( IOHelper.MapPath(SystemDirectories.Umbraco + "/images/umbraco"));
			FileInfo[] fileInfo = dirInfo.GetFiles();
			
			for(int i = 0; i < fileInfo.Length;i++) 
			{
				ListItem li = new ListItem(fileInfo[i].Name);
				if (!this.Page.IsPostBack && li.Value == docType.IconUrl) li.Selected = true;
				IconDDL.Items.Add(li);
			}

			if (!this.Page.IsPostBack) 
			{
				NameTxt.Text = docType.Text;
				AliasTxt.Text = docType.Alias;

			}
	
			pp.addProperty("Navn",NameTxt);
			pp.addProperty("Alias",AliasTxt);
			pp.addProperty("Ikon",IconDDL);
			Panel1.Controls.Add(pp);

			br = new HtmlGenericControl();
			br.TagName = "p";
			Panel1.Controls.Add(br);
			foreach (uicontrols.Pane p in extraPropertyPanes) {
				Panel1.Controls.Add(p);
			}
		}


		#region "Generic properties"
		private void setupGenericPropertyTypesTab() 
		{
			// Add new generic propertytype
			pp = new uicontrols.Pane();
			
			AddPropertyTypeCtrl = new NodeTypeAddPropertyTypeControl(docType,this);
			pp.addProperty(AddPropertyTypeCtrl); 
			Panel4.Controls.Add(pp);	

			tabDDLs.Add(AddPropertyTypeCtrl.TabDDL);
			br = new HtmlGenericControl();
			br.TagName = "br";
			Panel4.Controls.Add(br);

			loadGenericPropertyTypesOnPane();			
		}

		private void loadGenericPropertyTypesOnPane() 
		{
			// View/ Edit all propertytypes..
			GenericPropertyTypes = new uicontrols.Pane();

			NodeTypeEditorCtrl = new NodeTypeEditorControl(this);
			GenericPropertyTypes.addProperty(NodeTypeEditorCtrl);
			Panel4.Controls.Add(GenericPropertyTypes);
		}

		#endregion

		#region "Tabs Add Delete Update DDL's"

		private void setupTabTab() 
		{
			pp = new uicontrols.Pane();
			pp.addProperty("Ny tab", TabTxt);
			Panel2.Controls.Add(pp);

			br = new HtmlGenericControl();
			br.TagName = "br";
			Panel2.Controls.Add(br);

			
			LoadExistingTabsOnTabsPane();
		
			
		}


		private void LoadExistingTabsOnTabsPane()  
		{
			uicontrols.Pane TabsPane = new uicontrols.Pane();
            foreach (cms.businesslogic.ContentType.TabI t in docType.getVirtualTabs.ToList())
			{
				Button tb = new Button();
				tb.Text = "Slet";
				tb.ID = "tab" + t.Id.ToString();
				TabsPane.addProperty("Faneblad: " + t.Caption, tb);
				tb.Click += new EventHandler(deleteTab);
			}
			Panel2.Controls.Add(TabsPane);
		}
		
		
		private void deleteTab(object sender, System.EventArgs e) 
		{
			Button b = (Button) sender;
			docType.DeleteVirtualTab(int.Parse(b.ID.Replace("tab","")));

			TabsPane.Controls.Clear();
			LoadExistingTabsOnTabsPane();
			
			populateTabDDLs();
            prnt.ClientTools.ShowSpeechBubble(BasePages.BasePage.speechBubbleIcon.info, ui.Text("speechBubbles", "contentTypeTabDeleted"), ui.Text("speechBubbles", "contentTypeTabDeletedText", b.ID));
		}


		private void populateTabDDLs() 
		{
			foreach (DropDownList ddl in tabDDLs) 
			{
				string selVal = "0";
				if(ddl.SelectedIndex >= 0) selVal = ddl.SelectedValue;

				ddl.Items.Clear();
                foreach (cms.businesslogic.web.DocumentType.TabI t in docType.getVirtualTabs.ToList()) 
				{
					ListItem li = new ListItem();
					li.Text  = t.Caption;
					li.Value = t.Id.ToString();
					ddl.Items.Add(li);
				}
				ddl.Items.Add(new ListItem("Egenskaber","0"));
				try 
				{
					ddl.SelectedValue = selVal;
				}
				catch
				{
					this.Page.Trace.Warn("Tab could not be selected");
				}
			}
		}


		#endregion
		
		#region "Custom controls"
		private class NodeTypeAddPropertyTypeControl : System.Web.UI.HtmlControls.HtmlTable 
		{
			private TextBox NameTxt = new TextBox();
			private TextBox AliasTxt = new TextBox();
			private DropDownList DataTypeDDL = new DropDownList();
			public DropDownList TabDDL = new DropDownList();
			private cms.businesslogic.ContentType _dt;
			private ContentTypeControl ctctrl;
			public NodeTypeAddPropertyTypeControl(cms.businesslogic.ContentType dt, ContentTypeControl parent) 
			{
				ctctrl = parent;
				this.Attributes.Add("width","100%");
				HtmlTableRow tr = new HtmlTableRow();
				HtmlTableCell td =  new HtmlTableCell();
				td.InnerText = "Alias";
				tr.Cells.Add(td);

				td = new HtmlTableCell();
				td.InnerText  = "Navn";
				tr.Cells.Add(td);

				td = new HtmlTableCell();
				td.InnerText = "Type";
				tr.Cells.Add(td);

				td = new HtmlTableCell();
				td.InnerText = "Fane";
				tr.Cells.Add(td);

				td = new HtmlTableCell();
				tr.Cells.Add(td);

				this.Rows.Add(tr);
				tr = new HtmlTableRow();

				td = new HtmlTableCell();
				td.Controls.Add(AliasTxt);
				tr.Cells.Add(td);

				td = new HtmlTableCell();
				td.Controls.Add(NameTxt);
				tr.Cells.Add(td);

				td = new HtmlTableCell();
				td.Controls.Add(DataTypeDDL);
				tr.Cells.Add(td);

				td = new HtmlTableCell();
				td.Controls.Add(TabDDL);
				tr.Cells.Add(td);

				td = new HtmlTableCell();
				Button btn = new Button();
					
				td.Controls.Add(btn);
				btn.Text = "Opret";
				tr.Cells.Add(td);
				this.Rows.Add(tr);

				// Add create new PropertyType eventhandler
				btn.Click += new System.EventHandler(this.AddPropertyType);


                foreach (cms.businesslogic.ContentType.TabI t in dt.getVirtualTabs.ToList()) 
				{
					ListItem li = new ListItem();
					li.Value = t.Id.ToString();
					li.Text = t.Caption;
					TabDDL.Items.Add(li);
				}
				ListItem lie = new ListItem();
				lie.Text = "Egenskaber";
				lie.Value = "0";
				TabDDL.Items.Add(lie);
				
				foreach(cms.businesslogic.datatype.DataTypeDefinition DataType in cms.businesslogic.datatype.DataTypeDefinition.GetAll()) 
				{
					ListItem li = new ListItem();
					li.Value = DataType.Id.ToString();
					li.Text = DataType.Text;
					DataTypeDDL.Items.Add(li);
				}
				_dt = dt;
			}


			private void AddPropertyType(object sender, System.EventArgs e) 
			{
				
				if (NameTxt.Text.Trim() != "" && AliasTxt.Text.Trim() != "") 
				{
					string[] info = {NameTxt.Text, DataTypeDDL.SelectedItem.Value};
                    ctctrl.prnt.ClientTools.ShowSpeechBubble(BasePages.BasePage.speechBubbleIcon.info, ui.Text("speechBubbles", "contentTypePropertyTypeCreated"), ui.Text("speechBubbles", "contentTypePropertyTypeCreatedText", info));
					_dt.AddPropertyType(cms.businesslogic.datatype.DataTypeDefinition.GetDataTypeDefinition(int.Parse(DataTypeDDL.SelectedValue)),AliasTxt.Text,NameTxt.Text);
					
					if (int.Parse(TabDDL.SelectedValue) != 0) 
					{
						_dt.SetTabOnPropertyType(_dt.getPropertyType(AliasTxt.Text),int.Parse(TabDDL.SelectedValue));
					}
					ctctrl.GenericPropertyTypes.Controls.Clear();
					ctctrl.loadGenericPropertyTypesOnPane();
					NameTxt.Text = "";
					AliasTxt.Text = "";
					// fire event to tell that a new propertytype is created!!
					ctctrl.OnPropertyTypeCreate(this, new System.EventArgs());
				} 
				else
				{
					
                    ctctrl.prnt.ClientTools.ShowSpeechBubble(BasePages.BasePage.speechBubbleIcon.error, ui.Text("error"), ui.Text("errors", "contentTypeAliasAndNameNotNull"));
				}
			}
		}


		private class NodeTypeEditorControl : System.Web.UI.HtmlControls.HtmlTable
		{
			private System.Collections.ArrayList PropertyTypes = new System.Collections.ArrayList();
			private ContentTypeControl prnt;
			public NodeTypeEditorControl(ContentTypeControl parent) 
			{
				prnt = parent;
				this.Attributes.Add("width","100%");
				HtmlTableRow tr = new HtmlTableRow();
				HtmlTableCell td =  new HtmlTableCell();
				td.InnerText = "Alias";
				tr.Cells.Add(td);

				td = new HtmlTableCell();
				td.InnerText  = "Navn";
				tr.Cells.Add(td);

				td = new HtmlTableCell();
				td.InnerText = "Type";
				tr.Cells.Add(td);

				td = new HtmlTableCell();
				td.InnerText = "Fane";
				tr.Cells.Add(td);

				td = new HtmlTableCell();
				tr.Cells.Add(td);

				this.Rows.Add(tr);
               
				cms.businesslogic.ContentType.TabI[] tbs = prnt.docType.getVirtualTabs;
	

				foreach (cms.businesslogic.propertytype.PropertyType pt in prnt.docType.PropertyTypes) 
				{
					editor e = new editor(pt,prnt);
					PropertyTypes.Add(e);
					this.Controls.Add(e);
				}
				
			}

			public string Save() 
			{
				
				foreach (editor e in PropertyTypes) 
				{
					e.save();
				}
				return "2 egenskabstyper ændret";
			}

			

			private class editor : System.Web.UI.HtmlControls.HtmlTableRow 
			{
				private DropDownList TabDDL = new DropDownList();
				private cms.businesslogic.propertytype.PropertyType pt;
				ContentTypeControl ent;
				public editor(cms.businesslogic.propertytype.PropertyType pt,ContentTypeControl parent) 
				{
					this.pt = pt;	
					ent = parent;
				}

				protected override void OnLoad(EventArgs e)
				{
					base.OnLoad (e);
					
					
					ent.tabDDLs.Add(TabDDL);

                    foreach (cms.businesslogic.ContentType.TabI t in ent.docType.getVirtualTabs.ToList()) 
						TabDDL.Items.Add(new ListItem(t.Caption,t.Id.ToString()));
					TabDDL.Items.Add(new ListItem("Egenskaber","0"));
					if (!Parent.Page.IsPostBack) 
					{
						int tabID  = cms.businesslogic.ContentType.getTabIdFromPropertyType(pt);
						TabDDL.SelectedValue = tabID.ToString();
					}
					
					HtmlTableCell td = new HtmlTableCell();

					td.InnerText = pt.Alias;
					this.Cells.Add(td);

					td = new HtmlTableCell();
					td.InnerText = pt.Name;
					this.Cells.Add(td);

					td = new HtmlTableCell();
					td.InnerText = pt.DataTypeDefinition.Text;
					this.Cells.Add(td);

					td = new HtmlTableCell();
					td.Controls.Add(TabDDL);
					this.Cells.Add(td);

					td = new HtmlTableCell();
					Button btn = new Button();
					btn.Text = "Slet";
					btn.ID = "propertytype" + pt.Id;
					btn.Click += new EventHandler(deletepropertytype_click);
					
					td.Controls.Add(btn);
					this.Cells.Add(td);
				}

				public void deletepropertytype_click(object sender, System.EventArgs e) 
				{
					Button s = (Button) sender;
					int propertytypeId = int.Parse(s.ID.Replace("propertytype",""));
					cms.businesslogic.propertytype.PropertyType.GetPropertyType(propertytypeId).delete();
				
					ent.GenericPropertyTypes.Controls.Clear();
					ent.loadGenericPropertyTypesOnPane();

					BasePages.BasePage bp = (BasePages.BasePage) this.Page;
                    bp.ClientTools.ShowSpeechBubble(BasePages.BasePage.speechBubbleIcon.info, ui.Text("speechBubbles", "contentTypePropertyTypeDeleted"), "");
					ent.OnPropertyTypeDelete(this, new System.EventArgs());
				}

				public void save() 
				{
					if (int.Parse(TabDDL.SelectedValue) > 0)
					{
						ent.docType.SetTabOnPropertyType(pt,int.Parse(TabDDL.SelectedValue));
					} 
					else
					{
						ent.docType.removePropertyTypeFromTab(pt);
					}
				}
			}
		}

		#endregion

	}
}
