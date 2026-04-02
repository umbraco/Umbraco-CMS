namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Event messages collection
/// </summary>
public sealed class EventMessages : DisposableObjectSlim
{
    private readonly List<EventMessage> _msgs = new();

    /// <summary>
    ///     Gets the number of messages in the collection.
    /// </summary>
    public int Count => _msgs.Count;

    /// <summary>
    ///     Adds a message to the collection.
    /// </summary>
    /// <param name="msg">The message to add.</param>
    public void Add(EventMessage msg) => _msgs.Add(msg);

    /// <summary>
    ///     Gets all messages in the collection.
    /// </summary>
    /// <returns>An enumerable of all event messages.</returns>
    public IEnumerable<EventMessage> GetAll() => _msgs;

    /// <inheritdoc />
    protected override void DisposeResources() => _msgs.Clear();
}
