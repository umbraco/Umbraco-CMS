using System;
using System.Collections.Generic;
using Umbraco.Core.Events;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Scoping
{
    internal class NoScope : IScope
    {
        private readonly ScopeProvider _scopeProvider;

        private UmbracoDatabase _database;
        private IList<EventMessage> _messages;

        public NoScope(ScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        public bool HasDatabase { get { return _database != null; } }

        public UmbracoDatabase Database
        {
            get { return _database ?? (_database = _scopeProvider.DatabaseFactory.CreateNewDatabase()); }
        }

        public bool HasMessages { get { return _messages != null; } }

        public IList<EventMessage> Messages
        {
            get { return _messages ?? (_messages = new List<EventMessage>()); }
        }

        public void Complete()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _scopeProvider.Disposing(this);
            GC.SuppressFinalize(this);
        }
    }
}
