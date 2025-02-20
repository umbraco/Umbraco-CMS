namespace Umbraco.Cms.Core.Services.Changes;

public class TreeChange<TItem>
{
    public TreeChange(TItem changedItem, TreeChangeTypes changeTypes)
    {
        Item = changedItem;
        ChangeTypes = changeTypes;
    }

    public TreeChange(TItem changedItem, TreeChangeTypes changeTypes, IEnumerable<string>? publishedCultures, IEnumerable<string>? unpublishedCultures)
    {
        Item = changedItem;
        ChangeTypes = changeTypes;
        PublishedCultures = publishedCultures;
        UnpublishedCultures = unpublishedCultures;
    }

    public TItem Item { get; }

    public TreeChangeTypes ChangeTypes { get; }

    public IEnumerable<string>? PublishedCultures { get; }

    public IEnumerable<string>? UnpublishedCultures { get; }

    public EventArgs ToEventArgs() => new EventArgs(this);

    public class EventArgs : System.EventArgs
    {
        public EventArgs(IEnumerable<TreeChange<TItem>> changes) => Changes = changes.ToArray();

        public EventArgs(TreeChange<TItem> change)
            : this(new[] { change })
        {
        }

        public IEnumerable<TreeChange<TItem>> Changes { get; }
    }
}
