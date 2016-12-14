using System;

namespace Umbraco.Core.Persistence
{
    // fixme - move this to test or whatever!

    internal class ThreadStaticDatabaseScopeAccessor : IDatabaseScopeAccessor
    {
        [ThreadStatic]
        private static DatabaseScope _databaseScope;

        public DatabaseScope Scope
        {
            get { return _databaseScope; }
            set { _databaseScope = value; }
        }
    }
}
