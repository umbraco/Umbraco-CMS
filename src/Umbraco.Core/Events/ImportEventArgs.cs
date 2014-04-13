using System.Collections.Generic;
using System.Xml.Linq;

namespace Umbraco.Core.Events
{
    public class ImportEventArgs<TEntity> : CancellableObjectEventArgs<IEnumerable<TEntity>>
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
    }
}