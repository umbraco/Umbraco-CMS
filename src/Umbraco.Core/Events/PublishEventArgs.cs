﻿using System;
using System.Collections.Generic;

namespace Umbraco.Core.Events
{
    public class PublishEventArgs<TEntity> : CancellableEnumerableObjectEventArgs<TEntity>, IEquatable<PublishEventArgs<TEntity>>
    {
        /// <summary>
        /// Constructor accepting multiple entities that are used in the publish operation
        /// </summary>
        /// <param name="eventObject"></param>
        /// <param name="canCancel"></param>
        /// <param name="eventMessages"></param>
        public PublishEventArgs(IEnumerable<TEntity> eventObject, bool canCancel, EventMessages eventMessages)
            : base(eventObject, canCancel, eventMessages)
        {
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
        /// <param name="eventMessages"></param>
        public PublishEventArgs(TEntity eventObject, bool canCancel, EventMessages eventMessages)
            : base(new List<TEntity> { eventObject }, canCancel, eventMessages)
        {
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
        }

        /// <summary>
        /// Returns all entities that were published during the operation
        /// </summary>
        public IEnumerable<TEntity> PublishedEntities => EventObject;

        public bool Equals(PublishEventArgs<TEntity> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PublishEventArgs<TEntity>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397);
            }
        }

        public static bool operator ==(PublishEventArgs<TEntity> left, PublishEventArgs<TEntity> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PublishEventArgs<TEntity> left, PublishEventArgs<TEntity> right)
        {
            return !Equals(left, right);
        }
    }
}
