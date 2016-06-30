using System;

namespace Umbraco.Core.Persistence
{
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
}
