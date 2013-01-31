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

		public CancellableEventArgs(bool canCancel)
		{
			CanCancel = canCancel;
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
				if (!CanCancel)
				{
					throw new InvalidOperationException("This event argument class does not support cancelling.");
				}
				return _cancel;
			}
			set
			{
				if (!CanCancel)
				{
					throw new InvalidOperationException("This event argument class does not support cancelling.");
				}
				_cancel = value;
			}
		}
	}
}