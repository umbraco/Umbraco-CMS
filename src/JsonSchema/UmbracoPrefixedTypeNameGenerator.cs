using System.Collections.Generic;
using NJsonSchema;

namespace JsonSchema
{
    public class UmbracoPrefixedTypeNameGenerator : DefaultTypeNameGenerator
    {
        private const string PREFIX = "umbraco";

        public override string Generate(NJsonSchema.JsonSchema schema, string typeNameHint, IEnumerable<string> reservedTypeNames)
        {
            return PREFIX + base.Generate(schema, typeNameHint, reservedTypeNames);
        }
    }
}
