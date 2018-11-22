using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Umbraco.Core.Events
{
    public class ExportEventArgs<TEntity> : CancellableObjectEventArgs<IEnumerable<TEntity>>, IEquatable<ExportEventArgs<TEntity>>
    {
        /// <summary>
        /// Constructor accepting a single entity instance
        /// </summary>
        /// <param name="eventObject"></param>
        /// <param name="xml"></param>
        /// <param name="canCancel"></param>
        public ExportEventArgs(TEntity eventObject, XElement xml, bool canCancel)
            : base(new List<TEntity> { eventObject }, canCancel)
        {
            Xml = xml;
        }

        /// <summary>
        /// Constructor accepting a single entity instance
        /// and cancellable by default
        /// </summary>
        /// <param name="eventObject"></param>
        /// <param name="elementName"></param>
        public ExportEventArgs(TEntity eventObject, string elementName) : base(new List<TEntity> {eventObject}, true)
        {
            Xml = new XElement(elementName);
        }

        protected ExportEventArgs(IEnumerable<TEntity> eventObject, bool canCancel) : base(eventObject, canCancel)
        {
        }

        protected ExportEventArgs(IEnumerable<TEntity> eventObject) : base(eventObject)
        {
        }

        /// <summary>
        /// Returns all entities that were exported during the operation
        /// </summary>
        public IEnumerable<TEntity> ExportedEntities
        {
            get { return EventObject; }
        }

        /// <summary>
        /// Returns the xml relating to the export event
        /// </summary>
        public XElement Xml { get; private set; }

        public bool Equals(ExportEventArgs<TEntity> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Equals(Xml, other.Xml);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ExportEventArgs<TEntity>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (Xml != null ? Xml.GetHashCode() : 0);
            }
        }

        public static bool operator ==(ExportEventArgs<TEntity> left, ExportEventArgs<TEntity> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ExportEventArgs<TEntity> left, ExportEventArgs<TEntity> right)
        {
            return !Equals(left, right);
        }
    }
}
