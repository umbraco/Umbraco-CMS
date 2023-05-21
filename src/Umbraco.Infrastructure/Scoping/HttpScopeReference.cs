// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Core.Scoping;

/// <summary>
///     Disposed at the end of the request to cleanup any orphaned Scopes.
/// </summary>
/// <remarks>Registered as Scoped in DI (per request)</remarks>
internal class HttpScopeReference : IHttpScopeReference
{
    private readonly ScopeProvider _scopeProvider;
    private bool _disposedValue;
    private bool _registered;

    public HttpScopeReference(ScopeProvider scopeProvider) => _scopeProvider = scopeProvider;

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
                    Scope? scope;
                    while ((scope = _scopeProvider.AmbientScope) != null)
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
