using System;
using System.Collections.Generic;
using Umbraco.Core.Events;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Scoping
{
    // note - scope is not thread-safe obviously

    internal class Scope : IScope
    {
        private readonly ScopeProvider _scopeProvider;
        private bool _disposed;
        private bool? _completed;

        private UmbracoDatabase _database;
        private IList<EventMessage> _messages;

        // initializes a new scope
        public Scope(ScopeProvider scopeProvider, bool detachable = false)
        {
            _scopeProvider = scopeProvider;
            Detachable = detachable;
        }

        // initializes a new scope in a nested scopes chain, with its parent
        public Scope(ScopeProvider scopeProvider, Scope parent)
            : this(scopeProvider)
        {
            ParentScope = parent;
        }

        // initializes a new scope, replacing a NoScope instance
        public Scope(ScopeProvider scopeProvider, NoScope noScope)
            : this(scopeProvider)
        {
            // steal everything from NoScope
            _database = noScope.DatabaseOrNull;
            _messages = noScope.MessagesOrNull;

            // make sure the NoScope can be replaced ie not in a transaction
            if (_database != null && _database.InTransaction)
                    throw new Exception("NoScope instance is not free.");
        }

        // a value indicating whether the scope is detachable
        // ie whether it was created by CreateDetachedScope
        public bool Detachable { get; private set; }

        // the parent scope (in a nested scopes chain)
        public Scope ParentScope { get; set; }

        // the original scope (when attaching a detachable scope)
        public Scope OrigScope { get; set; }

        //public bool HasDatabase
        //{
        //    get { return ParentScope == null ? _database != null : ParentScope.HasDatabase; }
        //}

        /// <inheritdoc />
        public UmbracoDatabase Database
        {
            get
            {
                EnsureNotDisposed();
                if (ParentScope != null) return ParentScope.Database;
                if (_database != null)
                {
                    if (_database.InTransaction == false) // stolen from noScope
                        _database.BeginTransaction(); // a scope implies a transaction, always
                    // fixme - what-if exception?
                    return _database;
                }
                var database = _scopeProvider.DatabaseFactory.CreateNewDatabase();
                database.BeginTransaction(); // a scope implies a transaction, always
                // fixme - should dispose db on exception?
                return _database = database;
            }
        }

        public UmbracoDatabase DatabaseOrNull
        {
            get
            {
                EnsureNotDisposed();
                return ParentScope == null ? _database : ParentScope.DatabaseOrNull;
            }
        }

        //public bool HasMessages
        //{
        //    get { return ParentScope == null ? _messages != null : ParentScope.HasMessages; }
        //}

        /// <inheritdoc />
        public IList<EventMessage> Messages
        {
            get
            {
                EnsureNotDisposed();
                if (ParentScope != null) return ParentScope.Messages;
                if (_messages == null)
                    _messages = new List<EventMessage>();
                return _messages;
            }
        }

        public IList<EventMessage> MessagesOrNull
        {
            get
            {
                EnsureNotDisposed();
                return ParentScope == null ? _messages : ParentScope.MessagesOrNull;
            }
        }

        /// <inheritdoc />
        public void Complete()
        {
            if (_completed.HasValue == false)
                _completed = true;
        }

        public void CompleteChild(bool? completed)
        {
            if (completed.HasValue)
            {
                if (completed.Value)
                {
                    // child did complete
                    // nothing to do
                }
                else
                {
                    // child did not complete, we cannot complete
                    _completed = false;
                }
            }
            else
            {
                // child did not complete, we cannot complete
                _completed = false;
            }
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("this");
        }

        public void Dispose()
        {
            EnsureNotDisposed();
            _scopeProvider.Disposing(this, _completed);
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
