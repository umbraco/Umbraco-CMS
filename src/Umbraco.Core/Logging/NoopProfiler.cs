namespace Umbraco.Cms.Core.Logging;

public class NoopProfiler : IProfiler
{
    private readonly VoidDisposable _disposable = new();

    public IDisposable Step(string name) => _disposable;

    public void Start()
    {
    }

    public void Stop(bool discardResults = false)
    {
    }

    private class VoidDisposable : DisposableObjectSlim
    {
        protected override void DisposeResources()
        {
        }
    }
}
