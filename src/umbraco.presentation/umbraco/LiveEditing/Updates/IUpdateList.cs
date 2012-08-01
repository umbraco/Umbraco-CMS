using System;
using System.Collections.Generic;

namespace umbraco.presentation.LiveEditing.Updates
{
    /// <summary>
    /// Interface for a list of Live Editing updates.
    /// </summary>
    public interface IUpdateList : IEnumerable<IUpdate>
    {
        /// <summary>Occurs when an update is added to the list.</summary>
        event EventHandler<UpdateAddedEventArgs> UpdateAdded;

        /// <summary>
        /// Adds the specified item to the list.
        /// </summary>
        /// <param name="update">The item.</param>
        void Add(IUpdate update);

        /// <summary>
        /// Saves all updates.
        /// </summary>
        void SaveAll();

        /// <summary>
        /// Publishes all updates.
        /// </summary>
        void PublishAll();

        /// <summary>
        /// Gets all updates of the specified type.
        /// </summary>
        /// <typeparam name="T">Type of updates to find.</typeparam>
        /// <returns>All matching updates.</returns>
        IList<T> GetAll<T>() where T : IUpdate;

        /// <summary>
        /// Gets all updates of the specified type satisfying the matcher.
        /// </summary>
        /// <typeparam name="T">Type of updates to find.</typeparam>
        /// <param name="matcher">The matcher.</param>
        /// <returns>All matching updates.</returns>
        IList<T> GetAll<T>(Predicate<T> matcher) where T : IUpdate;

        /// <summary>
        /// Gets the latest update of the specified type satisfying the matcher.
        /// </summary>
        /// <typeparam name="T">Type of the update.</typeparam>
        /// <param name="matcher">The matcher.</param>
        /// <returns>The latest matching update if found, otherwise <c>null</c>.</returns>
        T GetLatest<T>(Predicate<T> matcher) where T : IUpdate;
    }

    /// <summary>
    /// Event arguments for the <see cref="UpdateAdded"/> event.
    /// </summary>
    public class UpdateAddedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the added update.
        /// </summary>
        /// <value>The update.</value>
        public IUpdate Update { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateAddedEventArgs"/> class.
        /// </summary>
        /// <param name="update">The update.</param>
        public UpdateAddedEventArgs(IUpdate update)
        {
            Update = update;
        }
    }
}
