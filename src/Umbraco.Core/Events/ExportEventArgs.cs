using System.Collections.Generic;
using System.Xml.Linq;

namespace Umbraco.Core.Events
{
    public class ExportEventArgs<TEntity> : CancellableObjectEventArgs<IEnumerable<TEntity>>
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
    }
}