using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.presentation.LiveEditing.Modules;
using umbraco.presentation.LiveEditing.Modules.CreateModule;
using umbraco.presentation.LiveEditing.Modules.DeleteModule;
using umbraco.presentation.LiveEditing.Modules.ItemEditing;
//using umbraco.presentation.LiveEditing.Modules.MacroEditing;
using umbraco.presentation.LiveEditing.Modules.UnpublishModule;

using umbraco.BasePages;
using umbraco.cms.businesslogic.web;
using umbraco.BusinessLogic.Actions;
using umbraco.presentation.umbraco.controls;
using umbraco.presentation.ClientDependency;
namespace umbraco.presentation.LiveEditing.Controls
{
    /// <summary>
    /// The default toolbar used for Live Editing.
    /// </summary>
    [ClientDependency(1, ClientDependencyType.Javascript, "ui/jquery.js", "UmbracoClient")]
    [ClientDependency(20, ClientDependencyType.Javascript, "LiveEditing/Controls/LiveEditingToolbar.js", "UmbracoRoot")]
    public class LiveEditingToolbar : Control
    {
        #region Protected Constants

        /// <summary>Client ID of the toolbar.</summary>
        protected const string ToolbarId = "LiveEditingToolbar";

        /// <summary>Title of the toolbar.</summary>
        protected const string ToolbarTitle = "Umbraco Live Editing Toolbar";

        #endregion

        #region Private Fields

        private readonly LiveEditingManager m_Manager;

        private readonly UpdatePanel m_MainPanel = new UpdatePanel();
        private readonly LabelButton m_SaveButton = new LabelButton();
        private readonly LabelButton m_SaveAndPublishButton = new LabelButton();
        private readonly Button m_CloseButton = new Button();
        private readonly Panel m_MenuItemsPanel = new Panel();

        #endregion

        #region Public Constructor

        /// <summary>
        /// Creates a new default toolbar for the specified manager.
        /// </summary>
        /// <param name="manager">The manager.</param>
        public LiveEditingToolbar(LiveEditingManager manager)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");

            m_Manager = manager;
            AddModules();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Adds the modules to the toolbar.
        /// </summary>
        /// <remarks>Modules should be loaded from a configuration file in the future.</remarks>
        protected virtual void AddModules()
        {
            m_Manager.LiveEditingContext.Menu.Add(new Separator());
            m_Manager.LiveEditingContext.Menu.Add(new ItemEditor(m_Manager));
            m_Manager.LiveEditingContext.Menu.Add(new CreateModule(m_Manager));
            m_Manager.LiveEditingContext.Menu.Add(new UnpublishModule(m_Manager));
            m_Manager.LiveEditingContext.Menu.Add(new DeleteModule(m_Manager));
            //m_Manager.LiveEditingContext.Menu.Add(new MacroModule(m_Manager));
        }

        /// <summary>
        /// Refreshes the current page using a client redirect.
        /// </summary>
        protected virtual void RefreshPage()
        {
            // we're using a client redirect, to enable client balloon messages on exit
            string url = library.NiceUrl(UmbracoContext.Current.PageId.Value);
            ScriptManager.RegisterClientScriptBlock(Page, GetType(), new Guid().ToString(),
                                        String.Format("window.location.href='{0}';", url), true);
        }

        #endregion

        #region Control Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            EnsureChildControls();
            m_Manager.MessageReceived += Manager_MessageReceived;
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation
        /// to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Add(m_MainPanel);

            // create save button
            m_MainPanel.ContentTemplateContainer.Controls.Add(m_SaveButton);
            m_SaveButton.CssClass = "button";
            m_SaveButton.ToolTip = "Save";
            m_SaveButton.ImageUrl = String.Format("{0}/images/editor/Save.gif", GlobalSettings.Path);
            m_SaveButton.Click += new ImageClickEventHandler(SaveButton_Click);
            m_SaveButton.OnClientClick = "return LiveEditingToolbar._save()";
            m_SaveButton.Visible = UmbracoContext.Current.HasPermission(ActionUpdate.Instance.Letter);

            // create save and publish button
            m_MainPanel.ContentTemplateContainer.Controls.Add(m_SaveAndPublishButton);
            m_SaveAndPublishButton.CssClass = "button";
            m_SaveAndPublishButton.ToolTip = "Save and publish";
            m_SaveAndPublishButton.ImageUrl = String.Format("{0}/images/editor/SaveAndPublish.gif", GlobalSettings.Path);
            m_SaveAndPublishButton.Click += new ImageClickEventHandler(SaveAndPublishButton_Click);
            m_SaveAndPublishButton.OnClientClick = "return LiveEditingToolbar._saveAndPublish()";
            m_SaveAndPublishButton.Visible = m_SaveButton.Visible
                                             && UmbracoContext.Current.HasPermission(ActionPublish.Instance.Letter);

            
            // create close button
            string _btText = "Logout: " + new BasePage().getUser().Name;
            m_MainPanel.ContentTemplateContainer.Controls.Add(m_CloseButton);
            m_CloseButton.CssClass = "close";
            m_CloseButton.ToolTip = _btText;
            m_CloseButton.Text = _btText;

            //m_CloseButton.ImageUrl = String.Format("{0}/images/editor/Close.gif", GlobalSettings.Path);
            m_CloseButton.Click += new EventHandler(CloseButton_Click);

            // add the custom menu items
            foreach (Control menuItem in m_Manager.LiveEditingContext.Menu)
                m_MenuItemsPanel.Controls.Add(menuItem);
            m_MenuItemsPanel.CssClass = "ExtraMenuItems";
            m_MainPanel.ContentTemplateContainer.Controls.Add(m_MenuItemsPanel);

            // add the client toolbar
            m_MainPanel.ContentTemplateContainer.Controls.Add(new LiteralControl("<div id=\"LiveEditingClientToolbar\"></div>"));
        }

        /// <summary>
        /// Sends server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter"/> object,
        /// which writes the content to be rendered on the client.
        /// </summary>
        /// <param name="writer">
        ///     The <see cref="T:System.Web.UI.HtmlTextWriter"/> object
        ///     that receives the server control content.
        /// </param>
        protected override void Render(HtmlTextWriter writer)
        {
            // surround the control with a toolbar div
            writer.AddAttribute(HtmlTextWriterAttribute.Id, ToolbarId);
            writer.AddAttribute(HtmlTextWriterAttribute.Title, ToolbarTitle);
            writer.RenderBeginTag(HtmlTextWriterTag.Div);

            base.Render(writer);

            // close surrrounding div
            writer.RenderEndTag();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the SaveButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.ImageClickEventArgs"/> instance containing the event data.</param>
        protected void SaveButton_Click(object sender, ImageClickEventArgs e)
        {
            m_Manager.LiveEditingContext.Updates.SaveAll();
            m_Manager.DisplayUserMessage("Page saved", "The page was saved successfully. Remember to publish your changes.", "info");
        }

        /// <summary>
        /// Handles the Click event of the SaveAndPublishButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.ImageClickEventArgs"/> instance containing the event data.</param>
        protected void SaveAndPublishButton_Click(object sender, ImageClickEventArgs e)
        {
            // save and publish
            m_Manager.LiveEditingContext.Updates.SaveAll();
            m_Manager.LiveEditingContext.Updates.PublishAll();

            // show message to user
            m_Manager.DisplayUserMessage("Page published", "The page was published successfully.", "info");

            // redirect to page (updates everything, and is necessary if the page name was changed)
            RefreshPage();
        }

        /// <summary>
        /// Handles the Click event of the CloseButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.ImageClickEventArgs"/> instance containing the event data.</param>
        protected void CloseButton_Click(object sender, EventArgs e)
        {
            // disable Live Editing
            m_Manager.LiveEditingContext.Enabled = false;

            //ensures that users who are redirected to canvas also gets logged out on exit
            int userid = BasePages.UmbracoEnsuredPage.GetUserId(BasePages.UmbracoEnsuredPage.umbracoUserContextID);
            if (userid > 0) {
                BusinessLogic.User u = new global::umbraco.BusinessLogic.User(userid);
                if (u.DefaultToLiveEditing)
                    new BasePages.BasePage().ClearLogin();
            }

            RefreshPage();
        }


        /// <summary>
        /// Handles the MessageReceived event of the Manager control.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void Manager_MessageReceived(object sender, MesssageReceivedArgs e)
        {
            switch (e.Type)
            {
                case "save":
                    SaveButton_Click(sender, null);
                    break;
                case "saveAndPublish":
                    SaveAndPublishButton_Click(sender, null);
                    break;
            }
        }

        #endregion
    }
}
