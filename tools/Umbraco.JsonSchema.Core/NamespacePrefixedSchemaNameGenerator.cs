using NJsonSchema.Generation;

namespace Umbraco.JsonSchema.Core;

/// <inheritdoc />
internal class NamespacePrefixedSchemaNameGenerator : DefaultSchemaNameGenerator
{
    /// <inheritdoc />
    public override string Generate(Type type) => type.Namespace?.Replace(".", string.Empty) + base.Generate(type);
}
