using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.propertytype;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;
using umbraco.presentation.LiveEditing.Controls;
using umbraco.presentation.LiveEditing.Updates;
using umbraco.presentation.templateControls;
using umbraco.presentation.ClientDependency;
using umbraco.presentation.ClientDependency.Controls;
using umbraco.presentation.ClientDependency.Providers;
namespace umbraco.presentation.LiveEditing.Modules.ItemEditing
{
    /// <summary>
    /// Control that wraps the editor control to edit a field in Live Editing mode.
    /// </summary>
    [ClientDependency(1, ClientDependencyType.Javascript, "ui/jquery.js", "UmbracoClient")]
	[ClientDependency(21, ClientDependencyType.Javascript, "LiveEditing/Modules/ItemEditing/ItemEditing.js", "UmbracoRoot", InvokeJavascriptMethodOnLoad = "initializeGlobalItemEditing")]
	[ClientDependency(21, ClientDependencyType.Javascript, "tinymce3/tiny_mce_src.js", "UmbracoClient")] //For TinyMCE to work in LiveEdit, we need to ensure that this script is run before it loads...
    public class ItemEditor : UpdatePanel
    {
        #region Protected Constants
        
        /// <summary>Name of the wrapper tag.</summary>
        protected const string EditorWrapperTag = "umbraco:control";

        #endregion

        #region Private Fields

        /// <summary>The current Live Editing manager.</summary>
        private LiveEditingManager m_Manager;

        /// <summary>Control that will contain the editor control.</summary>
        private PlaceHolder m_Container;

        /// <summary>Data type of the field.</summary>
        private IDataType m_DataType;

        #endregion

        #region Public Properties
        
        /// <summary>
        /// Gets or sets the ID of the item that is to be edited.
        /// </summary>
        /// <value>The item ID, or 0 if no item is to be edited.</value>
        public virtual int ItemId
        {
            get { return ViewState["ItemId"] == null ? 0 : (int)ViewState["ItemId"]; }
            protected set { ViewState["ItemId"] = value; }
        } 

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets the current Live Editing context.
        /// </summary>
        /// <value>The Live Editing context.</value>
        protected ILiveEditingContext LiveEditingContext
        {
            get { return UmbracoContext.Current.LiveEditingContext; }
        }

        #endregion

        #region Public Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemEditor"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        public ItemEditor(LiveEditingManager manager)
        {
            m_Manager = manager;
        }

        #endregion

        #region Editing Methods
        
        /// <summary>
        /// Starts editing the item with the specified ID.
        /// </summary>
        /// <param name="itemId">The item ID.</param>
        public virtual void StartEditing(int itemId)
        {
            if (ItemId != 0)
                throw new ApplicationException(string.Format("Unable to edit item {0}: edit of item {1} still in progress.", itemId, ItemId));

            // the Render method will render the correct controls later on
            ItemId = itemId;

            // add initializing call to client script
            ScriptManager.RegisterStartupScript(Page, GetType(), new Guid().ToString(),
                                                string.Format("ItemEditing.startEdit({0});", ItemId), true);

			EnsureChildControls();
        }

        /// <summary>
        /// Stops editing the current item.
        /// </summary>
        /// <remarks>Currently triggered by the client.</remarks>
        protected virtual void StopEditing()
        {
            if (ItemId != 0)
            {
                Debug.Assert(HasControls());

                if (m_DataType != null)
                {
                    // load the postback data into the data editor
                    m_DataType.DataEditor.Save();

                    // store the update with the new data
                    Item item = FindItem(ItemId);
                    ItemUpdate update = new ItemUpdate(item.GetParsedNodeId(), item.Field, m_DataType.Data.Value);
                    LiveEditingContext.Updates.Add(update);
                }

                ItemId = 0;
            }
        }

        #endregion

        #region Control Overrides

        /// <summary>
        /// Raises the <see cref="E:Init"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            ID = "ItemEditor";
            RenderMode = UpdatePanelRenderMode.Inline;

			m_Manager.MessageReceived += Manager_MessageReceived;
			m_Manager.LiveEditingContext.Updates.UpdateAdded += Updates_UpdateAdded;
        }
        
        /// <summary>
        /// Raises the <see cref="E:Load"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // create editor controls if in edit modoe
			//this NEVER fires because onload occurs before ItemId is set in StartEditing
            if (ItemId != 0)
            {
                EnsureChildControls();
            }

            // enable editing on all items
            foreach (Item item in Utility.FindControls<Item>(Page.Master))
                item.Renderer = LiveEditingItemRenderer.Instance;
            
        }


        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls
        /// that use composition-based implementation to create any child controls
        /// they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
 

            if (ItemId != 0)
            {
                // find the item to edit
                Item item = FindItem(ItemId);

                if (item == null)
                {
                    // If the item is not found, two things could have occured:
                    //  1) the item ID is invalid (unlikely)
                    //  2) the item is still to be created as the result of an XSLT macro inside another item
                    // In both cases we can now safely parse already existing Item controls,
                    // as their values will not have been updated by the current postback.
                    // Plus we can solve case 2 because parsing makes those items available.

                    // force rendition of all macros of page Item controls
                    foreach (Item findItem in Utility.FindControls<Item>(i => i.CanUseLiveEditing, Page.Master))
                        ((LiveEditingItemRenderer)findItem.Renderer).ForceParseMacros(findItem);
                    // try looking up the item again
                    item = FindItem(ItemId);
                }

                // if the item is still not found, the item ID must be invalid
                if (item == null)
                    throw new ApplicationException(string.Format("The item with ID {0} was not found.", ItemId));

                // add main container
                m_Container = new PlaceHolder();
				m_Container.ID = "Container";
                ContentTemplateContainer.Controls.Add(m_Container);

                // add editor control, surrounded by a wrapper
                HtmlGenericControl editorControlWrapper = new EditorWrapper();
				editorControlWrapper.ID = "EditorControlWrapper";
                m_Container.Controls.Add(editorControlWrapper);
                Control dataEditor = CreateFieldDataEditor(item);
				dataEditor.ID = "DataEditor";
                editorControlWrapper.Controls.Add(dataEditor);

                // add hidden submit button to the container
                Button submit = new Button();
                // important: set ID so postback always works
                submit.ID = "ItemEditor_Submit";
                submit.Style[HtmlTextWriterStyle.Display] = "none";
                submit.Click += new EventHandler(Submit_Click);
                submit.EnableViewState = false;
                submit.ValidationGroup = "ItemEditor";
                m_Container.Controls.Add(submit);
            }
        }

        #endregion

        #region Protected Helper Methods
        
        /// <summary>
        /// Gets the editor control for the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The editor control.</returns>
        protected virtual Control CreateFieldDataEditor(Item item)
        {
            m_DataType = GetDataType(item);

            if (m_DataType != null)
            {
                Control editor;

                if (m_DataType.DataEditor is ILiveEditingDataEditor)
                {
                    ILiveEditingDataEditor dataEditor = (ILiveEditingDataEditor)m_DataType.DataEditor;
                    editor = dataEditor.LiveEditingControl;
                }
                else
                {
                    editor = m_DataType.DataEditor.Editor;
                }

                return editor;
            }
            else
            {
                return new LiteralControl("Live Editing of this field is not possible.");
            }
        }

        /// <summary>
        /// Gets the datatype for the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>The datatype.</returns>
        protected virtual IDataType GetDataType(Item item)
        {
            // find the property
            Property editProperty = null;
            if (item.GetParsedNodeId().HasValue)
            {
                Document document = new Document(item.GetParsedNodeId().Value);
                editProperty = document.getProperty(item.Field);
            }

            // find the data type
            IDataType editDataType = null;
            if (editProperty != null)
            {
                PropertyType editPropertyType = editProperty.PropertyType;
                editDataType = editPropertyType.DataTypeDefinition.DataType;
            }
            else
            {
                if (item.PageElements[item.Field] != null)
                {
                    editDataType = new PageElementEditor(item);
                }
            }

            // only allow editing if we the data can be previewed
            IDataWithPreview data = editDataType.Data as IDataWithPreview;
            if (data == null)
            {
                editDataType = null;
            }
            else
            {
                if (editProperty != null)
                    data.PropertyId = editProperty.Id;
                data.PreviewMode = true;
                // display the latest updated value if available
                ItemUpdate latestUpdate
                    = LiveEditingContext.Updates.GetLatest<ItemUpdate>(u => u.NodeId == item.GetParsedNodeId()
                                                                         && u.Field == item.Field);
                if (latestUpdate != null)
                    data.Value = latestUpdate.Data;
            }

            return editDataType;
        }

        /// <summary>
        /// Finds the item width the specified ID.
        /// </summary>
        /// <param name="id">The item ID.</param>
        /// <returns></returns>
        protected virtual Item FindItem(int id)
        {
            return (Item)Utility.FindControl<Item>(item => item.ItemId == id, Page.Master);
        }

        #endregion

        #region Event Handlers
        
        /// <summary>
        /// Handles the Click event of the Submit button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Submit_Click(object sender, EventArgs e)
        {
            StopEditing();
        }

        /// <summary>
        /// Handles the <c>MessageReceived</c> event of the manager.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void Manager_MessageReceived(object sender, MesssageReceivedArgs e)
        {
            switch (e.Type)
            {
                case "edititem":
                    StartEditing(int.Parse(e.Message));
                    break;
            }
        }

        /// <summary>
        /// Occurs when a new update is added to the Live Editing update collection.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="UpdateAddedEventArgs"/> instance containing the event data.</param>
        protected void Updates_UpdateAdded(object sender, UpdateAddedEventArgs e)
        {
            ItemUpdate update = e.Update as ItemUpdate;
            if (update != null)
            {
                List<Item> affectedItems = Utility.FindControls<Item>(item => item.GetParsedNodeId() == update.NodeId
                                                                           && item.Field == update.Field,
                                                                      Page.Master);
                foreach (Item affectedItem in affectedItems)
                {
                    // render the item, temporarily disabling the live editing special output
                    StringWriter itemRenderOutput = new StringWriter();
                    bool liveEditingWasDisabled = affectedItem.LiveEditingDisabled;
                    affectedItem.LiveEditingDisabled = true;
                    affectedItem.RenderControl(new HtmlTextWriter(itemRenderOutput));
                    affectedItem.LiveEditingDisabled = liveEditingWasDisabled;

                    // add the item's contents to the output
                    HtmlGenericControl itemUpdate = new HtmlGenericControl("umbraco:itemupdate");
                    itemUpdate.Attributes["itemId"] = affectedItem.ItemId.ToString();
                    itemUpdate.InnerHtml = template.ParseInternalLinks(itemRenderOutput.ToString());
                    itemUpdate.EnableViewState = false;
                    m_Manager.AddClientOutput(itemUpdate);
                }
            }
        }

        #endregion

        /// <summary>
        /// Custom tag that wraps the editor control.
        /// This class needs to implement <see cref="INamingContainer"/> to enable correct ViewState handling
        /// of the wrapped editor.
        /// </summary>
        protected class EditorWrapper : HtmlGenericControl, INamingContainer
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="EditorWrapper"/> class.
            /// </summary>
            public EditorWrapper()
                : base(ItemEditor.EditorWrapperTag)
            {
                ID = "EditorWrapper";
                Style[HtmlTextWriterStyle.Display] = "none";
            }
        }
    }
}
