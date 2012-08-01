using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using umbraco.interfaces;
using umbraco.presentation.templateControls;

namespace umbraco.presentation.LiveEditing.Modules.ItemEditing
{
    /// <summary>
    /// Control that lets the user edit page properties in Live Edit mode.
    /// </summary>
    /// <remarks>Keep this class internal, or it will cause problems since it's not a "real" data editor.</remarks>
    internal class PageElementEditor : Control, IDataWithPreview, IDataType, IDataEditor, ILiveEditingDataEditor
    {
        /// <summary>The item to edit.</summary>
        private Item m_Item;

        /// <summary>Textbox that edits the item.</summary>
        private TextBox m_FieldContents;

        /// <summary>
        /// Initializes a new instance of the <see cref="PageElementEditor"/> class.
        /// </summary>
        public PageElementEditor()
        {
            throw new ApplicationException("Internal class PageElementEditor should never be instantiated this way.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PageElementEditor"/> class.
        /// </summary>
        /// <param name="item">The item.</param>
        public PageElementEditor(Item item)
        {
            m_Item = item;
            EnsureChildControls();

            // does the field belong to the current page? (no node ID)
            if (String.IsNullOrEmpty(item.NodeId))
            {
                // get the current page's element
                Value = item.PageElements[item.Field];
            }
            else
            {
                // get the elements of the chosen page
                page itemPage = new page(content.Instance.XmlContent.GetElementById(item.NodeId.ToString()));
                Value = itemPage.Elements[item.Field];
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls
        /// that use composition-based implementation to create any child controls
        /// they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            m_FieldContents = new TextBox();
            Controls.Add(m_FieldContents);
        }

        #region IData Members

        /// <summary>
        /// Gets or sets the property id.
        /// </summary>
        /// <value>The property id.</value>
        public int PropertyId
        {
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Converts the data to XML.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The data as XML.</returns>
        public XmlNode ToXMl(System.Xml.XmlDocument data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public object Value
        {
            get
            {
                return m_FieldContents.Text;
            }
            set
            {
                m_FieldContents.Text = value.ToString();
            }
        }

        /// <summary>
        /// Creates a new value
        /// </summary>
        /// <param name="PropertyId">The property id.</param>
        public void MakeNew(int PropertyId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        public void Delete()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDataWithPreview Members

        /// <summary>
        /// Gets or sets a value indicating whether preview mode is switched on.
        /// In preview mode, the <see cref="Value"/> setter saves to a temporary location
        /// instead of persistent storage, which the getter also reads from on subsequent access.
        /// Switching off preview mode restores the persistent value.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if preview mode is switched on; otherwise, <c>false</c>.
        /// </value>
        public bool PreviewMode
        {
            get
            {
                return ViewState["PreviewMode"] == null ? false : (bool)ViewState["PreviewMode"];
            }
            set
            {
                ViewState["PreviewMode"] = value;
            }
        }

        #endregion

        #region IDataType Members

        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>The id.</value>
        public Guid Id
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the name of the data type.
        /// </summary>
        /// <value>The name of the data type.</value>
        public string DataTypeName
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the data editor.
        /// </summary>
        /// <value>The data editor.</value>
        public IDataEditor DataEditor
        {
            get { return this; }
        }

        /// <summary>
        /// Gets the prevalue editor.
        /// </summary>
        /// <value>The prevalue editor.</value>
        public IDataPrevalue PrevalueEditor
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <value>The data.</value>
        public IData Data
        {
            get { return this; }
        }

        /// <summary>
        /// Gets or sets the data type definition id.
        /// </summary>
        /// <value>The data type definition id.</value>
        public int DataTypeDefinitionId
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region IDataEditor Members

        /// <summary>
        /// Saves this instance.
        /// </summary>
        public void Save()
        { }

        /// <summary>
        /// Gets a value indicating whether a label is shown
        /// </summary>
        /// <value><c>true</c> if [show label]; otherwise, <c>false</c>.</value>
        public bool ShowLabel
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets a value indicating whether the editor should be treated as a rich text editor.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if [treat as rich text editor]; otherwise, <c>false</c>.
        /// </value>
        public bool TreatAsRichTextEditor
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the editor control
        /// </summary>
        /// <value>The editor.</value>
        public Control Editor
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region ILiveEditingDataEditor Members

        /// <summary>
        /// Gets the control used for Live Editing.
        /// </summary>
        /// <value></value>
        public Control LiveEditingControl
        {
            get { return this; }
        }

        #endregion
    }
}
