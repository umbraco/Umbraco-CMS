// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using NJsonSchema.Generation;

namespace JsonSchema
{
    internal class NamespacePrefixedSchemaNameGenerator : DefaultSchemaNameGenerator
    {
        public override string Generate(Type type) => type.Namespace?.Replace(".", string.Empty) + base.Generate(type);
    }
}
