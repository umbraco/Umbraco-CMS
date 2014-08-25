namespace Umbraco.Core.Events
{
    public class CopyEventArgs<TEntity> : CancellableObjectEventArgs<TEntity>
    {
        public CopyEventArgs(TEntity original, TEntity copy, bool canCancel, int parentId)
            : base(original, canCancel)
        {
            Copy = copy;
            ParentId = parentId;
        }

        public CopyEventArgs(TEntity eventObject, TEntity copy, int parentId)
            : base(eventObject)
        {
            Copy = copy;
            ParentId = parentId;
        }

        public CopyEventArgs(TEntity eventObject, TEntity copy, bool canCancel, int parentId, bool relateToOriginal)
            : base(eventObject, canCancel)
        {
            Copy = copy;
            ParentId = parentId;
            RelateToOriginal = relateToOriginal;
        }

        /// <summary>
        /// The copied entity
        /// </summary>
        public TEntity Copy { get; set; }

        /// <summary>
        /// The original entity
        /// </summary>
        public TEntity Original
        {
            get { return EventObject; }
        }

        /// <summary>
        /// Gets or Sets the Id of the objects new parent.
        /// </summary>
        public int ParentId { get; private set; }

        public bool RelateToOriginal { get; set; }
    }
}