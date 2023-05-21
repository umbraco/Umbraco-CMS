namespace Umbraco.Cms.Core.IO;

// shadow filesystems is definitively ... too convoluted
internal class ShadowFileSystems : ICompletable
{
    private readonly FileSystems _fileSystems;
    private bool _completed;

    // invoked by the filesystems when shadowing
    public ShadowFileSystems(FileSystems fileSystems, string id)
    {
        _fileSystems = fileSystems;
        Id = id;

        _fileSystems.BeginShadow(id);
    }

    // for tests
    public string Id { get; }

    // invoked by the scope when exiting, if completed
    public void Complete() => _completed = true;

    // invoked by the scope when exiting
    public void Dispose() => _fileSystems.EndShadow(Id, _completed);
}
