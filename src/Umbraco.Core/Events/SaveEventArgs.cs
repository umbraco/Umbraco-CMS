using System.Collections.Generic;

namespace Umbraco.Core.Events
{
	public class SaveEventArgs<TEntity> : CancellableObjectEventArgs<IEnumerable<TEntity>>
	{
		/// <summary>
		/// Constructor accepting multiple entities that are used in the saving operation
		/// </summary>
		/// <param name="eventObject"></param>
		/// <param name="canCancel"></param>
		public SaveEventArgs(IEnumerable<TEntity> eventObject, bool canCancel)
			: base(eventObject, canCancel)
		{
		}

		/// <summary>
		/// Constructor accepting multiple entities that are used in the saving operation
		/// </summary>
		/// <param name="eventObject"></param>
		public SaveEventArgs(IEnumerable<TEntity> eventObject)
			: base(eventObject)
		{
		}

		/// <summary>
		/// Constructor accepting a single entity instance
		/// </summary>
		/// <param name="eventObject"></param>
		public SaveEventArgs(TEntity eventObject)
			: base(new List<TEntity> { eventObject })
		{
		}

		/// <summary>
		/// Constructor accepting a single entity instance
		/// </summary>
		/// <param name="eventObject"></param>
		/// <param name="canCancel"></param>
		public SaveEventArgs(TEntity eventObject, bool canCancel)
			: base(new List<TEntity> { eventObject }, canCancel)
		{
		}

		/// <summary>
		/// Returns all entities that were saved during the operation
		/// </summary>
		public IEnumerable<TEntity> SavedEntities
		{
			get { return EventObject; }
		}
	}
}