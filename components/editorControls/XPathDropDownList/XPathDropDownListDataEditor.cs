using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;

namespace umbraco.editorControls.XPathDropDownList
{
	/// <summary>
	/// XPath configurabale DropDownList Data Type
	/// </summary>
	public class XPathDropDownListDataEditor : CompositeControl, IDataEditor
	{
		/// <summary>
		/// Field for the data.
		/// </summary>
		private IData m_Data;

		/// <summary>
		/// Field for the options.
		/// </summary>
		private XPathDropDownListOptions m_Options;

		/// <summary>
		/// Field for the CustomValidator.
		/// </summary>
		private CustomValidator m_CustomValidator = new CustomValidator();

		/// <summary>
		/// Field for the DropDownList.
		/// </summary>
		private DropDownList m_DropDownList = new DropDownList();

		/// <summary>
		/// Gets a value indicating whether [treat as rich text editor].
		/// </summary>
		/// <value>
		/// 	<c>true</c> if [treat as rich text editor]; otherwise, <c>false</c>.
		/// </value>
		public virtual bool TreatAsRichTextEditor
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Gets a value indicating whether [show label].
		/// </summary>
		/// <value><c>true</c> if [show label]; otherwise, <c>false</c>.</value>
		public virtual bool ShowLabel
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Gets the editor.
		/// </summary>
		/// <value>The editor.</value>
		public Control Editor
		{
			get
			{
				return this;
			}
		}

		/// <summary>
		/// Initializes a new instance of XPathCheckBoxListDataEditor
		/// </summary>
		/// <param name="data"></param>
		/// <param name="options"></param>
		internal XPathDropDownListDataEditor(IData data, XPathDropDownListOptions options)
		{
			this.m_Data = data;
			this.m_Options = options;
		}

		/// <summary>
		/// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
		/// </summary>
		protected override void CreateChildControls()
		{
			this.m_DropDownList.DataSource = uQuery.GetNodesByXPath(this.m_Options.XPath).ToNameIds();
			this.m_DropDownList.DataTextField = "Value";
			this.m_DropDownList.DataValueField = this.m_Options.UseId ? "Key" : "Value";
			this.m_DropDownList.DataBind();

			// Add a default please select value
			this.m_DropDownList.Items.Insert(0, new ListItem(string.Empty, "-1"));

			this.Controls.Add(this.m_CustomValidator);
			this.Controls.Add(this.m_DropDownList);
		}

		/// <summary>
		/// Raises the <see cref="E:System.Web.UI.Control.Load"/> event.
		/// </summary>
		/// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			this.EnsureChildControls();

			if (!this.Page.IsPostBack)
			{
				// Get selected items from Node Name or Node Id
				var dropDownListItem = this.m_DropDownList.Items.FindByValue(this.m_Data.Value.ToString());
				if (dropDownListItem != null)
				{
					dropDownListItem.Selected = true;
				}
			}
		}

		/// <summary>
		/// Called by Umbraco when saving the node
		/// </summary>
		public void Save()
		{
			Property property = new Property(((DefaultData)this.m_Data).PropertyId);
			if (property.PropertyType.Mandatory && this.m_DropDownList.SelectedValue == "-1")
			{
				// Property is mandatory, but no value selected in the DropDownList
				this.m_CustomValidator.IsValid = false;

				DocumentType documentType = new DocumentType(property.PropertyType.ContentTypeId);
				ContentType.TabI tab = documentType.getVirtualTabs.Where(x => x.Id == property.PropertyType.TabId).FirstOrDefault();

				if (tab != null)
				{
					this.m_CustomValidator.ErrorMessage = ui.Text("errorHandling", "errorMandatory", new string[] { property.PropertyType.Alias, tab.Caption }, User.GetCurrent());
				}
			}

			this.m_Data.Value = this.m_DropDownList.SelectedValue;
		}
	}
}
