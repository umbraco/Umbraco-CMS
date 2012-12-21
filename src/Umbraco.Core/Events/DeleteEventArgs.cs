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
		public DeleteEventArgs(IEnumerable<TEntity> eventObject, bool canCancel) : base(eventObject, canCancel)
		{
		}

		/// <summary>
		/// Constructor accepting multiple entities that are used in the delete operation
		/// </summary>
		/// <param name="eventObject"></param>
		public DeleteEventArgs(IEnumerable<TEntity> eventObject) : base(eventObject)
		{
		}

		/// <summary>
		/// Constructor accepting a single entity instance
		/// </summary>
		/// <param name="eventObject"></param>
		public DeleteEventArgs(TEntity eventObject)
			: base(new List<TEntity> { eventObject })
		{
		}

		/// <summary>
		/// Constructor accepting a single entity instance
		/// </summary>
		/// <param name="eventObject"></param>
		/// <param name="canCancel"></param>
		public DeleteEventArgs(TEntity eventObject, bool canCancel)
			: base(new List<TEntity> { eventObject }, canCancel)
		{
		}

		/// <summary>
		/// Returns all entities that were deleted during the operation
		/// </summary>
		public IEnumerable<TEntity> DeletedEntities
		{
			get { return EventObject; }
		} 
	}

	public class DeleteEventArgs : CancellableEventArgs
	{
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