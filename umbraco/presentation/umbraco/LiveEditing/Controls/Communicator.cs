using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using ClientDependency.Core;

namespace umbraco.presentation.LiveEditing.Controls
{
    /// <summary>
    /// Control that can receive messages from the client.
    /// </summary>
	[ClientDependency(100, ClientDependencyType.Javascript, "LiveEditing/Controls/Communicator.js", "UmbracoRoot")]
    public class Communicator : Control
    {
        /// <summary>
        /// Occurs when the communicator receives a message from the client.
        /// </summary>
        public event EventHandler<MesssageReceivedArgs> MessageReceived;

        private UpdatePanel m_MainPanel;
        private TextBox m_TypeBox;
        private TextBox m_MessageBox;
        private Button m_SubmitButton;

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            EnsureChildControls(); 
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls 
        /// that use composition-based implementation to create any child controls
        /// they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            m_MainPanel = new UpdatePanel();
            m_MainPanel.RenderMode = UpdatePanelRenderMode.Inline;

            // The next two lines ensure that nothing is sent to the client,
            // as there's no information to send anyway. This reduces overhead.
            m_MainPanel.UpdateMode = UpdatePanelUpdateMode.Conditional;
            m_MainPanel.ChildrenAsTriggers = false;

            Controls.Add(m_MainPanel);

            m_TypeBox = new TextBox();
            m_TypeBox.ID = "Type";
            m_TypeBox.EnableViewState = false;
            m_TypeBox.ValidationGroup = "Communicator";
            m_MainPanel.ContentTemplateContainer.Controls.Add(m_TypeBox);

            m_MessageBox = new TextBox();
            m_MessageBox.ID = "Message";
            m_MessageBox.EnableViewState = false;
            m_MessageBox.ValidationGroup = "Communicator";
            m_MainPanel.ContentTemplateContainer.Controls.Add(m_MessageBox);

            m_SubmitButton = new Button();
            m_SubmitButton.ID = "Submit";
            m_MessageBox.EnableViewState = false;
            m_SubmitButton.ValidationGroup = "Communicator";
            m_SubmitButton.Click += new EventHandler(SubmitButton_Click);
            m_MainPanel.ContentTemplateContainer.Controls.Add(m_SubmitButton);

        }

        /// <summary>
        /// Handles the Click event of the m_SubmitButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            if (MessageReceived != null)
                MessageReceived(this, new MesssageReceivedArgs(m_TypeBox.Text, m_MessageBox.Text));
        }

        /// <summary>
        /// Sends server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter"/> object,
        /// which writes the content to be rendered on the client.
        /// </summary>
        /// <param name="writer">
        ///     The <see cref="T:System.Web.UI.HtmlTextWriter"/>
        ///     object that receives the server control content.
        /// </param>
        protected override void Render(HtmlTextWriter writer)
        {
            // Write opening tag, not using RenderBeginTag to avoid whitespace being rendered.
            // This makes m_MainPanel the first element of the div on all browsers.
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "communicator");
            writer.WriteBeginTag("div");
            writer.WriteAttribute("class", "communicator");
            writer.WriteAttribute("style", "display: none;");
            writer.Write(HtmlTextWriter.TagRightChar);

            m_MainPanel.RenderControl(writer);

            writer.WriteEndTag("div");
        }
    }

    /// <summary>
    /// Arguments for the <see cref="MessageReceived"/> event.
    /// </summary>
    public class MesssageReceivedArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the type of the message.
        /// </summary>
        /// <value>The type.</value>
        public virtual string Type { get; protected set; }

        /// <summary>
        /// Gets or sets the message text.
        /// </summary>
        /// <value>The message.</value>
        public virtual string Message { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MesssageReceivedArgs"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="message">The message text.</param>
        public MesssageReceivedArgs(string type, string message)
        {
            Type = type;
            Message = message;
        }
    }
}
