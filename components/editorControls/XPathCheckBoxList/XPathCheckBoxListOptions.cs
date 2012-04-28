using System.ComponentModel;
using umbraco.cms.businesslogic.datatype;

namespace umbraco.editorControls.XPathCheckBoxList
{
	/// <summary>
	/// Data Class, used to store the configuration options for the XPathCheckBoxListPreValueEditor
	/// </summary>
	public class XPathCheckBoxListOptions : AbstractOptions
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="XPathCheckBoxListOptions"/> class.
		/// </summary>
		public XPathCheckBoxListOptions()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XPathCheckBoxListOptions"/> class.
		/// </summary>
		/// <param name="loadDefaults">if set to <c>true</c> [load defaults].</param>
		public XPathCheckBoxListOptions(bool loadDefaults)
			: base(loadDefaults)
		{
		}

		/// <summary>
		/// XPath string used to get Nodes to be used as CheckBox options in a CheckBoxList
		/// </summary>
		[DefaultValue("")]
		public string XPath { get; set; }

		/// <summary>
		/// Defaults to true, where the property value will be stored as an Xml Fragment, else if false, a Csv will be stored
		/// </summary>
		[DefaultValue(true)]
		public bool UseXml { get; set; }

		/// <summary>
		/// Defaults to true, where property value stored is NodeIds, else if false, then value stored is the Node Names
		/// </summary>
		[DefaultValue(true)]
		public bool UseIds { get; set; }
	}
}