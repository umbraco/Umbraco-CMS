using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.XPath;
// using uComponents.DataTypes.Shared.Extensions;
// using uComponents.DataTypes.Shared.PrevalueEditors;
using umbraco.cms.businesslogic.datatype;

namespace umbraco.editorControls.XPathDropDownList
{
    class XPathDropDownListPreValueEditor : AbstractJsonPrevalueEditor
    {
        /// <summary>
        /// TextBox control to get the XPath expression
        /// </summary>
        private TextBox xPathTextBox = new TextBox();

        /// <summary>
        /// RequiredFieldValidator to ensure an XPath expression has been entered
        /// </summary>
        private RequiredFieldValidator xPathRequiredFieldValidator = new RequiredFieldValidator();

        /// <summary>
        /// Server side validation of XPath expression, to ensure some nodes are returned
        /// </summary>
        private CustomValidator xPathCustomValidator = new CustomValidator();

        /// <summary>
        /// Drop Down List to pick either Node Name or Node Id
        /// </summary>
        private DropDownList valueTypeDropDownList = new DropDownList();

        /// <summary>
        /// Data object used to define the configuration status of this PreValueEditor
        /// </summary>
        private XPathDropDownListOptions options = null;

        /// <summary>
        /// Gets the options data object that represents the current state of this datatypes configuration
        /// </summary>
        internal XPathDropDownListOptions Options
        {
            get
            {
                if (this.options == null)
                {
                    // Deserialize any stored settings for this PreValueEditor instance
                    this.options = this.GetPreValueOptions<XPathDropDownListOptions>();

                    // If still null, ie, object couldn't be de-serialized from PreValue[0] string value
                    if (this.options == null)
                    {
                        // Create a new Options data object with the default values
                        this.options = new XPathDropDownListOptions();
                    }
                }
                return this.options;
            }
        }

        /// <summary>
        /// Initialize a new instance of XPathCheckBoxlistPreValueEditor
        /// </summary>
        /// <param name="dataType">XPathCheckBoxListDataType</param>
        public XPathDropDownListPreValueEditor(umbraco.cms.businesslogic.datatype.BaseDataType dataType)
            : base(dataType, umbraco.cms.businesslogic.datatype.DBTypes.Nvarchar)
        {
        }

        /// <summary>
        /// Creates all of the controls and assigns all of their properties
        /// </summary>
        protected override void CreateChildControls()
        {
            this.xPathTextBox.ID = "xPathTextBox";
            this.xPathTextBox.CssClass = "umbEditorTextField";

            this.xPathRequiredFieldValidator.ControlToValidate = this.xPathTextBox.ID;
            this.xPathRequiredFieldValidator.Display = ValidatorDisplay.Dynamic;
            this.xPathRequiredFieldValidator.ErrorMessage = " XPath expression required";

            this.xPathCustomValidator.ControlToValidate = this.xPathTextBox.ID;
            this.xPathCustomValidator.Display = ValidatorDisplay.Dynamic;
            this.xPathCustomValidator.ServerValidate += new ServerValidateEventHandler(XPathCustomValidator_ServerValidate);

            this.valueTypeDropDownList.ID = "valueTypeDropDownList";
            this.valueTypeDropDownList.Items.Add(new ListItem("Node Id", bool.TrueString));
            this.valueTypeDropDownList.Items.Add(new ListItem("Node Name", bool.FalseString));

            this.Controls.Add(this.xPathTextBox);
            this.Controls.Add(this.xPathRequiredFieldValidator);
            this.Controls.Add(this.xPathCustomValidator);
            this.Controls.Add(this.valueTypeDropDownList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //if (!this.Page.IsPostBack)
            //{
            // Read in stored configuration values
            this.xPathTextBox.Text = this.Options.XPath;
            this.valueTypeDropDownList.SelectedValue = this.Options.UseId.ToString();
            //}
        }

        /// <summary>
        /// Will run the entered XPath expression to ensure it returns at least 1 node
        /// </summary>
        /// <param name="source">xPathCustomValidator</param>
        /// <param name="args"></param>
        private void XPathCustomValidator_ServerValidate(object source, ServerValidateEventArgs args)
        {
            string xPath = args.Value;
            bool isValid = false;

            try
            {
                if (uQuery.GetNodesByXPath(xPath).Count >= 0)
                {
                    isValid = true;
                }
            }
            catch (XPathException)
            {
                this.xPathCustomValidator.ErrorMessage = " Syntax error in XPath expression";
            }

            args.IsValid = isValid;
        }

        /// <summary>
        /// Saves the pre value data to Umbraco
        /// </summary>
        public override void Save()
        {
            if (this.Page.IsValid)
            {
                this.Options.XPath = this.xPathTextBox.Text;
                this.Options.UseId = bool.Parse(this.valueTypeDropDownList.SelectedValue);

                this.SaveAsJson(this.Options);  // Serialize to Umbraco database field
            }
        }

        /// <summary>
        /// Replaces the base class writer and instead uses the shared uComponents extension method, to inject consistant markup
        /// </summary>
        /// <param name="writer"></param>
        protected override void RenderContents(HtmlTextWriter writer)
        {
            writer.AddPrevalueRow("XPath Expression", this.xPathTextBox, this.xPathRequiredFieldValidator, this.xPathCustomValidator);
            writer.AddPrevalueRow("Value", this.valueTypeDropDownList);
        }

    }
}