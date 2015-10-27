using System;
using System.Security.Permissions;

namespace Umbraco.Core.Events
{
    /// <summary>
	/// Event args for that can support cancellation
	/// </summary>
	[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
	public class CancellableEventArgs : EventArgs
	{
		private bool _cancel;

        public CancellableEventArgs(bool canCancel, EventMessages eventMessages)
        {
            if (eventMessages == null) throw new ArgumentNullException("eventMessages");
            CanCancel = canCancel;
            Messages = eventMessages;
        }

        public CancellableEventArgs(bool canCancel)
		{
			CanCancel = canCancel;
            //create a standalone messages
            Messages = new EventMessages();
        }

        public CancellableEventArgs(EventMessages eventMessages)
            : this(true, eventMessages)
        {
        }

        public CancellableEventArgs()
			: this(true)
		{
		}

		/// <summary>
		/// Flag to determine if this instance will support being cancellable
		/// </summary>
		public bool CanCancel { get; set; }

		/// <summary>
		/// If this instance supports cancellation, this gets/sets the cancel value
		/// </summary>
		public bool Cancel
		{
			get
			{
				if (CanCancel == false)
				{
					throw new InvalidOperationException("This event argument class does not support cancelling.");
				}
				return _cancel;
			}
			set
			{
				if (CanCancel == false)
				{
					throw new InvalidOperationException("This event argument class does not support cancelling.");
				}
				_cancel = value;
			}
		}

        /// <summary>
        /// if this instance supports cancellation, this will set Cancel to true with an affiliated cancellation message
        /// </summary>
        /// <param name="cancelationMessage"></param>
        public void CancelOperation(EventMessage cancelationMessage)
        {
            Cancel = true;
            cancelationMessage.IsDefaultEventMessage = true;
            Messages.Add(cancelationMessage);
        }

        /// <summary>
        /// Returns the EventMessages object which is used to add messages to the message collection for this event
        /// </summary>
        public EventMessages Messages { get; private set; }
    }
}