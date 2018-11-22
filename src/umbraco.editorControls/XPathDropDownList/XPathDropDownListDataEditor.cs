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
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class XPathDropDownListDataEditor : CompositeControl, IDataEditor
    {
        /// <summary>
        /// Field for the data.
        /// </summary>
        private IData data;

        /// <summary>
        /// Field for the options.
        /// </summary>
        private XPathDropDownListOptions options;

        /// <summary>
        /// Field for the CustomValidator.
        /// </summary>
        private CustomValidator customValidator = new CustomValidator();

        /// <summary>
        /// Field for the DropDownList.
        /// </summary>
        private DropDownList dropDownList = new DropDownList();

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
            this.data = data;
            this.options = options;
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            switch (this.options.UmbracoObjectType)
            {
                case uQuery.UmbracoObjectType.Unknown:
                case uQuery.UmbracoObjectType.Document:

                    this.dropDownList.DataSource = uQuery.GetNodesByXPath(this.options.XPath).Where(x => x.Id != -1).ToNameIds();
                    break;

                case uQuery.UmbracoObjectType.Media:

                    this.dropDownList.DataSource = uQuery.GetMediaByXPath(this.options.XPath).Where(x => x.Id != -1).ToNameIds();
                    break;

                case uQuery.UmbracoObjectType.Member:

                    this.dropDownList.DataSource = uQuery.GetMembersByXPath(this.options.XPath).ToNameIds();
                    break;
            }
            
            this.dropDownList.DataTextField = "Value";
            this.dropDownList.DataValueField = this.options.UseId ? "Key" : "Value";
            this.dropDownList.DataBind();

            // Add a default please select value
            this.dropDownList.Items.Insert(0, new ListItem(string.Concat(ui.Text("choose"), "..."), "-1"));

            this.Controls.Add(this.customValidator);
            this.Controls.Add(this.dropDownList);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load"/> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.EnsureChildControls();

            if (!this.Page.IsPostBack && this.data.Value != null)
            {
                // Get selected items from Node Name or Node Id
                var dropDownListItem = this.dropDownList.Items.FindByValue(this.data.Value.ToString());
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
            Property property = new Property(((umbraco.cms.businesslogic.datatype.DefaultData)this.data).PropertyId);
            if (property.PropertyType.Mandatory && this.dropDownList.SelectedValue == "-1")
            {
                // Property is mandatory, but no value selected in the DropDownList
                this.customValidator.IsValid = false;

                DocumentType documentType = new DocumentType(property.PropertyType.ContentTypeId);
                ContentType.TabI tab = documentType.getVirtualTabs.Where(x => x.Id == property.PropertyType.TabId).FirstOrDefault();

                if (tab != null)
                {
                    this.customValidator.ErrorMessage = ui.Text("errorHandling", "errorMandatory", new string[] { property.PropertyType.Alias, tab.Caption }, User.GetCurrent());
                }
            }

            this.data.Value = this.dropDownList.SelectedValue;
        }
    }
}
