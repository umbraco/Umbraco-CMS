namespace Umbraco.Core.Events
{
	public class DeleteEventArgs<TEntity> : CancellableObjectEventArgs<TEntity>
	{
		public DeleteEventArgs(TEntity entity, bool canCancel) : base(entity, canCancel)
		{
		}

		public DeleteEventArgs(TEntity entity) : base(entity)
		{
		}
	}

	public class DeleteEventArgs : CancellableEventArgs
	{
		public DeleteEventArgs(int id, bool canCancel)
			: base(canCancel)
		{
			Id = id;
		}

		public DeleteEventArgs(int id)
		{
			Id = id;
		}

		/// <summary>
		/// Gets the Id of the object being deleted.
		/// </summary>
		public int Id { get; private set; }
	}
}