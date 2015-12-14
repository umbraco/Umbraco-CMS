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
	public class CancellableObjectEventArgs<T> : CancellableEventArgs
	{

        public CancellableObjectEventArgs(T eventObject, bool canCancel, EventMessages eventMessages)
            : base(canCancel, eventMessages)
        {
            EventObject = eventObject;
        }

        public CancellableObjectEventArgs(T eventObject, EventMessages eventMessages)
            : this(eventObject, true, eventMessages)
        {
        }

        public CancellableObjectEventArgs(T eventObject, bool canCancel)
			: base(canCancel)
		{
			EventObject = eventObject;
		}

		public CancellableObjectEventArgs(T eventObject)
			: this(eventObject, true)
		{
		}

		/// <summary>
		/// Returns the object relating to the event
		/// </summary>
		/// <remarks>
		/// This is protected so that inheritors can expose it with their own name
		/// </remarks>
		protected T EventObject { get; set; }

	}
}