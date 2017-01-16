using System;
using System.Collections.Generic;
using Umbraco.Core.Events;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Scoping
{
    internal class NoScope : IScope
    {
        private readonly ScopeProvider _scopeProvider;
        private bool _disposed;

        private UmbracoDatabase _database;
        private IList<EventMessage> _messages;

        public NoScope(ScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        //public bool HasDatabase { get { return _database != null; } }

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

        //public bool HasMessages { get { return _messages != null; } }

        public IList<EventMessage> Messages
        {
            get
            {
                EnsureNotDisposed();
                return _messages ?? (_messages = new List<EventMessage>());
            }
        }

        public IList<EventMessage> MessagesOrNull
        {
            get
            {
                EnsureNotDisposed();
                return _messages;
            }
        }

        public void Complete()
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
            _scopeProvider.Disposing(this);
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
