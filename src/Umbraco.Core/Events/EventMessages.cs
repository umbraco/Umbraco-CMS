namespace Umbraco.Cms.Core.Events;

/// <summary>
///     Event messages collection
/// </summary>
public sealed class EventMessages : DisposableObjectSlim
{
    private readonly List<EventMessage> _msgs = new();

    public int Count => _msgs.Count;

    public void Add(EventMessage msg) => _msgs.Add(msg);

    public IEnumerable<EventMessage> GetAll() => _msgs;

    protected override void DisposeResources() => _msgs.Clear();
}
