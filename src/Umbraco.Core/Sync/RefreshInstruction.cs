using System;

namespace Umbraco.Core.Sync
{
    [Serializable]
    public class RefreshInstruction
    {
        public RefreshMethodType RefreshType { get; set; }
        public Guid RefresherId { get; set; }
        public Guid GuidId { get; set; }
        public int IntId { get; set; }
        public string JsonIds { get; set; }
        public string JsonPayload { get; set; }

        [Serializable]
        public enum RefreshMethodType
        {
            RefreshAll,
            RefreshByGuid,
            RefreshById,
            RefreshByIds,
            RefreshByJson,
            RemoveById
        }

        protected bool Equals(RefreshInstruction other)
        {
            return RefreshType == other.RefreshType && RefresherId.Equals(other.RefresherId) && GuidId.Equals(other.GuidId) && IntId == other.IntId && string.Equals(JsonIds, other.JsonIds) && string.Equals(JsonPayload, other.JsonPayload);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((RefreshInstruction) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (int) RefreshType;
                hashCode = (hashCode*397) ^ RefresherId.GetHashCode();
                hashCode = (hashCode*397) ^ GuidId.GetHashCode();
                hashCode = (hashCode*397) ^ IntId;
                hashCode = (hashCode*397) ^ (JsonIds != null ? JsonIds.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (JsonPayload != null ? JsonPayload.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(RefreshInstruction left, RefreshInstruction right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(RefreshInstruction left, RefreshInstruction right)
        {
            return !Equals(left, right);
        }
    }
}