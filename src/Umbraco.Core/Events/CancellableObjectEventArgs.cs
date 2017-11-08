using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Events
{
    /// <summary>
    /// Used as a base class for the generic type CancellableObjectEventArgs{T} so that we can get direct 'object' access to the underlying EventObject
    /// </summary>
    [HostProtection(SecurityAction.LinkDemand, SharedState = true)]
    public abstract class CancellableObjectEventArgs : CancellableEventArgs
    {
        protected CancellableObjectEventArgs(object eventObject, bool canCancel, EventMessages messages, IDictionary<string, object> additionalData)
            : base(canCancel, messages, additionalData)
        {
            EventObject = eventObject;
        }

        protected CancellableObjectEventArgs(object eventObject, bool canCancel, EventMessages eventMessages)
            : base(canCancel, eventMessages)
        {
            EventObject = eventObject;
        }

        protected CancellableObjectEventArgs(object eventObject, EventMessages eventMessages)
            : this(eventObject, true, eventMessages)
        {
        }

        protected CancellableObjectEventArgs(object eventObject, bool canCancel)
            : base(canCancel)
        {
            EventObject = eventObject;
        }

        protected CancellableObjectEventArgs(object eventObject)
            : this(eventObject, true)
        {
        }

        /// <summary>
        /// Returns the object relating to the event
        /// </summary>
        /// <remarks>
        /// This is protected so that inheritors can expose it with their own name
        /// </remarks>
        internal object EventObject { get; set; }

    }

    /// <summary>
    /// Event args for a strongly typed object that can support cancellation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [HostProtection(SecurityAction.LinkDemand, SharedState = true)]
    public class CancellableObjectEventArgs<T> : CancellableObjectEventArgs, IEquatable<CancellableObjectEventArgs<T>>
    {
        public CancellableObjectEventArgs(T eventObject, bool canCancel, EventMessages messages, IDictionary<string, object> additionalData)
            : base(eventObject, canCancel, messages, additionalData)
        {
        }

        public CancellableObjectEventArgs(T eventObject, bool canCancel, EventMessages eventMessages)
            : base(eventObject, canCancel, eventMessages)
        {
        }

        public CancellableObjectEventArgs(T eventObject, EventMessages eventMessages)
            : base(eventObject, eventMessages)
        {
        }

        public CancellableObjectEventArgs(T eventObject, bool canCancel)
            : base(eventObject, canCancel)
        {
        }

        public CancellableObjectEventArgs(T eventObject)
            : base(eventObject)
        {
        }

        /// <summary>
        /// Returns the object relating to the event
        /// </summary>
        /// <remarks>
        /// This is protected so that inheritors can expose it with their own name
        /// </remarks>
        protected new T EventObject
        {
            get { return (T) base.EventObject; }
            set { base.EventObject = value; }
        }

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
            return Equals((CancellableObjectEventArgs<T>)obj);
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

    [HostProtection(SecurityAction.LinkDemand, SharedState = true)]
    public class CancellableEnumerableObjectEventArgs<T> : CancellableObjectEventArgs<IEnumerable<T>>, IEquatable<CancellableEnumerableObjectEventArgs<T>>
    {
        public CancellableEnumerableObjectEventArgs(IEnumerable<T> eventObject, bool canCancel, EventMessages messages, IDictionary<string, object> additionalData)
            : base(eventObject, canCancel, messages, additionalData)
        { }

        public CancellableEnumerableObjectEventArgs(IEnumerable<T> eventObject, bool canCancel, EventMessages eventMessages)
            : base(eventObject, canCancel, eventMessages)
        { }

        public CancellableEnumerableObjectEventArgs(IEnumerable<T> eventObject, EventMessages eventMessages)
            : base(eventObject, eventMessages)
        { }

        public CancellableEnumerableObjectEventArgs(IEnumerable<T> eventObject, bool canCancel)
            : base(eventObject, canCancel)
        { }

        public CancellableEnumerableObjectEventArgs(IEnumerable<T> eventObject)
            : base(eventObject)
        { }

        public bool Equals(CancellableEnumerableObjectEventArgs<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return EventObject.SequenceEqual(other.EventObject);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CancellableEnumerableObjectEventArgs<T>)obj);
        }

        public override int GetHashCode()
        {
            return HashCodeHelper.GetHashCode(EventObject);
        }
    }
}