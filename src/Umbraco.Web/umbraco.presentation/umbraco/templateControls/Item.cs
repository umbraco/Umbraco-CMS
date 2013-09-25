using System;
using System.Collections;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core.Models;
using umbraco.BusinessLogic.Actions;

namespace umbraco.presentation.templateControls
{
    /// <summary>
    /// Control that renders an Umbraco item on a page.
    /// </summary>
    [DefaultProperty("Field")]
    [ToolboxData("<{0}:Item runat=\"server\"></{0}:Item>")]
	[Designer("umbraco.presentation.templateControls.ItemDesigner, umbraco")]
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

        /// <summary>
        /// Gets or sets the XPath expression used for the inline XSLT transformation.
        /// </summary>
        /// <value>
        /// The XPath expression, or an empty string to disable XSLT transformation.
        /// The code <c>{0}</c> is used as a placeholder for the rendered field contents.
        /// </value>
        [Bindable(true)]
        [Category("Umbraco")]
        [DefaultValue("")]
        [Localizable(true)]
        public string Xslt
        {
            get { return (string)ViewState["Xslt"] ?? String.Empty; }
            set { ViewState["Xslt"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether XML entity escaping of the XSLT transformation output is disabled.
        /// </summary>
        /// <value><c>true</c> HTML escaping is disabled; otherwise, <c>false</c> (default).</value>
        /// <remarks>
        ///     This corresponds value to the <c>disable-output-escaping</c> parameter
        ///     of the XSLT <c>value-of</c> element.
        /// </remarks>
        [Bindable(true)]
        [Category("Umbraco")]
        [DefaultValue(false)]
        [Localizable(true)]
        public bool XsltDisableEscaping
        {
            get { return ViewState["XsltEscape"] == null ? false : (bool)ViewState["XsltEscape"]; }
            set { ViewState["XsltEscape"] = value; }
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

        #region Overriden Control Methods
        
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
                string tempNodeId = helper.parseAttribute(PageElements, NodeId);
                int nodeIdInt = 0;
                if (int.TryParse(tempNodeId, out nodeIdInt))
                {
                    return nodeIdInt;
                }
            }
            else if (PageElements["pageID"] != null)
            {
                return int.Parse(PageElements["pageID"].ToString());
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
        
        /// <summary>
        /// Determines whether the field is a dictionary item.
        /// </summary>
        /// <returns><c>true</c> if the field is a dictionary item; otherwise, <c>false</c>.</returns>
        protected virtual bool FieldIsDictionaryItem()
        {
            return helper.FindAttribute(new AttributeCollectionAdapter(Attributes), "field").StartsWith("#");
        }

        /// <summary>
        /// Determines whether the field is recursive.
        /// </summary>
        /// <returns><c>true</c> if the field is recursive; otherwise, <c>false</c>.</returns>
        protected virtual bool FieldIsRercursive()
        {
            return helper.FindAttribute(new AttributeCollectionAdapter(Attributes), "recursive") == "true";
        }

        /// <summary>
        /// Determines whether field uses the API to lookup the value
        /// (if a NodeId attribute is specified and is different from the current page id).
        /// </summary>
        /// <returns><c>true</c> if API lookup is used; otherwise, <c>false</c>.</returns>
        [Obsolete("Method never implemented", true)]
        protected virtual bool FieldIsApiLookup()
        {
            // TODO: remove false and add security
            return false;
        }

        /// <summary>
        /// Gets a value indicating whether the current item is editable by the current user.
        /// </summary>
        /// <value><c>true</c> if the current item is editable by the current user; otherwise, <c>false</c>.</value>
        protected virtual bool FieldEditableWithUserPermissions()
        {
            BusinessLogic.User u = helper.GetCurrentUmbracoUser();
            return u != null && u.GetPermissions(PageElements["path"].ToString()).Contains(ActionUpdate.Instance.Letter.ToString());
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
