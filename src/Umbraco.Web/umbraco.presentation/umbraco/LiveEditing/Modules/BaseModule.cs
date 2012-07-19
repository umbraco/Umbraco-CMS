using System;
using System.Web.UI;
using umbraco.presentation.LiveEditing.Controls;

namespace umbraco.presentation.LiveEditing.Modules
{
    /// <summary>
    /// Base class that Live Editing modules can choose to extend, providing some common features.
    /// Note that inheriting this control is not a requirement for modules.
    /// </summary>
    public abstract class BaseModule : Control
    {
        /// <summary>
        /// Gets the current Live Editing manager.
        /// </summary>
        /// <value>The current Live Editing manager.</value>
        protected LiveEditingManager Manager { get; private set; }

        /// <summary>
        /// Gets the current Live Editing context.
        /// </summary>
        /// <value>The current Live Editing context.</value>
        protected ILiveEditingContext LiveEditingContext
        {
            get { return Manager.LiveEditingContext; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseModule"/> class.
        /// </summary>
        /// <param name="manager">The Live Editing manager.</param>
        public BaseModule(LiveEditingManager manager)
        {
            if (manager == null)
                throw new ArgumentNullException("manager");
            if (!manager.LiveEditingContext.Enabled)
                throw new ApplicationException("Live Editing is not enabled.");

            Manager = manager;
            Manager.MessageReceived += Manager_MessageReceived;
        }

        /// <summary>
        /// Handles the <c>MessageReceived</c> event of the Live Editing manager.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected virtual void Manager_MessageReceived(object sender, MesssageReceivedArgs e) { }
    }
}
