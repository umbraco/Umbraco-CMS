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
        /// <param name="isAllPublished"></param>
        /// <param name="eventMessages"></param>
        public PublishEventArgs(IEnumerable<TEntity> eventObject, bool canCancel, bool isAllPublished, EventMessages eventMessages)
            : base(eventObject, canCancel, eventMessages)
        {
            IsAllRepublished = isAllPublished;
        }

        /// <summary>
        /// Constructor accepting multiple entities that are used in the publish operation
        /// </summary>
        /// <param name="eventObject"></param>
        /// <param name="eventMessages"></param>
        public PublishEventArgs(IEnumerable<TEntity> eventObject, EventMessages eventMessages)
            : base(eventObject, eventMessages)
        {
        }

        /// <summary>
        /// Constructor accepting a single entity instance
        /// </summary>
        /// <param name="eventObject"></param>
        /// <param name="eventMessages"></param>
        public PublishEventArgs(TEntity eventObject, EventMessages eventMessages)
            : base(new List<TEntity> { eventObject }, eventMessages)
        {
        }

        /// <summary>
        /// Constructor accepting a single entity instance
        /// </summary>
        /// <param name="eventObject"></param>
        /// <param name="canCancel"></param>
        /// <param name="isAllPublished"></param>
        /// <param name="eventMessages"></param>
        public PublishEventArgs(TEntity eventObject, bool canCancel, bool isAllPublished, EventMessages eventMessages)
            : base(new List<TEntity> { eventObject }, canCancel, eventMessages)
        {
            IsAllRepublished = isAllPublished;
        }

        /// <summary>
        /// Constructor accepting multiple entities that are used in the publish operation
        /// </summary>
        /// <param name="eventObject"></param>
        /// <param name="canCancel"></param>
        /// <param name="isAllPublished"></param>
        public PublishEventArgs(IEnumerable<TEntity> eventObject, bool canCancel, bool isAllPublished)
			: base(eventObject, canCancel)
		{
            IsAllRepublished = isAllPublished;
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
        /// <param name="isAllPublished"></param>
        public PublishEventArgs(TEntity eventObject, bool canCancel, bool isAllPublished)
			: base(new List<TEntity> { eventObject }, canCancel)
		{
            IsAllRepublished = isAllPublished;
		}

		/// <summary>
		/// Returns all entities that were published during the operation
		/// </summary>
		public IEnumerable<TEntity> PublishedEntities
		{
			get { return EventObject; }
		}

        public bool IsAllRepublished { get; private set; }
    }
}