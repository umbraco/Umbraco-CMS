using System;
using NJsonSchema.Generation;

namespace JsonSchema
{
    public class NamespacePrefixedSchemaNameGenerator : DefaultSchemaNameGenerator
    {
        public override string Generate(Type type)
        {
            return type.Namespace.Replace(".", String.Empty) + base.Generate(type);
        }
    }
}
