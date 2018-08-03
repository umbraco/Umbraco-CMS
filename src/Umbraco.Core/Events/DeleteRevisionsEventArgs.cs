using System;

namespace Umbraco.Core.Events
{
	public class DeleteRevisionsEventArgs : DeleteEventArgs, IEquatable<DeleteRevisionsEventArgs>
	{
		public DeleteRevisionsEventArgs(int id, bool canCancel, Guid specificVersion = default(Guid), bool deletePriorVersions = false, DateTime dateToRetain = default(DateTime))
			: base(id, canCancel)
		{
			DeletePriorVersions = deletePriorVersions;
			SpecificVersion = specificVersion;
			DateToRetain = dateToRetain;
		}

		public DeleteRevisionsEventArgs(int id, Guid specificVersion = default(Guid), bool deletePriorVersions = false, DateTime dateToRetain = default(DateTime))
			: base(id)
		{
			DeletePriorVersions = deletePriorVersions;
			SpecificVersion = specificVersion;
			DateToRetain = dateToRetain;			
		}

		public bool DeletePriorVersions { get; private set; }
		public Guid SpecificVersion { get; private set; }
		public DateTime DateToRetain { get; private set; }
		
		/// <summary>
		/// Returns true if we are deleting a specific revision
		/// </summary>
		public bool IsDeletingSpecificRevision
		{
			get { return SpecificVersion != default(Guid); }
		}

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
	        if (obj.GetType() != this.GetType()) return false;
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