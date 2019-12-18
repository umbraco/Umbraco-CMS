using System;

namespace Umbraco.Core.Persistence
{
    internal static class UmbracoDatabaseExtensions
    {
        public static UmbracoDatabase AsUmbracoDatabase(this IUmbracoDatabase database)
        {
            var asDatabase = database as UmbracoDatabase;
            if (asDatabase == null) throw new Exception("oops: database.");
            return asDatabase;
        }
    }
}
