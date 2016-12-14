using System;
using System.Threading;

namespace Umbraco.Core.Persistence
{
    public class DatabaseScope : IDisposable
    {
        private readonly DatabaseScope _parent;
        private readonly IDatabaseScopeAccessor _accessor;
        private readonly UmbracoDatabaseFactory _factory;        
        private UmbracoDatabase _database;
        private bool _isParent;
        private int _disposed;
        private bool _disposeDatabase;

        // can specify a database to create a "substitute" scope eg for deploy - oh my

        internal DatabaseScope(IDatabaseScopeAccessor accessor, UmbracoDatabaseFactory factory, UmbracoDatabase database = null)
        {
            _accessor = accessor;
            _factory = factory;
            _database = database;
            _parent = _accessor.Scope;
            if (_parent != null) _parent._isParent = true;
            _accessor.Scope = this;
        }

        public UmbracoDatabase Database
        {
            get
            {
                if (Interlocked.CompareExchange(ref _disposed, 0, 0) != 0)
                    throw new ObjectDisposedException(null, "Cannot access a disposed object.");

                if (_database != null) return _database;
                if (_parent != null) return _parent.Database;
                _database = _factory.CreateDatabase();
                _disposeDatabase = true;
                return _database;
            }
        }

        public void Dispose()
        {
            if (_isParent)
                throw new InvalidOperationException("Cannot dispose a parent scope.");

            if (Interlocked.CompareExchange(ref _disposed, 1, 0) != 0)
                throw new ObjectDisposedException(null, "Cannot access a disposed object.");

            if (_disposeDatabase)
                _database.Dispose();

            _accessor.Scope = _parent;
            if (_parent != null) _parent._isParent = false;

            GC.SuppressFinalize(this);
        }
    }
}