using Umbraco.Cms.Persistence.EFCore.Scoping;

namespace Umbraco.Cms.Persistence.EFCore;

public class HttpEFCoreScopeReference : IHttpEFCoreScopeReference
{
    private readonly IEFCoreScopeAccessor _efCoreScopeAccessor;
    private bool _disposedValue;
    private bool _registered;

    public HttpEFCoreScopeReference(IEFCoreScopeAccessor efCoreScopeAccessor) => _efCoreScopeAccessor = efCoreScopeAccessor;

    public void Dispose() =>

        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(true);

    public void Register() => _registered = true;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                if (_registered)
                {
                    // dispose the entire chain (if any)
                    // reset (don't commit by default)
                    while (_efCoreScopeAccessor.AmbientScope is EfCoreScope scope)
                    {
                        scope.Reset();
                        scope.Dispose();
                    }
                }
            }

            _disposedValue = true;
        }
    }
}
