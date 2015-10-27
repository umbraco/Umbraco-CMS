using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Events
{
    public class MoveEventArgs<TEntity> : CancellableObjectEventArgs<TEntity>
    {
        /// <summary>
        /// Constructor accepting a collection of MoveEventInfo objects
        /// </summary>
        /// <param name="canCancel"></param>
        /// <param name="eventMessages"></param>
        /// <param name="moveInfo">
        /// A colleciton of MoveEventInfo objects that exposes all entities that have been moved during a single move operation
        /// </param>
        public MoveEventArgs(bool canCancel, EventMessages eventMessages, params MoveEventInfo<TEntity>[] moveInfo)
            : base(default(TEntity), canCancel, eventMessages)
        {
            if (moveInfo.FirstOrDefault() == null)
            {
                throw new ArgumentException("moveInfo argument must contain at least one item");
            }

            MoveInfoCollection = moveInfo;
            //assign the legacy props
            EventObject = moveInfo.First().Entity;
            ParentId = moveInfo.First().NewParentId;
        }

        /// <summary>
        /// Constructor accepting a collection of MoveEventInfo objects
        /// </summary>
        /// <param name="eventMessages"></param>
        /// <param name="moveInfo">
        /// A colleciton of MoveEventInfo objects that exposes all entities that have been moved during a single move operation
        /// </param>
        public MoveEventArgs(EventMessages eventMessages, params MoveEventInfo<TEntity>[] moveInfo)
            : base(default(TEntity), eventMessages)
        {
            if (moveInfo.FirstOrDefault() == null)
            {
                throw new ArgumentException("moveInfo argument must contain at least one item");
            }

            MoveInfoCollection = moveInfo;
            //assign the legacy props
            EventObject = moveInfo.First().Entity;
            ParentId = moveInfo.First().NewParentId;
        }

        /// <summary>
        /// Constructor accepting a collection of MoveEventInfo objects
        /// </summary>
        /// <param name="canCancel"></param>
        /// <param name="moveInfo">
        /// A colleciton of MoveEventInfo objects that exposes all entities that have been moved during a single move operation
        /// </param>
        public MoveEventArgs(bool canCancel, params MoveEventInfo<TEntity>[] moveInfo)
            : base(default(TEntity), canCancel)
        {
            if (moveInfo.FirstOrDefault() == null)
            {
                throw new ArgumentException("moveInfo argument must contain at least one item");
            }

            MoveInfoCollection = moveInfo;
            //assign the legacy props
            EventObject = moveInfo.First().Entity;
            ParentId = moveInfo.First().NewParentId;
        }

        /// <summary>
        /// Constructor accepting a collection of MoveEventInfo objects
        /// </summary>
        /// <param name="moveInfo">
        /// A colleciton of MoveEventInfo objects that exposes all entities that have been moved during a single move operation
        /// </param>
        public MoveEventArgs(params MoveEventInfo<TEntity>[] moveInfo)
            : base(default(TEntity))
        {
            if (moveInfo.FirstOrDefault() == null)
            {
                throw new ArgumentException("moveInfo argument must contain at least one item");
            }

            MoveInfoCollection = moveInfo;
            //assign the legacy props
            EventObject = moveInfo.First().Entity;
            ParentId = moveInfo.First().NewParentId;
        }

        [Obsolete("Use the overload that specifies the MoveEventInfo object")]
        public MoveEventArgs(TEntity eventObject, bool canCancel, int parentId)
            : base(eventObject, canCancel)
        {
            ParentId = parentId;
        }

        [Obsolete("Use the overload that specifies the MoveEventInfo object")]
        public MoveEventArgs(TEntity eventObject, int parentId)
            : base(eventObject)
        {
            ParentId = parentId;
        }

        /// <summary>
        /// Gets all MoveEventInfo objects used to create the object
        /// </summary>
        public IEnumerable<MoveEventInfo<TEntity>> MoveInfoCollection { get; private set; }

        /// <summary>
        /// The entity being moved
        /// </summary>
        [Obsolete("Retrieve the entity object from the MoveInfoCollection property instead")]
        public TEntity Entity
        {
            get { return EventObject; }
        }

        /// <summary>
        /// Gets the Id of the object's new parent
        /// </summary>
        [Obsolete("Retrieve the ParentId from the MoveInfoCollection property instead")]
        public int ParentId { get; private set; }
    }
}