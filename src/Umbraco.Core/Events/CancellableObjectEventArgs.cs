using System;
using System.Collections.Generic;
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
	public class CancellableObjectEventArgs<T> : CancellableEventArgs, IEquatable<CancellableObjectEventArgs<T>>
	{
	    public CancellableObjectEventArgs(T eventObject, bool canCancel, EventMessages messages, IDictionary<string, object> additionalData)
            : base(canCancel, messages, additionalData)
	    {
            EventObject = eventObject;
        }

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

	    public bool Equals(CancellableObjectEventArgs<T> other)
	    {
	        if (ReferenceEquals(null, other)) return false;
	        if (ReferenceEquals(this, other)) return true;
	        return base.Equals(other) && EqualityComparer<T>.Default.Equals(EventObject, other.EventObject);
	    }

	    public override bool Equals(object obj)
	    {
	        if (ReferenceEquals(null, obj)) return false;
	        if (ReferenceEquals(this, obj)) return true;
	        if (obj.GetType() != this.GetType()) return false;
	        return Equals((CancellableObjectEventArgs<T>) obj);
	    }

	    public override int GetHashCode()
	    {
	        unchecked
	        {
	            return (base.GetHashCode() * 397) ^ EqualityComparer<T>.Default.GetHashCode(EventObject);
	        }
	    }

	    public static bool operator ==(CancellableObjectEventArgs<T> left, CancellableObjectEventArgs<T> right)
	    {
	        return Equals(left, right);
	    }

	    public static bool operator !=(CancellableObjectEventArgs<T> left, CancellableObjectEventArgs<T> right)
	    {
	        return !Equals(left, right);
	    }
	}
}