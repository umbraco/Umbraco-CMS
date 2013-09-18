using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using ClientDependency.Core;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.propertytype;

namespace umbraco.controls.GenericProperties
{
	

	/// <summary>
	///		Summary description for GenericProperty.
	/// </summary>
	[ClientDependency(ClientDependencyType.Css, "GenericProperty/genericproperty.css", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "GenericProperty/genericproperty.js", "UmbracoClient")]
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
            get { return _tabs; }
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
            get {return tbAlias.Text;} // FIXME so we blindly trust the UI for safe aliases?!
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
            SetDefaultDocumentTypeProperty();
			checkMandatory.Checked = false;
		}

		protected void Page_Load(object sender, System.EventArgs e)
		{
			if (!IsPostBack) 
			{
				UpdateInterface();
			}
		}

        //SD: this is temporary in v4, in v6 we have a proper user control hierarchy
        //containing this property.
        //this is required due to this issue: http://issues.umbraco.org/issue/u4-493
        //because we need to execute some code in async but due to the localization 
        //framework requiring an httpcontext.current, it will not work. 
        //http://issues.umbraco.org/issue/u4-2143
        //so, we are going to make a property here and ensure that the basepage has
        //resolved the user before we execute the async task so that in this method
        //our calls to ui.text will include the current user and not rely on the 
        //httpcontext.current. This also makes it perform better:
        // http://issues.umbraco.org/issue/U4-2142
	    private User CurrentUser
	    {
	        get { return ((BasePage) Page).getUser(); }
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
                DeleteButton.CssClass = "delete-button";
                DeleteButton.Attributes.Add("onclick", "return confirm('" + ui.Text("areyousure", CurrentUser) + "');");
				DeleteButton2.Visible = true;
                DeleteButton2.CssClass = "delete-button";
                DeleteButton2.Attributes.Add("onclick", "return confirm('" + ui.Text("areyousure", CurrentUser) + "');");
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
                var itemSelected = false;
				foreach(cms.businesslogic.datatype.DataTypeDefinition dt in _dataTypeDefinitions) 
				{
					var li = new ListItem(dt.Text, dt.Id.ToString());
                    if ((_pt != null && _pt.DataTypeDefinition.Id == dt.Id))
                    {
                        li.Selected = true;
                        itemSelected = true;
                    }

					ddlTypes.Items.Add(li);
				}

                // If item not selected from previous edit or load, set to default according to settings
                if (!itemSelected)
                {
                    SetDefaultDocumentTypeProperty();
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
			if (_pt != null && string.IsNullOrEmpty(_pt.ValidationRegExp) == false)
				tbValidation.Text = _pt.ValidationRegExp;

			// description
			if (_pt != null && _pt.Description != "")
				tbDescription.Text = _pt.GetRawDescription();
		}

        private void SetDefaultDocumentTypeProperty()
        {
            var itemToSelect = ddlTypes.Items.Cast<ListItem>()
                .FirstOrDefault(item => item.Text.ToLowerInvariant() == UmbracoConfiguration.Current.UmbracoSettings.Content.DefaultDocumentTypeProperty.ToLowerInvariant());
            
            if (itemToSelect != null)
            {
                itemToSelect.Selected = true;
            }
            else
            {
                ddlTypes.SelectedIndex = -1;
            }
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

            // [ClientDependency(ClientDependencyType.Javascript, "js/UmbracoCasingRules.aspx", "UmbracoRoot")]
		    var loader = ClientDependency.Core.Controls.ClientDependencyLoader.GetInstance(new HttpContextWrapper(Context));
		    var helper = new UrlHelper(new RequestContext(new HttpContextWrapper(Context), new RouteData()));
            loader.RegisterDependency(helper.GetCoreStringsControllerPath() + "ServicesJavaScript", ClientDependencyType.Javascript);
		}
		
		/// <summary>
		///		Required method for Designer support - do not modify
		///		the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.DeleteButton.Click +=DeleteButton_Click;
            this.DeleteButton2.Click += DeleteButton2_Click;

		}

        void DeleteButton2_Click(object sender, EventArgs e)
        {
            Delete(this, new System.EventArgs());
        }

        void DeleteButton_Click(object sender, EventArgs e)
        {
            Delete(this, new System.EventArgs());
        }
		#endregion

	}
}
