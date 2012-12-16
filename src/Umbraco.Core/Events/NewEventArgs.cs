namespace Umbraco.Core.Events
{
	public class NewEventArgs<TEntity> : CancellableObjectEventArgs<TEntity>
	{
		public NewEventArgs(TEntity entity, bool canCancel, string @alias, int parentId) : base(entity, canCancel)
		{
			Alias = alias;
			ParentId = parentId;
		}

		public NewEventArgs(TEntity entity, string @alias, int parentId) : base(entity)
		{
			Alias = alias;
			ParentId = parentId;
		}

		/// <summary>
		/// Gets or Sets the Alias.
		/// </summary>
		public string Alias { get; private set; }

		/// <summary>
		/// Gets or Sets the Id of the parent.
		/// </summary>
		public int ParentId { get; private set; }
	}
}