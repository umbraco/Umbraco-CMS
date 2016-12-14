using System;

namespace Umbraco.Core.Persistence
{
    // fixme - move this to test or whatever!

    internal class ThreadStaticUmbracoDatabaseAccessor : IUmbracoDatabaseAccessor
    {
        [ThreadStatic]
        private static UmbracoDatabase _umbracoDatabase;

        public UmbracoDatabase UmbracoDatabase
        {
            get { return _umbracoDatabase; }
            set { _umbracoDatabase = value; }
        }
    }

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
