using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Web;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Web.Actions;
using Umbraco.Web.Composing;
using Umbraco.Web.Macros;


namespace umbraco.presentation.templateControls
{
    /// <summary>
    /// Control that renders an Umbraco item on a page.
    /// </summary>
    [DefaultProperty("Field")]
    [ToolboxData("<{0}:Item runat=\"server\"></{0}:Item>")]
    [Designer("umbraco.presentation.templateControls.ItemDesigner, Umbraco.Web")]
    public class Item : CompositeControl
    {

        #region Private Fields

        /// <summary>The item's unique ID on the page.</summary>
        private readonly int m_ItemId;
        public AttributeCollectionAdapter LegacyAttributes;
        #endregion

        /// <summary>
        /// Used by the UmbracoHelper to assign an IPublishedContent to the Item which allows us to pass this in
        /// to the 'item' ctor so that it renders with the new API instead of the old one.
        /// </summary>
        internal IPublishedContent ContentItem { get; private set; }

        #region Public Control Properties

        /// <summary>
        /// Gets or sets the field name.
        /// </summary>
        /// <value>The field name.</value>
        [Bindable(true)]
        [Category("Umbraco")]
        [DefaultValue("")]
        [Localizable(true)]
        public string Field
        {
            get { return (string)ViewState["Field"] ?? String.Empty; }
            set { ViewState["Field"] = value; }
        }

        /// <summary>
        /// Gets or sets the node id expression.
        /// </summary>
        /// <value>The node id expression.</value>
        [Bindable(true)]
        [Category("Umbraco")]
        [DefaultValue("")]
        [Localizable(true)]
        public string NodeId
        {
            get { return (string)ViewState["NodeId"] ?? String.Empty; }
            set { ViewState["NodeId"] = value; }
        }

        /// <summary>
        /// Gets or sets the text to display if the control is empty.
        /// </summary>
        /// <value>The text to display if the control is empty.</value>
        [Bindable(true)]
        [Category("Umbraco")]
        [DefaultValue("")]
        [Localizable(true)]
        public string TextIfEmpty
        {
            get { return (string)ViewState["TextIfEmpty"] ?? String.Empty; }
            set { ViewState["TextIfEmpty"] = value; }
        }
        
        [Bindable(true)]
        [Category("Umbraco")]
        [DefaultValue("")]
        [Localizable(true)]
        public bool DebugMode
        {
            get { return ((ViewState["DebugMode"] == null) ? false : (bool)ViewState["DebugMode"]); }
            set { ViewState["DebugMode"] = value; }
        }

        public ItemRenderer Renderer { get; set; }

        #endregion

        #region Public Readonly Properties

        /// <summary>
        /// Gets the item's unique ID on the page.
        /// </summary>
        /// <value>The item id.</value>
        public int ItemId
        {
            get { return m_ItemId; }
        }

        /// <summary>
        /// Gets the Umbraco page elements.
        /// </summary>
        /// <value>The Umbraco page elements.</value>
        public Hashtable PageElements
        {
            get
            {
                return Context.Items["pageElements"] as Hashtable;
            }
        }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Item"/> class.
        /// </summary>
        public Item()
        {
            Renderer = ItemRenderer.Instance;
        }

        /// <summary>
        /// Internal ctor used to assign an IPublishedContent object.
        /// </summary>
        /// <param name="contentItem"></param>
        internal Item(IPublishedContent contentItem)
            :this()
        {
            ContentItem = contentItem;
        }

        #endregion

        #region Overridden Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            Attributes.Add("field", Field);
            LegacyAttributes = new AttributeCollectionAdapter(Attributes);
            Renderer.Init(this);

            base.OnInit(e);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load"/> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            Renderer.Load(this);

            base.OnLoad(e);
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl"/> content
        /// to the specified <see cref="T:System.Web.UI.HtmlTextWriter"/> object, for display on the client.
        /// </summary>
        /// <param name="writer">
        ///     An <see cref="T:System.Web.UI.HtmlTextWriter"/> that represents
        ///     the output stream to render HTML content on the client.
        /// </param>
        protected override void Render(HtmlTextWriter writer)
        {
            Renderer.Render(this, writer);
        }

        #endregion

        #region Helper Functions

        /// <summary>
        /// Gets the parsed node id. As a nodeid on a item element can be null, an integer or even a squarebracket syntax, this helper method
        /// is handy for getting the exact parsed nodeid back.
        /// </summary>
        /// <returns>The parsed nodeid, the id of the current page OR null if it's not specified</returns>
        public int? GetParsedNodeId()
        {
            if (!String.IsNullOrEmpty(NodeId))
            {
                string tempNodeId = MacroRenderer.ParseAttribute(PageElements, NodeId);
                int nodeIdInt = 0;
                if (int.TryParse(tempNodeId, out nodeIdInt))
                {
                    return nodeIdInt;
                }
            }
            else if (UmbracoContext.Current.PageId != null)
            {
                return UmbracoContext.Current.PageId.Value;
            }
            return null;
        }


        /// <summary>
        /// Gets a value indicating whether this control is inside the form tag.
        /// </summary>
        /// <returns><c>true</c> if this control is inside the form tag; otherwise, <c>false</c>.</returns>
        protected virtual bool IsInsideFormTag()
        {
            // check if this control has an ancestor that is the page form
            for (Control parent = this.Parent; parent != null; parent = parent.Parent)
                if (parent == Page.Form)
                    return true;

            return false;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return String.Format("Item {0} (NodeId '{1}' : {2})", ItemId, NodeId, Field);
        }

        #endregion

        #region Field Information Functions

        static string FindAttribute(IDictionary attributes, string key)
        {
            key = key.ToLowerInvariant();
            var attributeValue = attributes.Contains(key) ? attributes[key].ToString() : string.Empty;
            return MacroRenderer.ParseAttribute(null, attributeValue);
        }

        /// <summary>
        /// Determines whether the field is a dictionary item.
        /// </summary>
        /// <returns><c>true</c> if the field is a dictionary item; otherwise, <c>false</c>.</returns>
        protected virtual bool FieldIsDictionaryItem()
        {
            return FindAttribute(new AttributeCollectionAdapter(Attributes), "field").StartsWith("#");
        }

        /// <summary>
        /// Determines whether the field is recursive.
        /// </summary>
        /// <returns><c>true</c> if the field is recursive; otherwise, <c>false</c>.</returns>
        protected virtual bool FieldIsRercursive()
        {
            return FindAttribute(new AttributeCollectionAdapter(Attributes), "recursive") == "true";
        }


        /// <summary>
        /// Gets a value indicating whether the current item is editable by the current user.
        /// </summary>
        /// <value><c>true</c> if the current item is editable by the current user; otherwise, <c>false</c>.</value>
        protected virtual bool FieldEditableWithUserPermissions()
        {
            var u = UmbracoContext.Current.Security.CurrentUser;
            if (u == null) return false;
            var permission = Current.Services.UserService.GetPermissions(u, PageElements["path"].ToString());

            return permission.AssignedPermissions.Contains(ActionUpdate.ActionLetter.ToString(CultureInfo.InvariantCulture), StringComparer.Ordinal);
        }

        #endregion
    }

    public class ItemDesigner : System.Web.UI.Design.ControlDesigner {

        private Item m_control;

        public override string GetDesignTimeHtml()
        {
            m_control =  this.Component as Item;
            return returnMarkup(String.Format("Getting '{0}'", m_control.Field));
        }

        private string returnMarkup(string message)
        {
            return "<span style=\"background-color: #DDD; padding: 2px;\">" + message  + "</span>";
        }

        protected override string GetErrorDesignTimeHtml(Exception e)
        {
            if (this.Component != null) {
                m_control = this.Component as Item;
            }

            if (m_control != null && !String.IsNullOrEmpty(m_control.Field))
            {
                return returnMarkup(String.Format("Getting '{0}'", m_control.Field));
            }
            else { return returnMarkup("<em>Please add a Field property</em>"); }
        }
    }
}
