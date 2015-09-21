using System.Collections.Generic;

namespace Umbraco.Core.Events
{
	public class DeleteEventArgs<TEntity> : CancellableObjectEventArgs<IEnumerable<TEntity>>
	{
	    /// <summary>
	    /// Constructor accepting multiple entities that are used in the delete operation
	    /// </summary>
	    /// <param name="eventObject"></param>
	    /// <param name="canCancel"></param>
	    /// <param name="eventMessages"></param>
	    public DeleteEventArgs(IEnumerable<TEntity> eventObject, bool canCancel, EventMessages eventMessages) : base(eventObject, canCancel, eventMessages)
        {
            MediaFilesToDelete = new List<string>();
        }

	    /// <summary>
	    /// Constructor accepting multiple entities that are used in the delete operation
	    /// </summary>
	    /// <param name="eventObject"></param>
	    /// <param name="eventMessages"></param>
	    public DeleteEventArgs(IEnumerable<TEntity> eventObject, EventMessages eventMessages) : base(eventObject, eventMessages)
        {
            MediaFilesToDelete = new List<string>();
        }

	    /// <summary>
	    /// Constructor accepting a single entity instance
	    /// </summary>
	    /// <param name="eventObject"></param>
	    /// <param name="eventMessages"></param>
	    public DeleteEventArgs(TEntity eventObject, EventMessages eventMessages)
            : base(new List<TEntity> { eventObject }, eventMessages)
        {
            MediaFilesToDelete = new List<string>();
        }

	    /// <summary>
	    /// Constructor accepting a single entity instance
	    /// </summary>
	    /// <param name="eventObject"></param>
	    /// <param name="canCancel"></param>
	    /// <param name="eventMessages"></param>
	    public DeleteEventArgs(TEntity eventObject, bool canCancel, EventMessages eventMessages)
            : base(new List<TEntity> { eventObject }, canCancel, eventMessages)
        {
            MediaFilesToDelete = new List<string>();
        }

        /// <summary>
        /// Constructor accepting multiple entities that are used in the delete operation
        /// </summary>
        /// <param name="eventObject"></param>
        /// <param name="canCancel"></param>
        public DeleteEventArgs(IEnumerable<TEntity> eventObject, bool canCancel) : base(eventObject, canCancel)
		{
            MediaFilesToDelete = new List<string>();
		}

		/// <summary>
		/// Constructor accepting multiple entities that are used in the delete operation
		/// </summary>
		/// <param name="eventObject"></param>
		public DeleteEventArgs(IEnumerable<TEntity> eventObject) : base(eventObject)
		{
            MediaFilesToDelete = new List<string>();
		}

		/// <summary>
		/// Constructor accepting a single entity instance
		/// </summary>
		/// <param name="eventObject"></param>
		public DeleteEventArgs(TEntity eventObject)
			: base(new List<TEntity> { eventObject })
		{
            MediaFilesToDelete = new List<string>();
		}

		/// <summary>
		/// Constructor accepting a single entity instance
		/// </summary>
		/// <param name="eventObject"></param>
		/// <param name="canCancel"></param>
		public DeleteEventArgs(TEntity eventObject, bool canCancel)
			: base(new List<TEntity> { eventObject }, canCancel)
		{
            MediaFilesToDelete = new List<string>();
		}

		/// <summary>
		/// Returns all entities that were deleted during the operation
		/// </summary>
		public IEnumerable<TEntity> DeletedEntities
		{
			get { return EventObject; }
		}

        /// <summary>
        /// A list of media files that can be added to during a deleted operation for which Umbraco will ensure are removed
        /// </summary>
        public List<string> MediaFilesToDelete { get; private set; } 
	}

	public class DeleteEventArgs : CancellableEventArgs
	{
        public DeleteEventArgs(int id, bool canCancel, EventMessages eventMessages)
            : base(canCancel, eventMessages)
        {
            Id = id;
        }

        public DeleteEventArgs(int id, bool canCancel)
			: base(canCancel)
		{
			Id = id;
		}

		public DeleteEventArgs(int id)
		{
			Id = id;
		}

		/// <summary>
		/// Gets the Id of the object being deleted.
		/// </summary>
		public int Id { get; private set; }
	}
}