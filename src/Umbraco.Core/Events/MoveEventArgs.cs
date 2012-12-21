namespace Umbraco.Core.Events
{
	public class MoveEventArgs<TEntity> : CancellableObjectEventArgs<TEntity>
	{
		public MoveEventArgs(TEntity eventObject, bool canCancel, int parentId) : base(eventObject, canCancel)
		{
			ParentId = parentId;
		}

		public MoveEventArgs(TEntity eventObject, int parentId) : base(eventObject)
		{
			ParentId = parentId;
		}

		/// <summary>
		/// The entity being moved
		/// </summary>
		public TEntity Entity
		{
			get { return EventObject; }
		}

		/// <summary>
		/// Gets or Sets the Id of the objects new parent.
		/// </summary>
		public int ParentId { get; private set; }
	}
}