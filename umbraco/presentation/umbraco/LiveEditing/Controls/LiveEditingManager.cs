using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.presentation.LiveEditing.Modules.ItemEditing;
using umbraco.cms.businesslogic.datatype;

namespace umbraco.presentation.LiveEditing.Controls
{
    /// <summary>
    /// Control that manages Live Editing on a certain page.
    /// Provides public properties, events and methods for Live Editing controls.
    /// Add this control to a (master) page to enable Live Editing.
    /// </summary>
    [ClientDependency(1, ClientDependencyType.Css, "LiveEditing/CSS/LiveEditing.css", true)]
    [ClientDependency(1, ClientDependencyType.Javascript, "/umbraco_client/ui/jquery.js", false, "_jQueryNoConflict = function(){jQuery.noConflict();}")]
    [ClientDependency(2, ClientDependencyType.Javascript, "js/UmbracoSpeechBubble.js", true)]
    public class LiveEditingManager : Control
    {

        #region Private Fields

        /// <summary>The associated Live Editing context.</summary>
        private readonly ILiveEditingContext m_Context;

        /// <summary>Toolbar instance.</summary>
        private LiveEditingToolbar m_Toolbar;

        /// <summary>Communicator instance.</summary>
        private Communicator m_Communicator;

        /// <summary>Placeholder to which dynamic output can be added.</summary>
        private PlaceHolder m_Output;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the associated Live Editing context.
        /// </summary>
        /// <value>The Live Editing context.</value>
        public ILiveEditingContext LiveEditingContext
        {
            get { return m_Context; }
        }

        #endregion

        #region Public Events

        /// <summary>
        /// Occurs when a message is received from the client.
        /// </summary>
        public event EventHandler<MesssageReceivedArgs> MessageReceived
        {
            add    { m_Communicator.MessageReceived += value; }
            remove { m_Communicator.MessageReceived -= value; }
        }

        #endregion

        #region Public Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="LiveEditingManager"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public LiveEditingManager(ILiveEditingContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (!context.Enabled)
                throw new ApplicationException("Live Editing is not enabled.");

            m_Context = context;
        }

        #endregion

        #region Control Members

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            EnsureChildControls();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ClientDependencyHelper.AddClientDependencies(this);
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls
        /// that use composition-based implementation to create any child controls
        /// they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            m_Communicator = new Communicator();
            Controls.Add(m_Communicator);

            m_Toolbar = new LiveEditingToolbar(this);
            Controls.Add(m_Toolbar);

            UpdatePanel m_OutputWrapper = new UpdatePanel();
            Controls.Add(m_OutputWrapper);
            m_Output = new PlaceHolder();
            m_OutputWrapper.ContentTemplateContainer.Controls.Add(m_Output);
        }

        #endregion

        #region Public Methods
        
        /// <summary>
        /// Adds a control to the client output.
        /// </summary>
        /// <param name="output">The control to add.</param>
        public virtual void AddClientOutput(Control output)
        {
            m_Output.Controls.Add(output);
        }

        /// <summary>
        /// Shows a balloon message on the client side.
        /// </summary>
        /// <param name="title">Unescaped title text.</param>
        /// <param name="message">Unescaped message text.</param>
        /// <param name="icon">The icon.</param>
        public virtual void DisplayUserMessage(string title, string message, string icon)
        {
            ScriptManager.RegisterClientScriptBlock(Page, GetType(), new Guid().ToString(),
                string.Format("UmbSpeechBubble.ShowMessage('{2}','{0}','{1}');",
                    EscapeJavascriptString(title), EscapeJavascriptString(message), EscapeJavascriptString(icon)), true);
        }
 
        #endregion

        #region Protected Methods
        
        /// <summary>
        /// Escapes a Javascript string for use inside a single quoted Javascript literal.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The escaped Javascript string.</returns>
        /// <remarks>Escaping only concerns slashes, returns and single quotes. This might not be complete.</remarks>
        protected virtual string EscapeJavascriptString(string input)
        {
            return input.Replace(@"\", @"\\").Replace("\n", @"\n").Replace(@"'", @"\'");
        }

        #endregion
    }
}
