namespace Umbraco.Core.Events
{
	public class MoveEventArgs<TEntity> : CancellableObjectEventArgs<TEntity>
	{
		public MoveEventArgs(TEntity entity, bool canCancel, int parentId) : base(entity, canCancel)
		{
			ParentId = parentId;
		}

		public MoveEventArgs(TEntity entity, int parentId) : base(entity)
		{
			ParentId = parentId;
		}

		/// <summary>
		/// Gets or Sets the Id of the objects new parent.
		/// </summary>
		public int ParentId { get; private set; }
	}
}