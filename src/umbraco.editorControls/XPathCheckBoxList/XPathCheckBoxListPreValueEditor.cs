using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.XPath;
using umbraco.cms.businesslogic.datatype;
using System.Linq;

namespace umbraco.editorControls.XPathCheckBoxList
{
	/// <summary>
	/// This PreValueEditor will require an XPath expression to define the nodes to pick as CheckBox options,
	/// TODO: [HR] min / max selections ?
	/// Uses the shared JsonPreValueEditor as nice way of lightweight serializing a config data class object into a single DB field
	/// </summary>
	class XPathCheckBoxListPreValueEditor : AbstractJsonPrevalueEditor
	{
		/// <summary>
		/// DropDownList for specifying the database column type.
		/// </summary>
		private DropDownList dbTypeDropDownList = new DropDownList();

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
		/// Store an Xml fragment or a Csv
		/// </summary>
		private RadioButtonList storageTypeRadioButtonList = new RadioButtonList() { RepeatDirection = RepeatDirection.Vertical, RepeatLayout = RepeatLayout.Flow };

		/// <summary>
		/// Select Node IDs or Node Names as the values to store
		/// </summary>
		private DropDownList valueTypeDropDownList = new DropDownList();

		/// <summary>
		/// Data object used to define the configuration status of this PreValueEditor
		/// </summary>
		private XPathCheckBoxListOptions options = null;

		/// <summary>
		/// Gets the options data object that represents the current state of this datatypes configuration
		/// </summary>
		internal XPathCheckBoxListOptions Options
		{
			get
			{
				if (this.options == null)
				{
					// Deserialize any stored settings for this PreValueEditor instance
					this.options = this.GetPreValueOptions<XPathCheckBoxListOptions>();

					// If still null, ie, object couldn't be de-serialized from PreValue[0] string value
					if (this.options == null)
					{
						// Create a new Options data object with the default values
						this.options = new XPathCheckBoxListOptions(true);
					}
				}
				return this.options;
			}
		}

		/// <summary>
		/// Initialize a new instance of XPathCheckBoxlistPreValueEditor
		/// </summary>
		/// <param name="dataType">XPathCheckBoxListDataType</param>
        public XPathCheckBoxListPreValueEditor(umbraco.cms.businesslogic.datatype.BaseDataType dataType)
			: base(dataType)
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

			this.storageTypeRadioButtonList.ID = "storageTypeRadioButtonList";
			this.storageTypeRadioButtonList.Items.Add(new ListItem("Xml", bool.TrueString));
			this.storageTypeRadioButtonList.Items.Add(new ListItem("Csv", bool.FalseString));

			this.valueTypeDropDownList.ID = "valueTypeDropDownList";
			this.valueTypeDropDownList.Items.Add(new ListItem("Node Ids", bool.TrueString));
			this.valueTypeDropDownList.Items.Add(new ListItem("Node Names", bool.FalseString));

			this.Controls.AddPrevalueControls(
				this.dbTypeDropDownList,
				this.xPathTextBox,
				this.xPathRequiredFieldValidator,
				this.xPathCustomValidator,
				this.storageTypeRadioButtonList,
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
			this.xPathTextBox.Text = this.Options.XPath;
			this.storageTypeRadioButtonList.SelectedValue = this.Options.UseXml.ToString();
			this.valueTypeDropDownList.SelectedValue = this.Options.UseIds.ToString();
		}

		/// <summary>
		/// Will run the entered XPath expression to ensure it's valid
		/// </summary>
		/// <param name="source">xPathCustomValidator</param>
		/// <param name="args"></param>
		private void XPathCustomValidator_ServerValidate(object source, ServerValidateEventArgs args)
		{
			string xPath = args.Value;
			bool isValid = false;

			try
			{
				if (uQuery.GetNodesByXPath(xPath).Count() >= 0)
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
                // always use NText
                base.m_DataType.DBType = cms.businesslogic.datatype.DBTypes.Ntext;

				this.Options.XPath = this.xPathTextBox.Text;
				this.Options.UseXml = bool.Parse(this.storageTypeRadioButtonList.SelectedValue);
				this.Options.UseIds = bool.Parse(this.valueTypeDropDownList.SelectedValue);

				this.SaveAsJson(this.Options);  // Serialize to Umbraco database field
			}
		}

		/// <summary>
		/// Replaces the base class writer and instead uses the shared uComponents extension method, to inject consistant markup
		/// </summary>
		/// <param name="writer"></param>
		protected override void RenderContents(HtmlTextWriter writer)
		{
			//writer.AddPrevalueRow("Database Type", this.dbTypeDropDownList);
            writer.AddPrevalueRow("XPath Expression", @"can use the tokens <strong>$ancestorOrSelf</strong>, <strong>$parentPage</strong> and <strong>$currentPage</strong>, eg.<br />
                                                        <br />                                            
                                                        all siblings: $parentPage//*[@id != $currentPage/@id] <br />                                                        
                                                        ", this.xPathTextBox, this.xPathRequiredFieldValidator, this.xPathCustomValidator);
			writer.AddPrevalueRow("Storage Type", this.storageTypeRadioButtonList);
			writer.AddPrevalueRow("Values", this.valueTypeDropDownList);
		}
	}
}
