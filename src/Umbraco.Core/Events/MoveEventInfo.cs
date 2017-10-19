using System;
using System.Collections.Generic;

namespace Umbraco.Core.Events
{
    public class MoveEventInfo<TEntity> : IEquatable<MoveEventInfo<TEntity>>
    {
        public MoveEventInfo(TEntity entity, string originalPath, int newParentId)
        {
            Entity = entity;
            OriginalPath = originalPath;
            NewParentId = newParentId;
        }

        public TEntity Entity { get; set; }
        public string OriginalPath { get; set; }
        public int NewParentId { get; set; }

        public bool Equals(MoveEventInfo<TEntity> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return EqualityComparer<TEntity>.Default.Equals(Entity, other.Entity) && NewParentId == other.NewParentId && string.Equals(OriginalPath, other.OriginalPath);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MoveEventInfo<TEntity>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = EqualityComparer<TEntity>.Default.GetHashCode(Entity);
                hashCode = (hashCode * 397) ^ NewParentId;
                hashCode = (hashCode * 397) ^ OriginalPath.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(MoveEventInfo<TEntity> left, MoveEventInfo<TEntity> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MoveEventInfo<TEntity> left, MoveEventInfo<TEntity> right)
        {
            return !Equals(left, right);
        }
    }
}