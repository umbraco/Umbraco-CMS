using System;
using Umbraco.Core.Cache;
using Umbraco.Core.Events;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Scoping
{
    /// <summary>
    /// Implements <see cref="IScope"/> when there is no scope.
    /// </summary>
    internal class NoScope : IScope
    {
        private readonly ScopeProvider _scopeProvider;
        private bool _disposed;

        private UmbracoDatabase _database;
        private IEventDispatcher _eventDispatcher;
        private EventMessages _messages;

        public NoScope(ScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
#if DEBUG_SCOPES
            _scopeProvider.Register(this);
#endif
        }

#if DEBUG_SCOPES
        private readonly Guid _instanceId = Guid.NewGuid();
        public Guid InstanceId { get { return _instanceId; } }
#endif

        /// <inheritdoc />
        public RepositoryCacheMode RepositoryCacheMode
        {
            get { return RepositoryCacheMode.Default; }
        }

        /// <inheritdoc />
        public IsolatedRuntimeCache IsolatedRuntimeCache { get { throw new NotImplementedException(); } }

        /// <inheritdoc />
        public UmbracoDatabase Database
        {
            get
            {
                EnsureNotDisposed();
                return _database ?? (_database = _scopeProvider.DatabaseFactory.CreateNewDatabase());
            }
        }

        public UmbracoDatabase DatabaseOrNull
        {
            get
            {
                EnsureNotDisposed();
                return _database;
            }
        }

        /// <inheritdoc />
        public EventMessages Messages
        {
            get
            {
                EnsureNotDisposed();
                // fixme - should be a 'no scope event messages' of some sort
                return _messages ?? (_messages = new EventMessages());
            }
        }

        public EventMessages MessagesOrNull
        {
            get
            {
                EnsureNotDisposed();
                return _messages;
            }
        }

        /// <inheritdoc />
        public IEventDispatcher Events
        {
            get
            {
                EnsureNotDisposed();
                return _eventDispatcher ?? (_eventDispatcher = new PassThroughEventDispatcher());
            }
        }

        /// <inheritdoc />
        public void Complete()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public T Enlist<T>(string key, Func<T> creator)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public T Enlist<T>(string key, Func<T> creator, ActionTime actionTimes, Action<ActionTime, bool, T> action)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Enlist(string key, ActionTime actionTimes, Action<ActionTime, bool> action)
        {
            throw new NotImplementedException();
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("this");
        }

        public void Dispose()
        {
            EnsureNotDisposed();

            if (this != _scopeProvider.AmbientScope)
                throw new InvalidOperationException("Not the ambient scope.");

#if DEBUG_SCOPES
            _scopeProvider.Disposed(this);
#endif

            if (_database != null)
                _database.Dispose();

            _scopeProvider.AmbientScope = null;

            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
