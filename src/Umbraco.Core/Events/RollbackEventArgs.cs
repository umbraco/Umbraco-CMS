namespace Umbraco.Core.Events
{
	public class RollbackEventArgs<TEntity> : CancellableObjectEventArgs<TEntity>
	{
		public RollbackEventArgs(TEntity entity, bool canCancel) : base(entity, canCancel)
		{
		}

		public RollbackEventArgs(TEntity entity) : base(entity)
		{
		}
	}
}