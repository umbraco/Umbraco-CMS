using System;
using System.Collections.Generic;
using System.Data;
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
        private readonly IsolationLevel _isolationLevel;
        private IEventManager _eventManager;
        private bool _disposed;
        private bool? _completed;

        private UmbracoDatabase _database;
        private IList<EventMessage> _messages;

        // this is v7, in v8 this has to change to RepeatableRead
        private const IsolationLevel DefaultIsolationLevel = IsolationLevel.ReadCommitted;

        // initializes a new scope
        public Scope(ScopeProvider scopeProvider, IsolationLevel isolationLevel = IsolationLevel.Unspecified, bool detachable = false)
        {
            _scopeProvider = scopeProvider;
            _isolationLevel = isolationLevel;
            Detachable = detachable;
#if DEBUG_SCOPES
            _scopeProvider.Register(this);
#endif
        }

        // initializes a new scope in a nested scopes chain, with its parent
        public Scope(ScopeProvider scopeProvider, Scope parent, IsolationLevel isolationLevel = IsolationLevel.Unspecified)
            : this(scopeProvider, isolationLevel)
        {
            ParentScope = parent;
        }

        // initializes a new scope, replacing a NoScope instance
        public Scope(ScopeProvider scopeProvider, NoScope noScope, IsolationLevel isolationLevel = IsolationLevel.Unspecified)
            : this(scopeProvider, isolationLevel)
        {
            // steal everything from NoScope
            _database = noScope.DatabaseOrNull;
            _messages = noScope.MessagesOrNull;

            // make sure the NoScope can be replaced ie not in a transaction
            if (_database != null && _database.InTransaction)
                    throw new Exception("NoScope instance is not free.");
        }

#if DEBUG_SCOPES
        private readonly Guid _instanceId = Guid.NewGuid();
        public Guid InstanceId { get { return _instanceId; } }
#endif

        // a value indicating whether the scope is detachable
        // ie whether it was created by CreateDetachedScope
        public bool Detachable { get; private set; }

        // the parent scope (in a nested scopes chain)
        public Scope ParentScope { get; set; }

        // the original scope (when attaching a detachable scope)
        public IScope OrigScope { get; set; }

        private IsolationLevel IsolationLevel
        {
            get
            {
                if (_isolationLevel != IsolationLevel.Unspecified) return _isolationLevel;
                if (ParentScope != null) return ParentScope.IsolationLevel;
                return DefaultIsolationLevel;
            }
        }

        /// <inheritdoc />
        public UmbracoDatabase Database
        {
            get
            {
                EnsureNotDisposed();
                if (ParentScope != null)
                {
                    var database = ParentScope.Database;
                    if (_isolationLevel > IsolationLevel.Unspecified && database.CurrentTransactionIsolationLevel < _isolationLevel)
                        throw new Exception("Scope requires isolation level " + _isolationLevel + ", but got " + database.CurrentTransactionIsolationLevel + " from parent.");
                    _database = database;
                }

                if (_database != null)
                {
                    // if the database has been created by a Scope instance it has to be
                    // in a transaction, however it can be a database that was stolen from
                    // a NoScope instance, in which case we need to enter a transaction, as
                    // a scope implies a transaction, always
                    if (_database.InTransaction)
                        return _database;
                }
                else
                {
                    // create a new database
                    _database = _scopeProvider.DatabaseFactory.CreateNewDatabase();
                }

                // enter a transaction, as a scope implies a transaction, always
                try
                {
                    _database.BeginTransaction(IsolationLevel);
                    return _database;
                }
                catch
                {
                    _database.Dispose();
                    _database = null;
                    throw;
                }
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

        public IEventManager EventManager
        {
            get
            {
                EnsureNotDisposed();
                return _eventManager ?? (_eventManager = new ScopedEventManager());
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

#if DEBUG_SCOPES
            _scopeProvider.Disposed(this);
#endif

            var parent = ParentScope;
            _scopeProvider.AmbientScope = parent;

            if (parent != null)
                parent.ChildCompleted(_completed);
            else
                DisposeLastScope();

            if (_eventManager != null)
            {
                _eventManager.Dispose();
            }

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
