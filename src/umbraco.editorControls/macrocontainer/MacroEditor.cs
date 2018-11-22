using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;
using Umbraco.Core;
using Umbraco.Core.Macros;
using Umbraco.Core.ObjectResolution;
using umbraco.cms.businesslogic.macro;
using System.Collections;
using umbraco.presentation;
using System.Web;

namespace umbraco.editorControls.macrocontainer
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class MacroEditor : System.Web.UI.Control
    {
        private List<string> _allowedMacros;
        private DropDownList _macroSelectDropdown;
        private LinkButton _delete;
        private Table _formTable;
        private Hashtable _dataValues;
        private string _data;
        private LiteralControl propertiesHeader = new LiteralControl("<h4>" + ui.Text("macroContainerSettings") + " <span class=\"settingsToggle\"><a href=\"#\" onClick=\"jQuery(this).closest('.macroeditor').find('.macroSettings').toggle()\">Show/Hide</a></span> </h4> ");

        public MacroEditor(string Data, List<string> allowedMacros)
        {
            _data = Data;
            _allowedMacros = allowedMacros;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            _macroSelectDropdown = new DropDownList();
            _macroSelectDropdown.ID = ID + "_ddselectmacro";
            _macroSelectDropdown.SelectedIndexChanged += new EventHandler(_macroSelectDropdown_SelectedIndexChanged);
            _macroSelectDropdown.Items.Add(new ListItem(umbraco.ui.Text("choose"), ""));
            foreach (string item in _allowedMacros)
            {
                _macroSelectDropdown.Items.Add(new ListItem(GetMacroNameFromAlias(item), item));
            }
            _macroSelectDropdown.AutoPostBack = true;

            _delete = new LinkButton();
            _delete.CssClass = "macroDelete";
            _delete.ID = ID + "_btndelete";
            _delete.Text = ui.Text("removeMacro");
            _delete.Attributes.Add("style", "color:red;");
            _delete.Click += new EventHandler(_delete_Click);
            _formTable = new Table();
            _formTable.ID = ID + "_tblform";
            _formTable.CssClass = "macroSettings";

            propertiesHeader.Visible = false;
            this.Controls.Add(_delete);
            this.Controls.Add(_macroSelectDropdown);
            this.Controls.Add(propertiesHeader);
            this.Controls.Add(_formTable);
        }

        //using this to solve to eager loading,
        // replaces _macroSelectDropdown.Items.Add(new ListItem(Macro.GetByAlias(item).Name, item));
        // with  _macroSelectDropdown.Items.Add(new ListItem(GetMacroNameFromAlias(item), item));
        private string GetMacroNameFromAlias(string alias)
        {
            var macro = umbraco.macro.GetMacro(alias);

            return macro == null ? string.Empty : macro.Name;
        }
        
        void _delete_Click(object sender, EventArgs e)
        {
            _macroSelectDropdown.SelectedIndex = 0;
            InitializeForm("");
            this.Visible = false;
            MacroContainerEvent.Delete();
        }



        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {

                //Handle Initial Request
                if (DataValues["macroalias"] != null)
                {
                    //Data is available from the database, initialize the form with the data
                    string alias = DataValues["macroalias"].ToString();

                    //Set Pulldown selected value based on the macro alias
                    _macroSelectDropdown.SelectedValue = alias;

                    //Create from with values based on the alias
                    InitializeForm(alias);
                }
                else
                {
                    this.Visible = false;
                }

            }
            else
            {
                //Render form if properties are in the viewstate
                if (SelectedProperties.Count > 0)
                {
                    RendeFormControls();
                }
            }

            //Make sure child controls get rendered
            EnsureChildControls();

        }

        protected override void Render(HtmlTextWriter writer)
        {            
            writer.Write("<div id=\"container" + ID + "\" class=\"macroeditor\">");
            _delete.RenderControl(writer);
            writer.Write("<h4>Macro:</h4>");
            _macroSelectDropdown.RenderControl(writer);
            writer.Write(" ");//<a style=\"color: red;\" href=\"javascript:deletemacro('" + _macroSelectDropdown.ClientID + "','"+ID+"container' )\">Delete</a>");
            propertiesHeader.RenderControl(writer);
            _formTable.RenderControl(writer);
            writer.Write("</div>");
        }

        private void _macroSelectDropdown_SelectedIndexChanged(object sender, EventArgs e)
        {
            InitializeForm(_macroSelectDropdown.SelectedValue);

        }

        private void InitializeForm(string macroAlias)
        {

            //Hold selected Alias in Viewstate
            SelectedMacroAlias = macroAlias;

            //Create new property collection
            List<PersistableMacroProperty> props = new List<PersistableMacroProperty>();

            //Clear Old Control Collection
            _formTable.Controls.Clear();

            //Is a Macro selected
            if (!string.IsNullOrEmpty(macroAlias))
            {
                Macro formMacro = Macro.GetByAlias(macroAlias);

                ///Only render form when macro is found
                if (formMacro != null)
                {
                    if (formMacro.Properties.Length > 0)
                    {
                        propertiesHeader.Visible = true;
                    }
                    foreach (MacroProperty macroProperty in formMacro.Properties)
                    {
                        var prop = new PersistableMacroProperty();
                        prop.Alias = macroProperty.Alias;
                        prop.Name = macroProperty.Name;
                        prop.AssemblyName = macroProperty.Type.Assembly;
                        prop.TypeName = macroProperty.Type.Type;

                        //Assign value if specified
                        if (DataValues[macroProperty.Alias.ToLower()] != null)
                        {
                            prop.Value = DataValues[macroProperty.Alias.ToLower()].ToString();
                        }

                        props.Add(prop);
                    }
                }
            }
            //Hold selected properties in ViewState
            SelectedProperties = props;

            //Render the form
            RendeFormControls();
        }

        /// <summary>
        /// Renders a from based on the properties of the macro
        /// </summary>
        private void RendeFormControls()
        {
            foreach (PersistableMacroProperty prop in SelectedProperties)
            {
                //Create Literal
                Literal caption = new Literal();
                caption.ID = ID + "_" + string.Format("{0}Label", prop.Alias);
                caption.Text = prop.Name;

                //Get the MacroControl 
				Control macroControl = MacroFieldEditorsResolver.Current.GetMacroRenderControlByType(prop, ID + "_" + prop.Alias);

                AddFormRow(caption, macroControl);

            }
        }

        /// <summary>
        /// Add a new TableRow to the table. with two cells that holds the Caption and the  form element
        /// </summary>
        /// <param name="name"></param>
        /// <param name="formElement"></param>
        private void AddFormRow(Control name, Control formElement)
        {
            string aliasPrefix = formElement.ID;

            TableRow tr = new TableRow();
            tr.ID = ID + "_" + string.Format("{0}tr", aliasPrefix);

            TableCell tcLeft = new TableCell();
            tcLeft.ID = ID + "_" + string.Format("{0}tcleft", aliasPrefix);
            tcLeft.Width = 120;

            TableCell tcRight = new TableCell();
            tcRight.ID = ID + "_" + string.Format("{0}tcright", aliasPrefix);
            tcRight.Width = 300;

            tcLeft.Controls.Add(name);
            tcRight.Controls.Add(formElement);

            tr.Controls.Add(tcLeft);
            tr.Controls.Add(tcRight);

            _formTable.Controls.Add(tr);

        }

        /// <summary>
        /// Builds an Umbraco Macro tag if a user selected a macro
        /// </summary>
        /// <returns></returns>
        private string CreateUmbracoMacroTag()
        {
            string result = string.Empty;

            if (!string.IsNullOrEmpty(SelectedMacroAlias))
            {
                //Only create Macro Tag if we have something to store.
                StringBuilder sb = new StringBuilder();
                //Start
                sb.Append("<?UMBRACO_MACRO ");

                //Alias Attribute
                sb.AppendFormat(" macroalias=\"{0}\" ", SelectedMacroAlias);

                foreach (PersistableMacroProperty prop in SelectedProperties)
                {
                    //Make sure we find the correct Unique ID
                    string ControlIdToFind = ID + "_" + prop.Alias;
					string value = MacroFieldEditorsResolver.Current.GetValueFromMacroControl(_formTable.FindControl(ControlIdToFind));
                    sb.AppendFormat(" {0}=\"{1}\" ", prop.Alias, value);
                }
                sb.Append(" />");
                result = sb.ToString();
            }
            return result;
        }

        private string SelectedMacroAlias
        {

            get { return string.Format("{0}", ViewState[ID + "SelectedMacroAlias"]); }
            set { ViewState[ID + "SelectedMacroAlias"] = value; }
        }

        private List<PersistableMacroProperty> SelectedProperties
        {
            get
            {
                if (ViewState[ID + "controls"] == null)
                {
                    return new List<PersistableMacroProperty>();
                }
                else
                {
                    return (List<PersistableMacroProperty>)ViewState[ID + "controls"];
                }
            }
            set
            {
                ViewState[ID + "controls"] = value;
            }
        }

        public Hashtable DataValues
        {
            get
            {
                if (_dataValues == null)
                {
                    _dataValues = umbraco.helper.ReturnAttributes(_data);
                }
                return _dataValues;
            }
        }


        public string MacroTag
        {
            get
            {
                return CreateUmbracoMacroTag();
            }
        }
    }
}
