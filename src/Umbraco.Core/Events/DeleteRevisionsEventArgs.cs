using System;

namespace Umbraco.Core.Events
{
	public class DeleteRevisionsEventArgs : DeleteEventArgs
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
	}
}