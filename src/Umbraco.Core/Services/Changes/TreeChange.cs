namespace Umbraco.Cms.Core.Services.Changes;

/// <summary>
///     Represents a change to a tree item.
/// </summary>
/// <typeparam name="TItem">The type of the item that changed.</typeparam>
public class TreeChange<TItem>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TreeChange{TItem}"/> class.
    /// </summary>
    /// <param name="changedItem">The item that changed.</param>
    /// <param name="changeTypes">The types of changes that occurred.</param>
    public TreeChange(TItem changedItem, TreeChangeTypes changeTypes)
    {
        Item = changedItem;
        ChangeTypes = changeTypes;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="TreeChange{TItem}"/> class with culture information.
    /// </summary>
    /// <param name="changedItem">The item that changed.</param>
    /// <param name="changeTypes">The types of changes that occurred.</param>
    /// <param name="publishedCultures">The cultures that were published.</param>
    /// <param name="unpublishedCultures">The cultures that were unpublished.</param>
    public TreeChange(TItem changedItem, TreeChangeTypes changeTypes, IEnumerable<string>? publishedCultures, IEnumerable<string>? unpublishedCultures)
    {
        Item = changedItem;
        ChangeTypes = changeTypes;
        PublishedCultures = publishedCultures;
        UnpublishedCultures = unpublishedCultures;
    }

    /// <summary>
    ///     Gets the item that changed.
    /// </summary>
    public TItem Item { get; }

    /// <summary>
    ///     Gets the types of changes that occurred.
    /// </summary>
    public TreeChangeTypes ChangeTypes { get; }

    /// <summary>
    ///     Gets the cultures that were published, if any.
    /// </summary>
    public IEnumerable<string>? PublishedCultures { get; }

    /// <summary>
    ///     Gets the cultures that were unpublished, if any.
    /// </summary>
    public IEnumerable<string>? UnpublishedCultures { get; }

    /// <summary>
    ///     Converts this change to event arguments.
    /// </summary>
    /// <returns>Event arguments containing this change.</returns>
    public EventArgs ToEventArgs() => new EventArgs(this);

    /// <summary>
    ///     Event arguments for tree changes.
    /// </summary>
    public class EventArgs : System.EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EventArgs"/> class with multiple changes.
        /// </summary>
        /// <param name="changes">The changes that occurred.</param>
        public EventArgs(IEnumerable<TreeChange<TItem>> changes) => Changes = changes.ToArray();

        /// <summary>
        ///     Initializes a new instance of the <see cref="EventArgs"/> class with a single change.
        /// </summary>
        /// <param name="change">The change that occurred.</param>
        public EventArgs(TreeChange<TItem> change)
            : this(new[] { change })
        {
        }

        /// <summary>
        ///     Gets the changes that occurred.
        /// </summary>
        public IEnumerable<TreeChange<TItem>> Changes { get; }
    }
}
