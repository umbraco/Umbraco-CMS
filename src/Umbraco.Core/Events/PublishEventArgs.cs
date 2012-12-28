using System.Collections.Generic;

namespace Umbraco.Core.Events
{
    public class PublishEventArgs<TEntity> : CancellableObjectEventArgs<IEnumerable<TEntity>>
    {
        /// <summary>
		/// Constructor accepting multiple entities that are used in the publish operation
		/// </summary>
		/// <param name="eventObject"></param>
		/// <param name="canCancel"></param>
		public PublishEventArgs(IEnumerable<TEntity> eventObject, bool canCancel)
			: base(eventObject, canCancel)
		{
		}

		/// <summary>
		/// Constructor accepting multiple entities that are used in the publish operation
		/// </summary>
		/// <param name="eventObject"></param>
		public PublishEventArgs(IEnumerable<TEntity> eventObject)
			: base(eventObject)
		{
		}

		/// <summary>
		/// Constructor accepting a single entity instance
		/// </summary>
		/// <param name="eventObject"></param>
		public PublishEventArgs(TEntity eventObject)
			: base(new List<TEntity> { eventObject })
		{
		}

		/// <summary>
		/// Constructor accepting a single entity instance
		/// </summary>
		/// <param name="eventObject"></param>
		/// <param name="canCancel"></param>
        public PublishEventArgs(TEntity eventObject, bool canCancel)
			: base(new List<TEntity> { eventObject }, canCancel)
		{
		}

		/// <summary>
		/// Returns all entities that were published during the operation
		/// </summary>
		public IEnumerable<TEntity> PublishedEntities
		{
			get { return EventObject; }
		}
    }
}