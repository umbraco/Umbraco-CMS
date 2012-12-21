namespace Umbraco.Core.Events
{
	public class NewEventArgs<TEntity> : CancellableObjectEventArgs<TEntity>
	{
		public NewEventArgs(TEntity eventObject, bool canCancel, string @alias, int parentId) : base(eventObject, canCancel)
		{
			Alias = alias;
			ParentId = parentId;
		}

		public NewEventArgs(TEntity eventObject, string @alias, int parentId) : base(eventObject)
		{
			Alias = alias;
			ParentId = parentId;
		}

		/// <summary>
		/// The entity being created
		/// </summary>
		public TEntity Entity
		{
			get { return EventObject; }
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