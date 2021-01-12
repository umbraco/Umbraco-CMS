using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Umbraco.Core.Collections
{
    /// <summary>
    /// Allows clearing all event handlers
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class EventClearingObservableCollection<TValue> : ObservableCollection<TValue>
    {
        public EventClearingObservableCollection()
        {
        }

        public EventClearingObservableCollection(List<TValue> list) : base(list)
        {
        }

        public EventClearingObservableCollection(IEnumerable<TValue> collection) : base(collection)
        {
        }

        // need to override to clear
        public override event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Clears all event handlers for the <see cref="CollectionChanged"/> event
        /// </summary>
        public void ClearCollectionChangedEvents() => CollectionChanged = null;
    }
}
