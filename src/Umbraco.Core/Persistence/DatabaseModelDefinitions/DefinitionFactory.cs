using System;
using Umbraco.Core.Persistence.Migrations.Model;

namespace Umbraco.Core.Persistence.DatabaseModelDefinitions
{
    internal static class DefinitionFactory
    {
         public static TableDefinition GetTableDefinition(Type modelType)
         {
             var tableDefinition = new TableDefinition();
             return tableDefinition;
         }
    }
}