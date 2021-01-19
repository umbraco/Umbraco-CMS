﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Umbraco.Core.Collections
{
    /// <summary>
    /// Allows clearing all event handlers
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class EventClearingObservableCollection<TValue> : ObservableCollection<TValue>, INotifyCollectionChanged
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

        // need to explicitly implement with event accessor syntax in order to override in order to to clear
        // c# events are weird, they do not behave the same way as other c# things that are 'virtual',
        // a good article is here: https://medium.com/@unicorn_dev/virtual-events-in-c-something-went-wrong-c6f6f5fbe252
        // and https://stackoverflow.com/questions/2268065/c-sharp-language-design-explicit-interface-implementation-of-an-event
        private NotifyCollectionChangedEventHandler _changed;
        event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
        {
            add { _changed += value; }
            remove { _changed -= value; }
        }

        /// <summary>
        /// Clears all event handlers for the <see cref="CollectionChanged"/> event
        /// </summary>
        public void ClearCollectionChangedEvents() => _changed = null;
    }
}
