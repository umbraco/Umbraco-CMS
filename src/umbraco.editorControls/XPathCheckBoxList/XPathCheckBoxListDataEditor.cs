using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using umbraco.interfaces;

namespace umbraco.editorControls.XPathCheckBoxList
{
    /// <summary>
    /// Renders a CheckBoxList using with option nodes obtained by an XPath expression
    /// </summary>
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class XPathCheckBoxListDataEditor : CompositeControl, IDataEditor
    {
        /// <summary>
        /// Field for the data.
        /// </summary>
        private IData data;

        /// <summary>
        /// Field for the options.
        /// </summary>
        private XPathCheckBoxListOptions options;

        /// <summary>
        /// Field for the checkbox list.
        /// </summary>
        private CheckBoxList checkBoxList = new CheckBoxList();

        /// <summary>
        /// Gets a value indicating whether [treat as rich text editor].
        /// </summary>
        /// <value>
        ///     <c>true</c> if [treat as rich text editor]; otherwise, <c>false</c>.
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
        internal XPathCheckBoxListDataEditor(IData data, XPathCheckBoxListOptions options)
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

                    this.checkBoxList.DataSource = uQuery.GetNodesByXPath(this.options.XPath).Where(x => x.Id != -1).ToNameIds();
                    break;

                case uQuery.UmbracoObjectType.Media:

                    this.checkBoxList.DataSource = uQuery.GetMediaByXPath(this.options.XPath).Where(x => x.Id != -1).ToNameIds();
                    break;

                case uQuery.UmbracoObjectType.Member:

                    this.checkBoxList.DataSource = uQuery.GetMembersByXPath(this.options.XPath).ToNameIds();
                    break;
            }

            this.checkBoxList.DataTextField = "Value";
            this.checkBoxList.DataValueField = this.options.UseIds ? "Key" : "Value";
            this.checkBoxList.DataBind();

            this.Controls.Add(this.checkBoxList);
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
                string value = this.data.Value.ToString();
                List<string> selectedValues = new List<string>();

                if (xmlHelper.CouldItBeXml(value))
                {
                    // build selected values from XML fragment
                    foreach (XElement nodeXElement in XElement.Parse(value).Elements())
                    {
                        selectedValues.Add(nodeXElement.Value);
                    }
                }
                else
                {
                    // Assume a CSV source
                    selectedValues = value.Split(',').ToList();
                }

                // Find checkboxes where values match the stored values and set to selected
                ListItem checkBoxListItem = null;
                foreach (string selectedValue in selectedValues)
                {
                    checkBoxListItem = this.checkBoxList.Items.FindByValue(selectedValue);
                    if (checkBoxListItem != null)
                    {
                        checkBoxListItem.Selected = true;
                    }
                }
            }
        }

        /// <summary>
        /// Called by Umbraco when saving the node
        /// </summary>
        public void Save()
        {
            // Get all checked item values
            IEnumerable<string> selectedOptions = from ListItem item in this.checkBoxList.Items
                                                    where item.Selected
                                                    select item.Value;

            if (this.options.UseXml)
            {
                string elementName = this.options.UseIds ? "nodeId" : "nodeName";

                this.data.Value = new XElement(
                                        "XPathCheckBoxList",
                                        selectedOptions.Select(x => new XElement(elementName, x.ToString())))
                                        .ToString();
            }
            else
            {
                // Save the CSV
                this.data.Value = string.Join(",", selectedOptions.ToArray());
            }
        }
    }
}
