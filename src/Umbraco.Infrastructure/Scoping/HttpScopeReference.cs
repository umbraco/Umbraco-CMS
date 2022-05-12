// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Scoping
{

    /// <summary>
    /// Disposed at the end of the request to cleanup any orphaned Scopes.
    /// </summary>
    /// <remarks>Registered as Scoped in DI (per request)</remarks>
    internal class HttpScopeReference : IHttpScopeReference
    {
        private readonly Infrastructure.Scoping.ScopeProvider _scopeProvider;
        private bool _disposedValue;
        private bool _registered = false;

        public HttpScopeReference(Infrastructure.Scoping.ScopeProvider scopeProvider) => _scopeProvider = scopeProvider;

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
                        Infrastructure.Scoping.Scope? scope;
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

        public void Dispose() =>
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);

        public void Register() => _registered = true;
    }
}
