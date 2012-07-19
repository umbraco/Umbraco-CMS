using System;
using System.Collections;
using System.Collections.Generic;

namespace umbraco.presentation.LiveEditing.Updates
{
    /// <summary>
    /// Default implementation of <see cref="IUpdateList"/>.
    /// </summary>
    [Serializable]
    public class DefaultUpdateList : IUpdateList
    {
        /// <summary>The internal list of updates.</summary>
        private List<IUpdate> m_Updates = new List<IUpdate>();

        /// <summary>
        /// Occurs when an update is added to the list.
        /// </summary>
        [field: NonSerialized]
        public event EventHandler<UpdateAddedEventArgs> UpdateAdded;

        #region IUpdateList Members

        /// <summary>
        /// Adds the specified item to the list.
        /// </summary>
        /// <param name="update">The item.</param>
        public void Add(IUpdate update)
        {
            m_Updates.Add(update);
            if (UpdateAdded != null)
                UpdateAdded(this, new UpdateAddedEventArgs(update));
        }

        /// <summary>
        /// Saves all updates.
        /// </summary>
        public void SaveAll()
        {
            foreach(IUpdate update in this)
                update.Save();
        }

        /// <summary>
        /// Publishes all updates.
        /// </summary>
        public void PublishAll()
        {
            foreach (IUpdate update in this)
                update.Publish();
        }

        /// <summary>
        /// Gets all updates of the specified type.
        /// </summary>
        /// <typeparam name="T">Type of updates to find.</typeparam>
        /// <returns>All matching updates.</returns>
        public IList<T> GetAll<T>() where T : IUpdate
        {
            return GetAll<T>(t => true);
        }

        /// <summary>
        /// Gets all updates satisfying the matcher.
        /// </summary>
        /// <typeparam name="T">Type of updates to find.</typeparam>
        /// <param name="matcher">The matcher.</param>
        /// <returns>All matching updates.</returns>
        public IList<T> GetAll<T>(Predicate<T> matcher) where T : IUpdate
        {
            List<T> matchingUpdates = new List<T>();
            foreach(IUpdate update in this)
            {
                if(update is T && matcher((T)update))
                {
                    matchingUpdates.Add((T)update);
                }
            }
            return matchingUpdates;
        }

        /// <summary>
        /// Gets the latest update satisfying the matcher.
        /// </summary>
        /// <typeparam name="T">Type of the update.</typeparam>
        /// <param name="matcher">The matcher.</param>
        /// <returns>
        /// The latest matching update if found, otherwise <c>null</c>.
        /// </returns>
        public T GetLatest<T>(Predicate<T> matcher) where T : IUpdate
        {
            for (int i = m_Updates.Count - 1; i >= 0; i--)
            {
                IUpdate update = m_Updates[i];
                if (update is T && matcher((T)update))
                    return (T)update;
            }
            return default(T);
        }

        #endregion

        #region IEnumerable<FieldUpdate> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<IUpdate> GetEnumerator()
        {
            return m_Updates.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)m_Updates).GetEnumerator();
        }

        #endregion

    }
}
