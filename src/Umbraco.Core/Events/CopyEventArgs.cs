namespace Umbraco.Core.Events
{
	public class CopyEventArgs<TEntity> : CancellableObjectEventArgs<TEntity>
	{
		public CopyEventArgs(TEntity original, TEntity copy, bool canCancel, int parentId) : base(original, canCancel)
		{
			Copy = copy;
			ParentId = parentId;
		}

		public CopyEventArgs(TEntity entity, TEntity copy, int parentId) : base(entity)
		{
			Copy = copy;
			ParentId = parentId;
		}

		/// <summary>
		/// The copied entity
		/// </summary>
		public TEntity Copy { get; set; }

		/// <summary>
		/// Gets or Sets the Id of the objects new parent.
		/// </summary>
		public int ParentId { get; private set; }
	}
}