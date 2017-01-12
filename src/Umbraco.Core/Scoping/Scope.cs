using System;
using System.Collections.Generic;
using Umbraco.Core.Events;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Scoping
{
    internal class Scope : IScope
    {
        private readonly ScopeProvider _scopeProvider;
        private bool? _completed;

        private UmbracoDatabase _database;
        private IList<EventMessage> _messages;

        public Scope(ScopeProvider scopeProvider, bool detachable = false)
        {
            _scopeProvider = scopeProvider;
            Detachable = detachable;
        }

        public Scope(ScopeProvider scopeProvider, Scope parent)
            : this(scopeProvider)
        {
            ParentScope = parent;
        }

        public Scope(ScopeProvider scopeProvider, NoScope noScope)
            : this(scopeProvider)
        {
            // stealing everything from NoScope
            _database = noScope.HasDatabase ? noScope.Database : null;
            _messages = noScope.HasMessages ? noScope.Messages : null;
            if (_database != null)
            {
                // must not be in a transaction
                if (_database.Connection != null)
                    throw new Exception();
            }
        }

        public bool Detachable { get; private set; }

        public Scope ParentScope { get; set; }

        public Scope OrigScope { get; set; }

        public bool HasDatabase
        {
            get { return ParentScope == null ? _database != null : ParentScope.HasDatabase; }
        }

        public UmbracoDatabase Database
        {
            get
            {
                if (ParentScope != null) return ParentScope.Database;
                if (_database != null)
                {
                    if (_database.Connection == null) // stolen from noScope
                        _database.BeginTransaction(); // a scope implies a transaction, always
                    return _database;
                }
                _database = _scopeProvider.DatabaseFactory.CreateNewDatabase();
                _database.BeginTransaction(); // a scope implies a transaction, always
                return _database;
            }
        }

        public bool HasMessages
        {
            get { return ParentScope == null ? _messages != null : ParentScope.HasMessages; }
        }

        public IList<EventMessage> Messages
        {
            get
            {
                if (ParentScope != null) return ParentScope.Messages;
                if (_messages == null)
                    _messages = new List<EventMessage>();
                return _messages;
            }
        }

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

        public void Dispose()
        {
            _scopeProvider.Disposing(this, _completed);
            GC.SuppressFinalize(this);
        }
    }
}
