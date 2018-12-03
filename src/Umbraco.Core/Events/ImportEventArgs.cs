using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Umbraco.Core.Events
{
    public class ImportEventArgs<TEntity> : CancellableEnumerableObjectEventArgs<TEntity>, IEquatable<ImportEventArgs<TEntity>>
    {
        /// <summary>
        /// Constructor accepting an XElement with the xml being imported
        /// </summary>
        /// <param name="xml"></param>
        public ImportEventArgs(XElement xml) : base(new List<TEntity>(), true)
        {
            Xml = xml;
        }

        /// <summary>
        /// Constructor accepting a list of entities and an XElement with the imported xml
        /// </summary>
        /// <param name="eventObject"></param>
        /// <param name="xml"></param>
        /// <param name="canCancel"></param>
        public ImportEventArgs(IEnumerable<TEntity> eventObject, XElement xml, bool canCancel)
            : base(eventObject, canCancel)
        {
            Xml = xml;
        }

        protected ImportEventArgs(IEnumerable<TEntity> eventObject, bool canCancel) : base(eventObject, canCancel)
        {
        }

        protected ImportEventArgs(IEnumerable<TEntity> eventObject) : base(eventObject)
        {
        }

        /// <summary>
        /// Returns all entities that were imported during the operation
        /// </summary>
        public IEnumerable<TEntity> ImportedEntities
        {
            get { return EventObject; }
        }

        /// <summary>
        /// Returns the xml relating to the import event
        /// </summary>
        public XElement Xml { get; private set; }

        public bool Equals(ImportEventArgs<TEntity> other)
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
            return Equals((ImportEventArgs<TEntity>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (Xml != null ? Xml.GetHashCode() : 0);
            }
        }

        public static bool operator ==(ImportEventArgs<TEntity> left, ImportEventArgs<TEntity> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ImportEventArgs<TEntity> left, ImportEventArgs<TEntity> right)
        {
            return !Equals(left, right);
        }
    }
}
