using System;

namespace Umbraco.Core.Events
{
    public class DeleteRevisionsEventArgs : DeleteEventArgs, IEquatable<DeleteRevisionsEventArgs>
    {
        public DeleteRevisionsEventArgs(int id, bool canCancel, int specificVersion = default, bool deletePriorVersions = false, DateTime dateToRetain = default)
            : base(id, canCancel)
        {
            DeletePriorVersions = deletePriorVersions;
            SpecificVersion = specificVersion;
            DateToRetain = dateToRetain;
        }

        public DeleteRevisionsEventArgs(int id, int specificVersion = default, bool deletePriorVersions = false, DateTime dateToRetain = default)
            : base(id)
        {
            DeletePriorVersions = deletePriorVersions;
            SpecificVersion = specificVersion;
            DateToRetain = dateToRetain;
        }

        public bool DeletePriorVersions { get; }
        public int SpecificVersion { get; }
        public DateTime DateToRetain { get; }

        /// <summary>
        /// Returns true if we are deleting a specific revision
        /// </summary>
        public bool IsDeletingSpecificRevision => SpecificVersion != default;

        public bool Equals(DeleteRevisionsEventArgs other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && DateToRetain.Equals(other.DateToRetain) && DeletePriorVersions == other.DeletePriorVersions && SpecificVersion.Equals(other.SpecificVersion);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((DeleteRevisionsEventArgs) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ DateToRetain.GetHashCode();
                hashCode = (hashCode * 397) ^ DeletePriorVersions.GetHashCode();
                hashCode = (hashCode * 397) ^ SpecificVersion.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(DeleteRevisionsEventArgs left, DeleteRevisionsEventArgs right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DeleteRevisionsEventArgs left, DeleteRevisionsEventArgs right)
        {
            return !Equals(left, right);
        }
    }
}
