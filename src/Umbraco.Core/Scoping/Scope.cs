using System;
using System.Collections.Generic;
using Umbraco.Core.Events;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Scoping
{
    /// <summary>
    /// Implements <see cref="IScope"/>.
    /// </summary>
    /// <remarks>Not thread-safe obviously.</remarks>
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

        public void Reset()
        {
            _completed = null;
        }

        public void ChildCompleted(bool? completed)
        {
            // if child did not complete we cannot complete
            if (completed.HasValue == false || completed.Value == false)
                _completed = false;
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

            var parent = ParentScope;
            _scopeProvider.AmbientScope = parent;

            if (parent != null)
                parent.ChildCompleted(_completed);
            else
                DisposeLastScope();

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        private void DisposeLastScope()
        {
            // note - messages
            // at the moment we are totally not filtering the messages based on completion
            // status, so whether the scope is committed or rolled back makes no difference

            if (_database == null) return;

            try
            {
                if (_completed.HasValue && _completed.Value)
                    _database.CompleteTransaction();
                else
                    _database.AbortTransaction();
            }
            finally
            {
                _database.Dispose();
                _database = null;
            }
        }
    }
}
