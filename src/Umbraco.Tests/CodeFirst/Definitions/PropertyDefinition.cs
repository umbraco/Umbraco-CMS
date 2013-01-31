using System;
using Umbraco.Core.Models;

namespace Umbraco.Tests.CodeFirst.Definitions
{
    public class PropertyDefinition
    {
        public string Alias { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int Order { get; set; }

        public IDataTypeDefinition DataTypeDefinition { get; set; }

        public string PropertyGroup { get; set; }

        public bool Mandatory { get; set; }

        public string ValidationRegExp { get; set; }
    }
}