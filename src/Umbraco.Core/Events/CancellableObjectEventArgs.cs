using System;
using System.Security.Permissions;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Events
{
	/// <summary>
	/// Event args for a strongly typed object that can support cancellation
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
	public class CancellableObjectEventArgs<T> : EventArgs, ICancellableObjectEventArgs
	{
		private bool _cancel;

		public CancellableObjectEventArgs(T entity, bool canCancel)
		{
			Entity = entity;
			CanCancel = canCancel;
		}

		public CancellableObjectEventArgs(T entity)
			: this(entity, true)
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

		public T Entity { get; private set; }

	}
}