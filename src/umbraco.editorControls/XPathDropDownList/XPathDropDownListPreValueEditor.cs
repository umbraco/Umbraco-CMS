using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.XPath;
using umbraco.cms.businesslogic.datatype;

namespace umbraco.editorControls.XPathDropDownList
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    class XPathDropDownListPreValueEditor : AbstractJsonPrevalueEditor
    {
        /// <summary>
        /// Radio buttons to select type of node to pick from: Content / Media / Members
        /// </summary>
        private RadioButtonList typeRadioButtonList = new RadioButtonList();

        /// <summary>
        /// TextBox control to get the XPath expression
        /// </summary>
        private TextBox xPathTextBox = new TextBox();

        /// <summary>
        /// RequiredFieldValidator to ensure an XPath expression has been entered
        /// </summary>
        private RequiredFieldValidator xPathRequiredFieldValidator = new RequiredFieldValidator();

        /// <summary>
        /// Server side validation of XPath expression
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
            //radio buttons to select type of nodes that can be picked (Document, Media or Member)
            this.typeRadioButtonList.Items.Add(new ListItem(uQuery.UmbracoObjectType.Document.GetFriendlyName(), uQuery.UmbracoObjectType.Document.GetGuid().ToString()));
            this.typeRadioButtonList.Items.Add(new ListItem(uQuery.UmbracoObjectType.Media.GetFriendlyName(), uQuery.UmbracoObjectType.Media.GetGuid().ToString()));
            this.typeRadioButtonList.Items.Add(new ListItem(uQuery.UmbracoObjectType.Member.GetFriendlyName(), uQuery.UmbracoObjectType.Member.GetGuid().ToString()));

            this.xPathTextBox.ID = "xPathTextBox";
            this.xPathTextBox.CssClass = "umbEditorTextField";

            this.xPathRequiredFieldValidator.ControlToValidate = this.xPathTextBox.ID;
            this.xPathRequiredFieldValidator.Display = ValidatorDisplay.Dynamic;
            this.xPathRequiredFieldValidator.ErrorMessage = " XPath expression required";

            this.xPathCustomValidator.ControlToValidate = this.xPathTextBox.ID;
            this.xPathCustomValidator.Display = ValidatorDisplay.Dynamic;
            this.xPathCustomValidator.ServerValidate += new ServerValidateEventHandler(this.XPathCustomValidator_ServerValidate);

            this.valueTypeDropDownList.ID = "valueTypeDropDownList";
            this.valueTypeDropDownList.Items.Add(new ListItem("Node Id", bool.TrueString));
            this.valueTypeDropDownList.Items.Add(new ListItem("Node Name", bool.FalseString));

            this.Controls.AddPrevalueControls(
                this.typeRadioButtonList,
                this.xPathTextBox,
                this.xPathRequiredFieldValidator,
                this.xPathCustomValidator,
                this.valueTypeDropDownList);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Read in stored configuration values
            this.typeRadioButtonList.SelectedValue = this.Options.Type;
            this.xPathTextBox.Text = this.Options.XPath;
            this.valueTypeDropDownList.SelectedValue = this.Options.UseId.ToString();
        }

        /// <summary>
        /// Will run the entered XPath expression to ensure it returns a collection
        /// </summary>
        /// <param name="source">xPathCustomValidator</param>
        /// <param name="args"></param>
        private void XPathCustomValidator_ServerValidate(object source, ServerValidateEventArgs args)
        {
            string xPath = args.Value;
            bool isValid = false;

            try
            {
                switch (this.options.UmbracoObjectType)
                {
                    case uQuery.UmbracoObjectType.Document:
                        if (uQuery.GetNodesByXPath(xPath).Count() >= 0)
                        {
                            isValid = true;
                        }

                        break;

                    case uQuery.UmbracoObjectType.Media:
                        if (uQuery.GetMediaByXPath(xPath).Count() >= 0)
                        {
                            isValid = true;
                        }

                        break;

                    case uQuery.UmbracoObjectType.Member:
                        if (uQuery.GetMembersByXPath(xPath).Count() >= 0)
                        {
                            isValid = true;
                        }

                        break;
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
                this.Options.Type = this.typeRadioButtonList.SelectedValue;
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
            writer.AddPrevalueRow(
                        "Type", 
                        "the xml schema to query", 
                        this.typeRadioButtonList);

            writer.AddPrevalueRow(
                        "XPath Expression", 
                        "can use the tokens <strong>$ancestorOrSelf</strong>, <strong>$parentPage</strong> and <strong>$currentPage</strong>",                                                        
                        this.xPathTextBox, 
                        this.xPathRequiredFieldValidator, 
                        this.xPathCustomValidator);
            
            writer.AddPrevalueRow(
                        "Value", 
                        "store the node id or the name",
                        this.valueTypeDropDownList);
        }

    }
}