using System;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using umbraco.BasePages;
using ClientDependency.Core;
using umbraco.IO;

namespace umbraco.controls.GenericProperties
{
	

	/// <summary>
	///		Summary description for GenericProperty.
	/// </summary>
	[ClientDependency(ClientDependencyType.Css, "GenericProperty/genericproperty.css", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "GenericProperty/genericproperty.js", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "js/UmbracoCasingRules.aspx", "UmbracoRoot")]
    public partial class GenericProperty : System.Web.UI.UserControl
	{

		private cms.businesslogic.propertytype.PropertyType _pt;
		private cms.businesslogic.web.DocumentType.TabI[] _tabs;
		private cms.businesslogic.datatype.DataTypeDefinition[] _dataTypeDefinitions;
		private int _tabId = 0;

		public event System.EventHandler Delete;
		
		private string _fullId = "";

		public cms.businesslogic.datatype.DataTypeDefinition[] DataTypeDefinitions 
		{
			set 
			{
				_dataTypeDefinitions = value;
			}
		}

		public int TabId 
		{
			 set {
				 _tabId = value;
			 }
		}

		public cms.businesslogic.propertytype.PropertyType PropertyType 
		{
			set 
			{
               	_pt = value;
			}
			get 
			{
				return _pt;
			}
		}

		public cms.businesslogic.web.DocumentType.TabI[] Tabs 
		{
			set 
			{
				_tabs = value;
			}
		}

		public string Name 
		{
			get {
				return tbName.Text;
			}
		}
		public string Alias 
		{
			get {return tbAlias.Text;}
		}
		public string Description 
		{
			get {return tbDescription.Text;}
		}
		public string Validation
		{
			get {return tbValidation.Text;}
		}
		public bool Mandatory
		{
			get {return checkMandatory.Checked;}
		}
		public int Tab 
		{
			get {return int.Parse(ddlTab.SelectedValue);}
		}
		public string FullId
		{
			set 
			{
				_fullId = value;
			}
			get 
			{
				return _fullId;
			}
		}

        private int _id;
        public int Id {
            set {
                _id = value;
            }get{
                return _id;
            }
        }

		public int Type
		{
			get {return int.Parse(ddlTypes.SelectedValue);}
		}

		public void Clear() 
		{
			tbName.Text = "";
			tbAlias.Text = "";
			tbValidation.Text = "";
			tbDescription.Text = "";
			ddlTab.SelectedIndex = 0;
			ddlTypes.SelectedIndex = 0;
			checkMandatory.Checked = false;
		}

		protected void Page_Load(object sender, System.EventArgs e)
		{
			if (!IsPostBack) 
			{

				UpdateInterface();
			}

		}

		public void UpdateInterface() 
		{
			// Name and alias
			if (_pt != null) 
			{
                _id = _pt.Id;
				//form.Attributes.Add("style", "display: none;");
				tbName.Text = _pt.GetRawName();
				tbAlias.Text = _pt.Alias;
				FullHeader.Text = _pt.GetRawName() + " (" + _pt.Alias + "), Type: " + _pt.DataTypeDefinition.Text;;
				Header.Text = _pt.GetRawName();
				DeleteButton.Visible = true;
				DeleteButton.ImageUrl = SystemDirectories.Umbraco + "/images/delete_button.png";
				DeleteButton.Attributes.Add("style", "float: right; cursor: hand;");
				DeleteButton.Attributes.Add("onclick", "return confirm('" + ui.Text("areyousure") + "');");
				DeleteButton2.Visible = true;
                DeleteButton2.ImageUrl = SystemDirectories.Umbraco + "/images/delete_button.png";
				DeleteButton2.Attributes.Add("style", "float: right; cursor: hand;");
				DeleteButton2.Attributes.Add("onclick", "return confirm('" + ui.Text("areyousure") + "');");
			} 
			else 
			{
				// Add new header
				FullHeader.Text = "Click here to add a new property";
				Header.Text = "Create new property";

				// Hide image button
				DeleteButton.Visible = false;
				DeleteButton2.Visible = false;
			}
            validationLink.NavigateUrl = "#";
            validationLink.Attributes["onclick"] = ClientTools.Scripts.OpenModalWindow("dialogs/regexWs.aspx?target=" + tbValidation.ClientID, "Search for regular expression", 600, 500) + ";return false;";

			// Data type definitions
			if (_dataTypeDefinitions != null) 
			{
				ddlTypes.Items.Clear();
				foreach(cms.businesslogic.datatype.DataTypeDefinition dt in _dataTypeDefinitions) 
				{
					ListItem li = new ListItem(dt.Text, dt.Id.ToString());
					if (_pt != null && _pt.DataTypeDefinition.Id == dt.Id)
						li.Selected = true;
					ddlTypes.Items.Add(li);
				}
			}

			// tabs
			if (_tabs != null) 
			{
				ddlTab.Items.Clear();
				for (int i=0;i<_tabs.Length;i++) 
				{
					ListItem li = new ListItem(_tabs[i].Caption, _tabs[i].Id.ToString());
					if (_tabs[i].Id == _tabId)
						li.Selected = true;
					ddlTab.Items.Add(li);
				}
			}
			ListItem liGeneral = new ListItem("Generic Properties", "0");
			if (_tabId == 0)
				liGeneral.Selected = true;
			ddlTab.Items.Add(liGeneral);

			// mandatory
			if (_pt != null && _pt.Mandatory)
				checkMandatory.Checked = true;

			// validation
			if (_pt != null && _pt.ValidationRegExp != "")
				tbValidation.Text = _pt.ValidationRegExp;

			// description
			if (_pt != null && _pt.Description != "")
				tbDescription.Text = _pt.GetRawDescription();
		}

		protected void defaultDeleteHandler(object sender, System.EventArgs e) 
		{
		
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);

			this.Delete += new System.EventHandler(defaultDeleteHandler);
		}
		
		/// <summary>
		///		Required method for Designer support - do not modify
		///		the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.DeleteButton.Click += new System.Web.UI.ImageClickEventHandler(this.DeleteButton_Click);
			this.DeleteButton2.Click += new System.Web.UI.ImageClickEventHandler(this.DeleteButton2_Click);

		}
		#endregion

		private void DeleteButton_Click(object sender, System.Web.UI.ImageClickEventArgs e)
		{
			Delete(this,new System.EventArgs());
		}

		private void DeleteButton2_Click(object sender, System.Web.UI.ImageClickEventArgs e)
		{
			Delete(this,new System.EventArgs());
		}
	}
}
