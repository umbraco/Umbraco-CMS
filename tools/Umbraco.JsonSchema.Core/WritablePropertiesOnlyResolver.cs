using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Umbraco.JsonSchema.Core;

/// <inheritdoc />
internal class WritablePropertiesOnlyResolver : DefaultContractResolver
{
    /// <inheritdoc />
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        IList<JsonProperty> props = base.CreateProperties(type, memberSerialization);
        return props.Where(p => p.Writable).ToList();
    }
}
