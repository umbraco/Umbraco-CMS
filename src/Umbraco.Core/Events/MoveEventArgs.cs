using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Events
{
    public class MoveEventArgs<TEntity> : CancellableObjectEventArgs<TEntity>, IEquatable<MoveEventArgs<TEntity>>
    {
        private IEnumerable<MoveEventInfo<TEntity>> _moveInfoCollection;

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
        public IEnumerable<MoveEventInfo<TEntity>> MoveInfoCollection
        {
            get { return _moveInfoCollection; }
            set
            {
                var first = value.FirstOrDefault();
                if (first == null)
                {
                    throw new InvalidOperationException("MoveInfoCollection must have at least one item");
                }

                _moveInfoCollection = value;                

                //assign the legacy props                
                EventObject = first.Entity;
                ParentId = first.NewParentId;                
            }
        }

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

        public bool Equals(MoveEventArgs<TEntity> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && MoveInfoCollection.Equals(other.MoveInfoCollection);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MoveEventArgs<TEntity>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ MoveInfoCollection.GetHashCode();
            }
        }

        public static bool operator ==(MoveEventArgs<TEntity> left, MoveEventArgs<TEntity> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MoveEventArgs<TEntity> left, MoveEventArgs<TEntity> right)
        {
            return !Equals(left, right);
        }
    }
}