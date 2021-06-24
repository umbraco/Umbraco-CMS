using System.Collections.Generic;
using NJsonSchema;

namespace JsonSchema
{
    /// <summary>
    /// Prefixes all definitions with the injected prefix.
    /// </summary>
    public class PrefixedTypeNameGenerator : DefaultTypeNameGenerator
    {
        private readonly string _definitionPrefix;

        /// <summary>
        /// Creates a new instance of <see cref="PrefixedTypeNameGenerator"/>.
        /// </summary>
        /// <param name="definitionPrefix">The prefix to use.</param>
        public PrefixedTypeNameGenerator(string definitionPrefix) => _definitionPrefix = definitionPrefix;

        /// <inheritdoc />
        public override string Generate(NJsonSchema.JsonSchema schema, string typeNameHint, IEnumerable<string> reservedTypeNames)
            => $"{_definitionPrefix}{base.Generate(schema, typeNameHint, reservedTypeNames)}";
    }
}
