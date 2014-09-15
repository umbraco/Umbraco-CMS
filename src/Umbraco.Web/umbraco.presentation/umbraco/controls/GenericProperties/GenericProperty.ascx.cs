using System;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI.WebControls;
using ClientDependency.Core;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.propertytype;
using Umbraco.Core;
using Umbraco.Core.Configuration;

namespace umbraco.controls.GenericProperties
{
    /// <summary>
    ///		Summary description for GenericProperty.
    /// </summary>
    [ClientDependency(ClientDependencyType.Css, "GenericProperty/genericproperty.css", "UmbracoClient")]
    [ClientDependency(ClientDependencyType.Javascript, "GenericProperty/genericproperty.js", "UmbracoClient")]
    public partial class GenericProperty : System.Web.UI.UserControl
    {

        /// <summary>
        /// Constructor
        /// </summary>
        public GenericProperty()
        {
            FullId = "";
            AllowPropertyEdit = true;
        }

        private cms.businesslogic.datatype.DataTypeDefinition[] _dataTypeDefinitions;
        private int _tabId = 0;

        public event EventHandler Delete;

        /// <summary>
        /// Defines whether the property can be edited in the UI
        /// </summary>
        [Obsolete("Use the combination of AllowAliasEdit,AllowValidationEdit,AllowDelete,AllowDataTypeEdit instead")]
        public bool AllowPropertyEdit
        {
            get { return AllowAliasEdit && AllowValidationEdit && AllowDelete && AllowDataTypeEdit; }
            set { AllowAliasEdit = AllowValidationEdit = AllowDelete = AllowDataTypeEdit = true; }
        }

        /// <summary>
        /// Defines whether the property's name can be edited in the UI
        /// </summary>
        public bool AllowNameEdit { get; set; }
        public bool AllowAliasEdit { get; set; }
        public bool AllowDataTypeEdit { get; set; }
        public bool AllowTabEdit { get; set; }
        public bool AllowValidationEdit { get; set; }
        public bool AllowDescriptionEdit { get; set; }
        public bool AllowDelete { get; set; }

        public cms.businesslogic.datatype.DataTypeDefinition[] DataTypeDefinitions
        {
            set { _dataTypeDefinitions = value; }
        }

        public int TabId
        {
            set { _tabId = value; }
        }

        public PropertyType PropertyType { get; set; }

        public ContentType.TabI[] Tabs { get; set; }

        public string Name
        {
            get { return tbName.Text; }
        }

        public string Alias
        {
            get { return tbAlias.Text; } // FIXME so we blindly trust the UI for safe aliases?!
        }

        public string Description
        {
            get { return tbDescription.Text; }
        }
        public string Validation
        {
            get { return tbValidation.Text; }
        }
        public bool Mandatory
        {
            get { return checkMandatory.Checked; }
        }
        public int Tab
        {
            get { return int.Parse(ddlTab.SelectedValue); }
        }

        public string FullId { get; set; }

        public int Id { get; set; }

        public int Type
        {
            get { return int.Parse(ddlTypes.SelectedValue); }
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
            get { return ((BasePage)Page).getUser(); }
        }

        public void UpdateInterface()
        {
            // Name and alias
            if (PropertyType != null)
            {
                Id = PropertyType.Id;
                //form.Attributes.Add("style", "display: none;");
                tbName.Text = PropertyType.GetRawName();
                tbAlias.Text = PropertyType.Alias;
                FullHeader.Text = PropertyType.GetRawName() + " (" + PropertyType.Alias + "), Type: " + PropertyType.DataTypeDefinition.Text; ;
                Header.Text = PropertyType.GetRawName();

                DeleteButton.CssClass = "delete-button";
                DeleteButton.Attributes.Add("onclick", "return confirm('" + ui.Text("areyousure", CurrentUser) + "');");

                DeleteButton2.CssClass = "delete-button";
                DeleteButton2.Attributes.Add("onclick", "return confirm('" + ui.Text("areyousure", CurrentUser) + "');");

                DeleteButton.Visible = AllowDelete;
                DeleteButton2.Visible = AllowDelete;

                //alias visibility
                PropertyPanel2.Visible = AllowAliasEdit;
                //chk mandatory visibility
                PropertyPanel5.Visible = AllowValidationEdit;
                // validation visibility
                PropertyPanel6.Visible = AllowValidationEdit;
                // drop down data types visibility
                PropertyPanel3.Visible = AllowDataTypeEdit;
                // name visibility
                PropertyPanel1.Visible = AllowNameEdit;
                // desc visibility
                PropertyPanel7.Visible = AllowDescriptionEdit;
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
                foreach (cms.businesslogic.datatype.DataTypeDefinition dt in _dataTypeDefinitions)
                {
                    var li = new ListItem(dt.Text, dt.Id.ToString(CultureInfo.InvariantCulture));
                    if ((PropertyType != null && PropertyType.DataTypeDefinition.Id == dt.Id))
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
            if (Tabs != null)
            {
                ddlTab.Items.Clear();
                for (int i = 0; i < Tabs.Length; i++)
                {
                    ListItem li = new ListItem(Tabs[i].Caption, Tabs[i].Id.ToString());
                    if (Tabs[i].Id == _tabId)
                        li.Selected = true;
                    ddlTab.Items.Add(li);
                }
            }
            ListItem liGeneral = new ListItem("Generic Properties", "0");
            if (_tabId == 0)
                liGeneral.Selected = true;
            ddlTab.Items.Add(liGeneral);

            // mandatory
            if (PropertyType != null && PropertyType.Mandatory)
                checkMandatory.Checked = true;

            // validation
            if (PropertyType != null && string.IsNullOrEmpty(PropertyType.ValidationRegExp) == false)
                tbValidation.Text = PropertyType.ValidationRegExp;

            // description
            if (PropertyType != null && PropertyType.Description != "")
                tbDescription.Text = PropertyType.GetRawDescription();
        }

        private void SetDefaultDocumentTypeProperty()
        {
            var itemToSelect = ddlTypes.Items.Cast<ListItem>()
                .FirstOrDefault(item => item.Text.ToLowerInvariant() == UmbracoConfig.For.UmbracoSettings().Content.DefaultDocumentTypeProperty.ToLowerInvariant());

            if (itemToSelect != null)
            {
                itemToSelect.Selected = true;
            }
            else
            {
                ddlTypes.SelectedIndex = -1;
            }
        }

        protected void defaultDeleteHandler(object sender, EventArgs e)
        {

        }

        override protected void OnInit(EventArgs e)
        {
            base.OnInit(e);

            DeleteButton.Click += DeleteButton_Click;
            DeleteButton2.Click += DeleteButton2_Click;
            Delete += defaultDeleteHandler;

            // [ClientDependency(ClientDependencyType.Javascript, "js/UmbracoCasingRules.aspx", "UmbracoRoot")]
            var loader = ClientDependency.Core.Controls.ClientDependencyLoader.GetInstance(new HttpContextWrapper(Context));
            var helper = new UrlHelper(new RequestContext(new HttpContextWrapper(Context), new RouteData()));
            loader.RegisterDependency(helper.GetCoreStringsControllerPath() + "ServicesJavaScript", ClientDependencyType.Javascript);
        }

        void DeleteButton2_Click(object sender, EventArgs e)
        {
            Delete(this, new EventArgs());
        }

        void DeleteButton_Click(object sender, EventArgs e)
        {
            Delete(this, new EventArgs());
        }

        /// <summary>
        /// DeleteButton2 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.LinkButton DeleteButton2;

        /// <summary>
        /// FullHeader control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Literal FullHeader;

        /// <summary>
        /// DeleteButton control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.LinkButton DeleteButton;

        /// <summary>
        /// Header control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.Literal Header;

        /// <summary>
        /// tbName control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox tbName;

        /// <summary>
        /// PropertyPanel1 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.PropertyPanel PropertyPanel1;

        /// <summary>
        /// tbAlias control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox tbAlias;

        /// <summary>
        /// PropertyPanel2 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.PropertyPanel PropertyPanel2;

        /// <summary>
        /// ddlTypes control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.DropDownList ddlTypes;

        /// <summary>
        /// PropertyPanel3 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.PropertyPanel PropertyPanel3;

        /// <summary>
        /// ddlTab control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.DropDownList ddlTab;

        /// <summary>
        /// PropertyPanel4 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.PropertyPanel PropertyPanel4;

        /// <summary>
        /// checkMandatory control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.CheckBox checkMandatory;

        /// <summary>
        /// PropertyPanel5 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.PropertyPanel PropertyPanel5;

        /// <summary>
        /// tbValidation control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox tbValidation;

        /// <summary>
        /// validationLink control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.HyperLink validationLink;

        /// <summary>
        /// PropertyPanel6 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.PropertyPanel PropertyPanel6;

        /// <summary>
        /// tbDescription control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.TextBox tbDescription;

        protected global::umbraco.uicontrols.PropertyPanel PropertyPanel7;

    }
}