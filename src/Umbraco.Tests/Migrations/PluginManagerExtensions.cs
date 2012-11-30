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
            return resolver.ResolveTypes<IMigration>();
        }

        public static IEnumerable<IMigration> FindMigrations(this PluginManager resolver)
        {
            return resolver.FindAndCreateInstances<IMigration>();
        }
    }
}