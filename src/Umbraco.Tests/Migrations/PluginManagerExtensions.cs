using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Persistence.Migrations;

namespace Umbraco.Tests.Migrations
{
    /// <summary>
    /// Used for TypeInheritanceTest and CodeFirstTests
    /// </summary>
    internal static class PluginManagerExtensions
    {
        public static IEnumerable<Type> ResolveMigrationTypes(this PluginManager resolver)
        {
            return resolver.ResolveTypesWithAttribute<IMigration, MigrationAttribute>();
        }

        public static IEnumerable<IMigration> FindMigrations(this PluginManager resolver)
        {
            var types = resolver.ResolveTypesWithAttribute<IMigration, MigrationAttribute>();
            return resolver.CreateInstances<IMigration>(types);
        }
    }
}