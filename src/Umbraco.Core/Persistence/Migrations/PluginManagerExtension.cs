using System.Collections.Generic;

namespace Umbraco.Core.Persistence.Migrations
{
    internal static class PluginManagerExtension
    {
        public static IEnumerable<IMigration> FindMigrations(this PluginManager resolver)
        {
            var types = resolver.ResolveTypesWithAttribute<IMigration, MigrationAttribute>();
            return resolver.CreateInstances<IMigration>(types);
        }
    }
}