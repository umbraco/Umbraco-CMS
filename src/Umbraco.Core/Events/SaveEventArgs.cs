using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Events
{
	public class SaveEventArgs<TEntity> : CancellableObjectEventArgs<TEntity>
	{
		public SaveEventArgs(TEntity entity, bool canCancel) : base(entity, canCancel)
		{
		}

		public SaveEventArgs(TEntity entity) : base(entity)
		{
		}
	}
}